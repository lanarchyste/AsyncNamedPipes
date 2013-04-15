using System;
using System.IO.Pipes;
using AsyncNamedPipes.Message;

namespace AsyncNamedPipes
{
    public class NamedPipeConnection : NamedPipeBase
    {
        private readonly PipeStream _pipeStream;
        private readonly object _pipeLock = new object();

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
            if (!_pipeStream.IsConnected)
                return;

            var buffer = new byte[BufferLength];
            _pipeStream.BeginRead(buffer, 0, BufferLength, EndReceiveMessage, buffer);
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
