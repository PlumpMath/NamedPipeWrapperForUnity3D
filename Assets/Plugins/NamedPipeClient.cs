using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using NamedPipeWrapper.IO;
using NamedPipeWrapper.Threading;

namespace NamedPipeWrapper
{
    public class NamedPipeClient<TRead, TWrite>
        where TRead : class
        where TWrite : class
    {
        //public bool AutoReconnect { get; set; }

        public event ConnectionMessageEventHandler<TRead, TWrite> ServerMessage;
        public event ConnectionEventHandler<TRead, TWrite> Disconnected;
        public event PipeExceptionEventHandler Error;

        private readonly string _pipeName;
        private NamedPipeConnection<TRead, TWrite> _connection;

        private readonly AutoResetEvent _connected = new AutoResetEvent(false);
        private readonly AutoResetEvent _disconnected = new AutoResetEvent(false);

        private volatile bool _closedExplicitly;
        private string _serverName { get; set; }
        public NamedPipeClient(string pipeName, string serverName)
        {
            _pipeName = pipeName;
            _serverName = serverName;
            //AutoReconnect = true;
        }

        public void Start()
        {
            _closedExplicitly = false;
            var worker = new ThreadingWorker();
            worker.Error += OnError;
            worker.DoWork(ListenSync);
        }

        public void PushMessage(TWrite message)
        {
            if (_connection != null)
                _connection.PushMessage(message);
        }

        public void Stop()
        {
            _closedExplicitly = true;
            if (_connection != null)
                _connection.Close();
        }

        #region Wait for connection/disconnection
        public void WaitForConnection()
        {
            _connected.WaitOne();
        }

        public void WaitForConnection(int millisecondsTimeout)
        {
            _connected.WaitOne(millisecondsTimeout);
        }

        public void WaitForConnection(TimeSpan timeout)
        {
            _connected.WaitOne(timeout);
        }

        public void WaitForDisconnection()
        {
            _disconnected.WaitOne();
        }

        public void WaitForDisconnection(int millisecondsTimeout)
        {
            _disconnected.WaitOne(millisecondsTimeout);
        }

        public void WaitForDisconnection(TimeSpan timeout)
        {
            _disconnected.WaitOne(timeout);
        }

        #endregion

        #region Private methods
        private void ListenSync()
        {
            try
            {
                // Get the name of the data pipe that should be used from now on by this NamedPipeClient
                var handshake = PipeClientFactory.Connect<string, string>(_pipeName, _serverName);
                var dataPipeName = handshake.ReadObject();
                handshake.Close();
                var dataPipe = PipeClientFactory.CreateAndConnectPipe(dataPipeName, _serverName);
                // Connect to the actual data pipe
                _connection = ConnectionFactory.CreateConnection<TRead, TWrite>(dataPipe);

                // Create a Connection object for the data pipe
                _connection.Disconnected += OnDisconnected;
                _connection.ReceiveMessage += OnReceiveMessage;
                _connection.Error += ConnectionOnError;
                _connection.Open();
                _connected.Set();
            }
            catch (Exception e)
            {
                OnError(e);
            }
        }

        private void OnDisconnected(NamedPipeConnection<TRead, TWrite> connection)
        {
            if (Disconnected != null)
                Disconnected(connection);

            _disconnected.Set();

            if(!_closedExplicitly)
            {
                Start();
            }
        }

        private void OnReceiveMessage(NamedPipeConnection<TRead, TWrite> connection, TRead message)
        {
            if (ServerMessage != null)
                ServerMessage(connection, message);
        }

        /// <summary>
        ///     Invoked on the UI thread.
        /// </summary>
        private void ConnectionOnError(NamedPipeConnection<TRead, TWrite> connection, Exception exception)
        {
            OnError(exception);
        }

        /// <summary>
        ///     Invoked on the UI thread.
        /// </summary>
        /// <param name="exception"></param>
        private void OnError(Exception exception)
        {
            if (Error != null)
                Error(exception);
        }

        #endregion
    }

    static class PipeClientFactory
    {
        public static PipeStreamWrapper<TRead, TWrite> Connect<TRead, TWrite>(string pipeName, string serverName)
            where TRead : class
            where TWrite : class
        {
            return new PipeStreamWrapper<TRead, TWrite>(CreateAndConnectPipe(pipeName, serverName));
        }

        public static NamedPipeClientStream CreateAndConnectPipe(string pipeName, string serverName)
        {
            var pipe = CreatePipe(pipeName, serverName);
            pipe.Connect();
            return pipe;
        }

        private static NamedPipeClientStream CreatePipe(string pipeName, string serverName)
        {
            return new NamedPipeClientStream(serverName, pipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
        }
    }
}
