using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using AsyncNamedPipes.Message;

namespace AsyncNamedPipes
{
    public static class MessageSerializer
    {
        public static byte[] SerializeMessage(IMessage message)
        {
            var ms = new MemoryStream();
            var formatter = new BinaryFormatter();
            
            formatter.Serialize(ms, message);
            return ms.ToArray();
        }

        public static IMessage DeserializeMessage(byte[] bMessage)
        {
            var ms = new MemoryStream(bMessage);
            var formatter = new BinaryFormatter();
            
            var message = formatter.Deserialize(ms);
            return (IMessage)message;
        }
    }
}
