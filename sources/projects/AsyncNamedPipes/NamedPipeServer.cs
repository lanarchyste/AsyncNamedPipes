using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using AsyncNamedPipes.Event;
using AsyncNamedPipes.Message;

namespace AsyncNamedPipes
{
    public class NamedPipeServer : NamedPipeBase
    {
        private readonly List<NamedPipeConnection> _pipesConnected;
        private readonly PipeSecurity _pipeSecurity;
        private bool _isRunning;

        public NamedPipeServer(string pipeName, int instances)
            : base(pipeName)
        {
            _pipesConnected = new List<NamedPipeConnection>();

            _isRunning = true;

            _pipeSecurity = new PipeSecurity();

            var windowsIdentity = WindowsIdentity.GetCurrent();
            if (windowsIdentity != null && windowsIdentity.User != null)
                _pipeSecurity.AddAccessRule(new PipeAccessRule(windowsIdentity.User, PipeAccessRights.FullControl, AccessControlType.Allow));

            _pipeSecurity.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));


            for (var i = 0; i < instances; i++)
                CreateServerPipe();
        }

        ~NamedPipeServer()
        {
            Dispose(false);
        }

        public override void Disconnect()
        {
            lock (_pipesConnected)
            {
                _isRunning = false;

                foreach (var pipeConnected in _pipesConnected)
                {
                    pipeConnected.MessageReceived -= ClientMessageReceived;
                    pipeConnected.ClientDisconnected -= ClientDisconnected;
                    pipeConnected.Disconnect();
                }

                _pipesConnected.Clear();
            }

            for (; ; )
            {
                int clientAreadyConnected;

                lock (_pipesConnected)
                    clientAreadyConnected = _pipesConnected.Count;

                if (clientAreadyConnected == 0)
                    break;

                Thread.Sleep(0);
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

                    pipeConnected.MessageReceived -= ClientMessageReceived;
                    pipeConnected.ClientDisconnected -= ClientDisconnected;
                    pipeConnected.Disconnect();

                    pipesDisconnected.Add(pipeConnected);
                }

                foreach (var pipeDisconnected in pipesDisconnected)
                    _pipesConnected.Remove(pipeDisconnected);
            }
        }

        private void CreateServerPipe()
        {
            var serverStream = new NamedPipeServerStream(PipeName, PipeDirection.InOut, -1, PipeTransmissionMode.Message,
                                                         PipeOptions.Asynchronous | PipeOptions.WriteThrough,
                                                         BufferLength, BufferLength, _pipeSecurity);
            serverStream.BeginWaitForConnection(OnClientConnected, serverStream);
        }

        private void OnClientConnected(IAsyncResult result)
        {
            var serverStreamState = (NamedPipeServerStream)result.AsyncState;
            serverStreamState.EndWaitForConnection(result);

            if (_isRunning)
            {
                if (serverStreamState.IsConnected)
                {
                    var pipeConnected = new NamedPipeConnection(serverStreamState, PipeName);
                    pipeConnected.MessageReceived += ClientMessageReceived;
                    pipeConnected.ClientDisconnected += ClientDisconnected;

                    lock (_pipesConnected)
                        _pipesConnected.Add(pipeConnected);
                }

                CreateServerPipe();
            }
            else
                serverStreamState.Close();
        }

        private void ClientDisconnected(object sender, PipeDisconnectedEventArgs args)
        {
            lock (_pipesConnected)
                _pipesConnected.Remove(args.PipeDisconnected);
        }

        private void ClientMessageReceived(object sender, MessageEventArgs args)
        {
            OnMessageReceived(args);
        }
    }
}
