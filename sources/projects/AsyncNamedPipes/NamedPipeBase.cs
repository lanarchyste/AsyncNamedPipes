using System;
using AsyncNamedPipes.Message;

namespace AsyncNamedPipes
{
    public abstract class NamedPipeBase : IDisposable
    {
        public static readonly int BufferLength = 65536;

        private readonly object _eventLock = new object();
        private MessageEventHandler _messageReceived;

        protected NamedPipeBase(string pipeName)
        {
            PipeName = pipeName;
        }

        ~NamedPipeBase()
        {
            Dispose(false);
        }

        public string PipeName { get; private set; }

        public void Dispose()
        {
            Dispose(true);
        }

        public event MessageEventHandler MessageReceived
        {
            add
            {
                lock (_eventLock)
                {
                    _messageReceived = (MessageEventHandler)Delegate.Combine(_messageReceived, value);
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    _messageReceived = (MessageEventHandler)Delegate.Remove(_messageReceived, value);
                }
            }
        }

        public virtual void Disconnect()
        {
            lock (_eventLock)
            {
                _messageReceived = null;
            }
        }

        public abstract void SendMessage(IMessage message);

        protected virtual void OnMessageReceived(MessageEventArgs args)
        {
            lock (_eventLock)
            {
                if (_messageReceived != null)
                    _messageReceived(this, args);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                GC.SuppressFinalize(this);
        }
    }
}
