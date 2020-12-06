using AivyData.Entities;
using AivyData.Enums;
using AivyDofus.Handler;
using AivyDofus.IO;
using AivyDofus.Protocol.Buffer;
using AivyDofus.Protocol.Elements;
using AivyDofus.Proxy.Handlers;
using AivyDomain.Callback.Client;
using AivyDomain.Callback.Proxy;
using AivyDomain.Repository.Client;
using AivyDomain.UseCases.Client;
using Newtonsoft.Json.Serialization;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace AivyDofus.Proxy.Callbacks
{
    public class DofusProxyClientReceiveCallback : AbstractClientReceiveCallback
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected MessageDataBufferReader _data_buffer_reader;
        protected MessageBufferWriter _buffer_writer;

        protected MessageHandler<ProxyHandlerAttribute> _handler;
        protected BigEndianReader _reader;

        public readonly ProxyEntity _proxy;

        public override uint InstanceId => _instance_id.HasValue ? _instance_id.Value : base.InstanceId;

        public DofusProxyClientReceiveCallback(ClientEntity client,
                                               ClientEntity remote,
                                               ClientRepository repository,
                                               ClientCreatorRequest creator,
                                               ClientLinkerRequest linker,
                                               ClientConnectorRequest connector,
                                               ClientDisconnectorRequest disconnector,
                                               ClientSenderRequest sender,
                                               ProxyEntity proxy, ProxyTagEnum tag = ProxyTagEnum.UNKNOW)
            : base(client, remote, repository, creator, linker, connector, disconnector, sender, tag)
        {
            if (tag is ProxyTagEnum.UNKNOW) throw new ArgumentNullException(nameof(tag));

            _proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
        }

        protected override void _constructor_handled()
        {
            _handler = new MessageHandler<ProxyHandlerAttribute>();

            _rcv_action += OnReceive;
            _position = 0;
        }

        private ushort? _current_header { get; set; } = null;
        private uint? _instance_id { get; set; } = null;
        private int? _length { get; set; } = null;
        private byte[] _data { get; set; } = null;

        private int _message_id => _current_header.HasValue ? _current_header.Value >> 2 : 0;
        private int _static_header => _current_header.HasValue ? _current_header.Value & 3 : 0;

        private void _clear()
        {
            _current_header = null;
            _instance_id = null;
            _length = null;
            _data = null;
        }

        private long _position { get; set; } = 0;
        /// <summary>
        /// thx to Hitman for this implementation made by ToOnS
        /// </summary>
        /// <param name="stream"></param>
        private readonly object _rcv_locker = new object();
        protected virtual void OnReceive(MemoryStream stream)
        {
            lock (_rcv_locker)
            {
                if (_reader is null) _reader = new BigEndianReader();
                if (stream.Length > 0)
                {
                    _reader.Add(stream.ToArray(), 0, (int)stream.Length);
                }

                byte[] full_data = _reader.Data;
                while(_position < full_data.Length)
                {
                    if (full_data.Length - _position < 2)
                        break;
                    long start_pos = _position;

                    _current_header = (ushort)((full_data[_position] * 256) + full_data[_position + 1]);
                    _position += sizeof(ushort);

                    if (_tag == ProxyTagEnum.Client)
                    {
                        _instance_id = (uint)((full_data[_position] * 256 * 256 * 256) + (full_data[_position + 1] * 256 * 256) + (full_data[_position + 2] * 256) + full_data[_position + 3]);
                        _position += sizeof(uint);
                    }

                    if (full_data.Length - _position < _static_header)
                    {
                        _position = start_pos;
                        break;
                    }

                    switch (_static_header)
                    {
                        case 0: _length = 0; break;
                        case 1: _length = full_data[_position]; break;
                        case 2: _length = (ushort)((full_data[_position] * 256) + full_data[_position + 1]); break;
                        case 3: _length = (full_data[_position] * 256 * 256) + (full_data[_position + 1] * 256) + full_data[_position + 2]; break;
                    }
                    _position += _static_header;

                    _position += _length ?? 0;
                    
                    if (_position <= full_data.Length)
                    {
                        byte[] game_packet = new byte[_position - start_pos];
                        _data = new byte[_length ?? 0];
                        Array.Copy(full_data, start_pos, game_packet, 0, game_packet.Length);
                        Array.Copy(game_packet, game_packet.Length - (_length ?? 0), _data, 0, _data.Length);

                        NetworkElement _element = BotofuProtocolManager.Instance[ProxyCallbackTypeEnum.Dofus2][ProtocolKeyEnum.Messages, x => x.protocolID == _message_id];
                        if (_tag == ProxyTagEnum.Client)
                        {
                            // rmv element from not game socket
                            if (_instance_id > _proxy.GLOBAL_INSTANCE_ID * 2)
                            {
                                _element = null;
                            }
                            else
                            {
                                _proxy.LAST_CLIENT_INSTANCE_ID = _instance_id.Value;
                                _proxy.MESSAGE_RECEIVED_FROM_LAST = 0;
                            }
                        }
                        else
                        {
                            if (_message_id == StaticValues.RAW_DATA_MSG_RCV_ID) // rdm
                            {
                                _element = BotofuProtocolManager.Instance[ProxyCallbackTypeEnum.Dofus2][ProtocolKeyEnum.Messages, x => x.name == "RawDataMessage"];
                            }
                            _proxy.MESSAGE_RECEIVED_FROM_LAST++;
                        }

                        if (_element is null)
                        {
                            _client_sender.Handle(_remote, game_packet);
                        }
                        else
                        {
                            logger.Info($"[{_tag}] {_element.BasicString} - (c:{_proxy.GLOBAL_INSTANCE_ID}) (b_pos:{_position})");
                            using (_data_buffer_reader = new MessageDataBufferReader(_element))
                            {
                                using (BigEndianReader big_data_reader = new BigEndianReader(_data))
                                {
                                    using (NetworkContentElement data_content = _data_buffer_reader.Parse(big_data_reader))
                                    {
                                        if (_handler.Handle(this, _element, data_content).Result)
                                        {
                                            _client_sender.Handle(_remote, game_packet);
                                        }
                                        else
                                        {
                                            logger.Info($"{_element.BasicString} not forwarded");
                                        }
                                    }
                                }
                            }
                        }
                        _clear();

                        if(_position == full_data.Length)
                        {
                            _reader.Dispose();
                            _reader = new BigEndianReader();
                            _position = 0;
                            break;// or reasign full_data
                        }
                    }
                    else
                    {
                        _position = start_pos;
                        _clear();
                        break;
                    }
                }
            }
        }
    }
}


