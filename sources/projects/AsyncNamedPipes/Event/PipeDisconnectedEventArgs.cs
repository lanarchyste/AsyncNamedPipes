namespace AsyncNamedPipes.Event
{
    public class PipeDisconnectedEventArgs
    {
        private readonly NamedPipeConnection _pipeDisconnected;

        public PipeDisconnectedEventArgs(NamedPipeConnection pipeDisconnected)
        {
            _pipeDisconnected = pipeDisconnected;
        }

        public NamedPipeConnection PipeDisconnected
        {
            get { return _pipeDisconnected; }
        }
    }
}
