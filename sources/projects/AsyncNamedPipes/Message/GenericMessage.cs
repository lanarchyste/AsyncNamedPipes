using System;

namespace AsyncNamedPipes.Message
{
    [Serializable]
    public class GenericMessage : IMessage
    {
        public GenericMessage(string sender, string receiver, DateTime messageDateTime, Type messageType)
        {
            Sender = sender;
            Receiver = receiver;
            MessageDateTime = messageDateTime;
            MessageType = messageType;
        }

        public string Sender { get; private set; }

        public string Receiver { get; private set; }

        public DateTime MessageDateTime { get; private set; }

        public Type MessageType { get; private set; }
    }
}
