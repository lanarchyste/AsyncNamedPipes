using System;

namespace AsyncNamedPipes.Message
{
    public interface IMessage
    {
        string Sender { get; }
        string Receiver { get; }
        DateTime MessageDateTime { get; }
        Type MessageType { get; }
    }
}
