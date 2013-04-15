using System;
using System.IO.Pipes;

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

                BeginRead();
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

        public override void SendMessage(byte[] message)
        {
            lock (_pipeLock)
            {
                if (!_pipeStream.IsConnected)
                    return;

                if (message.Length <= 0)
                    return;

                _pipeStream.BeginWrite(message, 0, message.Length, EndWrite, null);
                _pipeStream.Flush();
            }
        }

        private void EndWrite(IAsyncResult result)
        {
            lock (_pipeLock)
            {
                _pipeStream.EndWrite(result);
                _pipeStream.Flush();
            }
        }

        private void BeginRead()
        {
            if (!_pipeStream.IsConnected)
                return;

            var buffer = new byte[BufferLength];
            _pipeStream.BeginRead(buffer, 0, BufferLength, EndRead, buffer);
        }

        private void EndRead(IAsyncResult result)
        {
            var buffer = (byte[])result.AsyncState;

            var length = _pipeStream.EndRead(result);
            if (length <= 0)
                return;

            var destinationArray = new byte[length];
            Array.Copy(buffer, 0, destinationArray, 0, length);

            OnMessageReceived(new MessageEventArgs(destinationArray));

            lock (_pipeLock)
                BeginRead();
        }
    }
}
