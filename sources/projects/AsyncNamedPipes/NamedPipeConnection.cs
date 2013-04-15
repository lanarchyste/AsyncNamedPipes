using System;
using System.IO.Pipes;
using AsyncNamedPipes.Event;
using AsyncNamedPipes.Message;

namespace AsyncNamedPipes
{
    public class NamedPipeConnection : NamedPipeBase
    {
        private readonly PipeStream _pipeStream;
        private readonly object _pipeLock = new object();
        private PipeDisconnectedEventHandler _clientDisconnected;

        public NamedPipeConnection(PipeStream pipeStream, string pipeName) : base(pipeName)
        {
            _pipeStream = pipeStream;

            ReceiveMessage();
        }

        ~NamedPipeConnection()
        {
            Dispose(false);
        }

        public bool IsConnected
        {
            get { return _pipeStream.IsConnected; }
        }

        public event PipeDisconnectedEventHandler ClientDisconnected
        {
            add
            {
                lock (_pipeLock)
                    _clientDisconnected = (PipeDisconnectedEventHandler)Delegate.Combine(_clientDisconnected, value);
            }
            remove
            {
                lock (_pipeLock)
                    _clientDisconnected = (PipeDisconnectedEventHandler)Delegate.Remove(_clientDisconnected, value);
            }
        }

        public override void Disconnect()
        {
            lock (_pipeLock)
            {
                base.Disconnect();
                _pipeStream.Close();
            }
        }

        public override void SendMessage(IMessage message)
        {
            lock (_pipeStream)
            {
                if (!_pipeStream.IsConnected)
                    return;

                if (message == null)
                    return;

                var messageSerialized = MessageSerializer.SerializeMessage(message);
                if (messageSerialized.Length <= 0)
                    return;

                _pipeStream.BeginWrite(messageSerialized, 0, messageSerialized.Length, EndSendMessage, null);
                _pipeStream.Flush();
            }
        }

        private void EndSendMessage(IAsyncResult result)
        {
            lock (_pipeLock)
            {
                _pipeStream.EndWrite(result);
                _pipeStream.Flush();
            }
        }

        private void ReceiveMessage()
        {
            var isConnected = _pipeStream.IsConnected;
            if (isConnected)
            {
                try
                {
                    var buffer = new byte[BufferLength];
                    _pipeStream.BeginRead(buffer, 0, BufferLength, EndReceiveMessage, buffer);
                }
                catch
                {
                    isConnected = false;
                }
            }

            if (!isConnected)
            {
                _pipeStream.Close();
                lock (_pipeLock)
                {
                    if (_clientDisconnected != null)
                        _clientDisconnected(this, new PipeDisconnectedEventArgs(this));
                }
            }
        }

        private void EndReceiveMessage(IAsyncResult result)
        {
            var buffer = (byte[])result.AsyncState;

            var length = _pipeStream.EndRead(result);
            if (length <= 0)
                return;

            var destinationArray = new byte[length];
            Array.Copy(buffer, 0, destinationArray, 0, length);

            var messageDeserialized = MessageSerializer.DeserializeMessage(destinationArray);
            OnMessageReceived(new MessageEventArgs(messageDeserialized));

            lock (_pipeLock)
                ReceiveMessage();
        }
    }
}
