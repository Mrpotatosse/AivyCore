using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AivyData.Entities
{
    public class ClientEntity
    {
        public Socket Socket { get; set; }
        public IPEndPoint RemoteIp { get; set; }
        public int ReceiveBufferLength { get; set; }

        public MemoryStream ReceiveBuffer { get; set; }
        public bool IsGameClient { get; set; }
        public string CurrentToken { get; set; }

        public bool IsRunning
        {
            get
            {
                try
                {
                    if (Socket != null && Socket.Connected)
                    {
                        try
                        {
                            if (Socket.Poll(1000, SelectMode.SelectRead))
                            {
                                if (Socket.Available == 0)
                                {
                                    return false;
                                }
                            }
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                    return false;
                }
                catch
                {
                    return false;
                }
            }
        }

        public override string ToString()
        {
            return $"{Socket?.RemoteEndPoint} r_state : {IsRunning}";
        }
    }
}
