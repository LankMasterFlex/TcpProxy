using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TcpProxy
{
    internal sealed class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) => File.WriteAllText("Exceptions.txt", e.ExceptionObject.ToString());

            if (args.Length < 3)
            {
                Console.WriteLine("Usage : TcpProxy <local port> <remote host> <remote port>");
                return;
            }

            int localPort = Convert.ToInt32(args[0]);
            string remoteHost = args[1];
            int remotePort = Convert.ToInt32(args[2]);

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
