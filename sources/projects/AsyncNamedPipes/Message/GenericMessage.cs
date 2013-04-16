using System;

namespace AsyncNamedPipes.Message
{
    [Serializable]
    public class GenericMessage : IMessage
    {
        public GenericMessage(string sender, string receiver, DateTime messageDateTime, Type messageType, object args)
        {
            Sender = sender;
            Receiver = receiver;
            MessageDateTime = messageDateTime;
            MessageType = messageType;
            Args = args;
        }

        public string Sender { get; private set; }

        public string Receiver { get; private set; }

        public DateTime MessageDateTime { get; private set; }

        public Type MessageType { get; private set; }

        public object Args { get; private set; }

        public override string ToString()
        {
            return MessageDateTime.ToShortDateString() + " - " + Args;
        }
    }
}
