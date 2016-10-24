using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NamedPipeWrapper.IO
{
    public class PipeStreamWrapper<TReadWrite> : PipeStreamWrapper<TReadWrite, TReadWrite>
        where TReadWrite : class {
      
        public PipeStreamWrapper(PipeStream stream) : base(stream)
        {
        }
    }

    public class PipeStreamWrapper<TRead, TWrite>
        where TRead : class
        where TWrite : class
    {
        public PipeStream BaseStream { get; private set; }

        public bool IsConnected
        {
            get { return BaseStream.IsConnected && _reader.IsConnected; }
        }

        public bool CanRead
        {
            get { return BaseStream.CanRead; }
        }
        public bool CanWrite
        {
            get { return BaseStream.CanWrite; }
        }

        private readonly PipeStreamReader<TRead> _reader;
        private readonly PipeStreamWriter<TWrite> _writer;

        public PipeStreamWrapper(PipeStream stream)
        {
            BaseStream = stream;
            _reader = new PipeStreamReader<TRead>(BaseStream);
            _writer = new PipeStreamWriter<TWrite>(BaseStream);
        }

        public TRead ReadObject()
        {
            return _reader.ReadObject();
        }
        public void WriteObject(TWrite obj)
        {
            _writer.WriteObject(obj);
        }

        public void WaitForPipeDrain()
        {
            _writer.WaitForPipeDrain();
        }

        /// <summary>
        ///     Closes the current stream and releases any resources (such as sockets and file handles) associated with the current stream.
        /// </summary>
        public void Close()
        {
            BaseStream.Close();
        }
    }
}
