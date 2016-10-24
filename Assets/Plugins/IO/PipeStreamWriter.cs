using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace NamedPipeWrapper.IO
{
   public class PipeStreamWriter<T> where T : class
    {
      public PipeStream BaseStream { get; private set; }

        private readonly BinaryFormatter _binaryFormatter = new BinaryFormatter();

        public PipeStreamWriter(PipeStream stream)
        {
            BaseStream = stream;
        }

        #region Private stream writers

        /// <exception cref="SerializationException">An object in the graph of type parameter <typeparamref name="T"/> is not marked as serializable.</exception>
        private byte[] Serialize(T obj)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    _binaryFormatter.Serialize(memoryStream, obj);
                    return memoryStream.ToArray();
                }
            }
            catch
            {
                //if any exception in the serialize, it will stop named pipe wrapper, so there will ignore any exception.
                return null;
            }
        }

        private void WriteLength(int len)
        {
            var lenbuf = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(len));
            BaseStream.Write(lenbuf, 0, lenbuf.Length);
        }

        private void WriteObject(byte[] data)
        {
            BaseStream.Write(data, 0, data.Length);
        }

        private void Flush()
        {
            BaseStream.Flush();
        }

        #endregion

       public void WriteObject(T obj)
        {
            var data = Serialize(obj);
            WriteLength(data.Length);
            WriteObject(data);
            Flush();
        }

        public void WaitForPipeDrain()
        {
            BaseStream.WaitForPipeDrain();
        }
    }
}