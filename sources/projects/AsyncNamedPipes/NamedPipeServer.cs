using System;
using System.Collections.Generic;
using System.IO.Pipes;
using AsyncNamedPipes.Message;

namespace AsyncNamedPipes
{
    public class NamedPipeServer : NamedPipeBase
    {
        private readonly List<NamedPipeConnection> _pipesConnected;

        public NamedPipeServer(string pipeName)
            : base(pipeName)
        {
            _pipesConnected = new List<NamedPipeConnection>();

            var serverStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, -1, PipeTransmissionMode.Message, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            serverStream.BeginWaitForConnection(ClientConnected, serverStream);
        }

        ~NamedPipeServer()
        {
            Dispose(false);
        }

        public override void Disconnect()
        {
            lock (_pipesConnected)
            {
                foreach (var pipeConnected in _pipesConnected)
                {
                    pipeConnected.Disconnect();
                }
                _pipesConnected.Clear();
            }
        }

        public override void SendMessage(IMessage message)
        {
            var pipesDisconnected = new List<NamedPipeConnection>();

            lock (_pipesConnected)
            {
                foreach (var pipeConnected in _pipesConnected)
                {
                    bool isDisconnected;
                    try
                    {
                        isDisconnected = !pipeConnected.IsConnected;
                        if (!isDisconnected)
                            pipeConnected.SendMessage(message);
                    }
                    catch (Exception)
                    {
                        isDisconnected = true;
                    }

                    if (!isDisconnected) 
                        continue;

                    pipeConnected.Disconnect();
                    pipesDisconnected.Add(pipeConnected);
                }

                foreach (var pipeDisconnected in pipesDisconnected)
                    _pipesConnected.Remove(pipeDisconnected);
            }
        }

        private void ClientConnected(IAsyncResult result)
        {
            var serverStreamState = (NamedPipeServerStream) result.AsyncState;
            serverStreamState.EndWaitForConnection(result);

            if (serverStreamState.IsConnected)
            {
                var pipeConnected = new NamedPipeConnection(serverStreamState, PipeName);
                pipeConnected.MessageReceived += ClientMessageReceived;

                lock (_pipesConnected)
                    _pipesConnected.Add(pipeConnected);
            }

            var serverStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, -1, PipeTransmissionMode.Message, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            serverStream.BeginWaitForConnection(ClientConnected, serverStream);
        }

        private void ClientMessageReceived(object sender, MessageEventArgs args)
        {
            OnMessageReceived(args);
        }
    }
}
