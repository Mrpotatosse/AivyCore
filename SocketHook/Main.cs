﻿using EasyHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static EasyHook.RemoteHooking;
using static SocketHook.NativeSocketMethods;

namespace SocketHook
{
    public class Main : IEntryPoint
    {
        private HookInterface _interface;
        private LocalHook _connectHook;
        private ushort _redirectionPort;

        public Main(IContext context, string channelName, int redirectionPort)
        {
            _interface = IpcConnectClient<HookInterface>(channelName);
            _redirectionPort = (ushort)redirectionPort;

            _interface.Ping();
        }

        public void Run(IContext context, string channelName, int redirectionPort)
        {
            _interface.NotifyInstalled(Process.GetCurrentProcess().ProcessName);

            try
            {
                _connectHook = LocalHook.Create(
                    LocalHook.GetProcAddress("Ws2_32.dll", "connect"),
                    new WinsockConnectDelegate(_onConnect), this);

                _connectHook.ThreadACL.SetExclusiveACL(new[] { 0 });
            }
            catch (Exception ex) { _interface.Error(ex); }

            WakeUpProcess();

            try
            {
                while (true) 
                {
                    Thread.Sleep(500);
                    _interface.Ping();
                }
            }
            catch
            {
                _connectHook.Dispose();

                LocalHook.Release();
            }
        }

        private int _onConnect(IntPtr socket, IntPtr address, int addrSize)
        {
            var structure = Marshal.PtrToStructure<sockaddr_in>(address);
            var ipAddress = new IPAddress(structure.sin_addr.S_addr);
            var port = structure.sin_port;

            if(_interface.LocalPortWhiteList().Contains(htons(port)) && 
                ipAddress.ToString() is string str_ip && 
                (str_ip == "127.0.0.1" || str_ip == "0.0.0.0" || str_ip == "")) // if local
            {
                return connect(socket, address, addrSize);
            }

            //_interface.Message($"Connection attempt at {ipAddress}:{htons(port)}, redirecting to 127.0.0.1:{_redirectionPort}...");

            _interface.IpRedirected(new IPEndPoint(ipAddress, htons(port)), Process.GetCurrentProcess().Id, _redirectionPort);

            var strucPtr = Marshal.AllocHGlobal(addrSize);
            var struc = new sockaddr_in
            {
                sin_addr = { S_addr = inet_addr(_interface.GetRedirectedIp()) },
                sin_port = htons(_redirectionPort),
                sin_family = (short)AddressFamily.InterNetworkv4,
            };

            Marshal.StructureToPtr(struc, strucPtr, true);
            return connect(socket, strucPtr, addrSize);
        }
    }
}
