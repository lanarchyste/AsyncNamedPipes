using System;
using AsyncNamedPipes.Message;

namespace AsyncNamedPipes
{
    [Serializable]
    public class MessageEventArgs
    {
        private readonly IMessage _message;

        public MessageEventArgs(IMessage message)
        {
            _message = message;
        }

        public IMessage Message
        {
            get { return _message; }
        }
    }
}
