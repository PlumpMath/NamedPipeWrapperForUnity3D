using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using NamedPipeWrapper.IO;
using NamedPipeWrapper.Threading;

namespace NamedPipeWrapper
{
    public class NamedPipeConnection<TRead, TWrite>
        where TRead : class
        where TWrite : class
    {
        public readonly int Id;
        public string Name { get; private set; }
        public bool IsConnected { get { return _streamWrapper.IsConnected; } }

        public event ConnectionEventHandler<TRead, TWrite> Disconnected;
        public event ConnectionMessageEventHandler<TRead, TWrite> ReceiveMessage;
        public event ConnectionExceptionEventHandler<TRead, TWrite> Error;

        private readonly PipeStreamWrapper<TRead, TWrite> _streamWrapper;
        private readonly AutoResetEvent _writeSignal = new AutoResetEvent(false);

        private readonly Queue<TWrite> _writeQueue = new Queue<TWrite>();

        private bool _notifiedSucceeded;

        internal NamedPipeConnection(int id, string name, PipeStream serverStream)
        {
            Id = id;
            Name = name;
            _streamWrapper = new PipeStreamWrapper<TRead, TWrite>(serverStream);
        }

        public void Open()
        {
            var readWorker = new ThreadingWorker();
            readWorker.Succeeded += OnSucceeded;
            readWorker.Error += OnError;
            readWorker.DoWork(ReadPipe);

            var writeWorker = new ThreadingWorker();
            writeWorker.Succeeded += OnSucceeded;
            writeWorker.Error += OnError;
            writeWorker.DoWork(WritePipe);
        }

        public void PushMessage(TWrite message)
        {
            _writeQueue.Enqueue(message);
            _writeSignal.Set();
        }

        public void Close()
        {
            CloseImpl();
        }

        private void CloseImpl()
        {
            _streamWrapper.Close();
            _writeSignal.Set();
        }

        private void OnSucceeded()
        {
            // Only notify observers once
            if (_notifiedSucceeded)
                return;

            _notifiedSucceeded = true;

            if (Disconnected != null)
                Disconnected(this);
        }

        private void OnError(Exception exception)
        {
            if (Error != null)
                Error(this, exception);
        }

        private void ReadPipe()
        {
            while (IsConnected && _streamWrapper.CanRead)
            {
                try
                {
                    var obj = _streamWrapper.ReadObject();
                    if (obj == null)
                    {
                        CloseImpl();
                        return;
                    }
                    if (ReceiveMessage != null)
                        ReceiveMessage(this, obj);
                }
                catch
                {
                    //we must igonre exception, otherwise, the namepipe wrapper will stop work.
                }
            }

        }

        private void WritePipe()
        {

            while (IsConnected && _streamWrapper.CanWrite)
            {
                try
                {
                    _writeSignal.WaitOne();
                    while (_writeQueue.Count > 0)
                    {
                        _streamWrapper.WriteObject(_writeQueue.Dequeue());
                        _streamWrapper.WaitForPipeDrain();
                    }
                }
                catch
                {
                    //we must igonre exception, otherwise, the namepipe wrapper will stop work.
                }
            }

        }
    }

    static class ConnectionFactory
    {
        private static int _lastId;

        public static NamedPipeConnection<TRead, TWrite> CreateConnection<TRead, TWrite>(PipeStream pipeStream)
            where TRead : class
            where TWrite : class
        {
            return new NamedPipeConnection<TRead, TWrite>(++_lastId, "Client " + _lastId, pipeStream);
        }
    }

    public delegate void ConnectionEventHandler<TRead, TWrite>(NamedPipeConnection<TRead, TWrite> connection)
        where TRead : class
        where TWrite : class;

    public delegate void ConnectionMessageEventHandler<TRead, TWrite>(NamedPipeConnection<TRead, TWrite> connection, TRead message)
        where TRead : class
        where TWrite : class;

    public delegate void ConnectionExceptionEventHandler<TRead, TWrite>(NamedPipeConnection<TRead, TWrite> connection, Exception exception)
        where TRead : class
        where TWrite : class;
}
