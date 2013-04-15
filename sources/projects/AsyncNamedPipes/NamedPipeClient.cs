using System;
using System.IO.Pipes;
using AsyncNamedPipes.Event;
using AsyncNamedPipes.Message;

namespace AsyncNamedPipes
{
    public class NamedPipeClient : NamedPipeBase
    {
        private readonly object _pipeLock = new object();
        private PipeStream _pipeStream;

        public NamedPipeClient(string pipeName)
            : base(pipeName)
        {
        }

        ~NamedPipeClient()
        {
            Dispose(false);
        }

        public void Connect(int timeout)
        {
            lock (_pipeLock)
            {
                _pipeStream = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
                ((NamedPipeClientStream)_pipeStream).Connect(timeout);

                _pipeStream.ReadMode = PipeTransmissionMode.Message;

                ReceiveMessage();
            }
        }

        public bool IsConnected
        {
            get
            {
                lock (_pipeLock)
                    return _pipeStream.IsConnected;
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
            lock (_pipeLock)
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
