using System;
using System.Collections.Concurrent;

namespace TcpProxy
{
    internal sealed class BufferPool
    {
        private int m_bufferSize;
        private ConcurrentBag<byte[]> m_objects;

        public BufferPool(int bufferSize)
        {
            m_bufferSize = bufferSize;
            m_objects = new ConcurrentBag<byte[]>();
        }

        public byte[] Get()
        {
            byte[] buffer;

            if (m_objects.TryTake(out buffer) == false)
                buffer = new byte[m_bufferSize];

            return buffer;
        }

        public void Put(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            if (buffer.Length != m_bufferSize)
                throw new ArgumentOutOfRangeException("buffer");

            Array.Clear(buffer, 0, m_bufferSize);
            m_objects.Add(buffer);
        }
    }
}
