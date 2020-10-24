using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AivyData.Entities
{
    public class OneLifeClientEntity : IDisposable
    {
        readonly Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        readonly IPEndPoint _ip;
        readonly string _message;

        public OneLifeClientEntity(IPEndPoint ip, string message)
        {
            _ip = ip ?? throw new ArgumentNullException(nameof(ip));
            _message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public void _connect_and_send()
        {
            _socket.Connect(_ip);

            while (!_socket.Connected)
            {

            }

            _socket.Send(Encoding.ASCII.GetBytes(_message));
        }

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
