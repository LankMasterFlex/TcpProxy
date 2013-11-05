﻿using System;
using System.Net.Sockets;

namespace TcpProxy
{
    internal sealed class Session : IDisposable
    {
        private readonly Socket m_socket;
        private readonly byte[] m_buffer;

        private bool m_disposed;

        public byte[] Buffer
        {
            get
            {
                return m_buffer;
            }
        }

        public bool Disposed
        {
            get
            {
                return m_disposed;
            }
        }

        public Action<byte[],int> OnDataReceived { get; set; }
        public Action OnDisconnected { get; set; }

        public Session(Socket socket)
        {
            m_socket = socket;
            m_buffer = TcpProxy.BufferPool.Get();
            m_disposed = false;
        }

        public void Receive()
        {
            if (m_disposed) { return; }

            SocketError outError = SocketError.Success;

            m_socket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, out outError, EndReceive, null);

            if(outError != SocketError.Success)
                Close();
        }

        private void EndReceive(IAsyncResult iar)
        {
            if (m_disposed) { return; }

            SocketError outError;

            int size = m_socket.EndReceive(iar, out outError);

            if (size == 0 || outError != SocketError.Success)
            {
                Close();
                return;
            }

            if (OnDataReceived != null)
                OnDataReceived(m_buffer,size);

            Receive();
        }

        public void Send(byte[] buffer,int length)
        {
            if (m_disposed) { return; }

            int offset = 0;

            while (offset < length)
            {
                SocketError outError = SocketError.Success;

                int size = m_socket.Send(buffer, offset, length - offset, SocketFlags.None, out outError);

                if (size == 0 || outError != SocketError.Success)
                {
                    Close();
                    return;
                }

                offset += size;
            }
        }

        public void Close()
        {
            if (!m_disposed)
            {
                m_disposed = true;

                m_socket.Shutdown(SocketShutdown.Both);
                m_socket.Close();

                TcpProxy.BufferPool.Put(m_buffer);

                if (OnDisconnected != null)
                    OnDisconnected();

                OnDataReceived = null;
                OnDisconnected = null;
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
