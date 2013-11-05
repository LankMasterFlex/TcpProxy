using System;
using System.Net;
using System.Net.Sockets;

namespace TcpProxy
{
    internal sealed class TcpProxy
    {
        public static BufferPool BufferPool { get; private set; }

        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage : TcpProxy <local port> <remote host> <remote port>");
                return;
            }

            int localPort = Convert.ToInt32(args[0]);
            string remoteHost = args[1];
            int remotePort = Convert.ToInt32(args[2]);

            BufferPool = new BufferPool(1024);

            var listener = new TcpListener(IPAddress.Any, localPort);
            listener.Start();

            Console.Title = string.Concat("TcpProxy : ", remoteHost, ':', remotePort);
            Console.WriteLine("Listening on port {0} for connections", localPort);

            while (true)
            {
                var socket = listener.AcceptSocket();
                new Redirector(socket, remoteHost, remotePort);
            }
        }
    }
}
