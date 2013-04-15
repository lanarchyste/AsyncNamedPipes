using System;

namespace AsyncNamedPipes
{
    [Serializable]
    public class MessageEventArgs
    {
        private readonly byte[] _message;

        public MessageEventArgs(byte[] message)
        {
            _message = message;
        }

        public byte[] Message
        {
            get { return _message; }
        }
    }
}
