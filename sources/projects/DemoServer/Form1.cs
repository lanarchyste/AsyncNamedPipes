using System;
using System.Windows.Forms;
using AsyncNamedPipes;
using AsyncNamedPipes.Event;
using AsyncNamedPipes.Message;

namespace DemoServer
{
    public partial class Form1 : Form
    {
        private readonly NamedPipeServer _pipeServer;

        public Form1()
        {
            InitializeComponent();

            _pipeServer = new NamedPipeServer("pipeDemo", 255);
            _pipeServer.MessageReceived += MessageReceived;
        }

        private void MessageReceived(object sender, MessageEventArgs args)
        {
            rtbMessageReceived.Text += args.Message + Environment.NewLine;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _pipeServer.Connect();

            btnStop.Enabled = _pipeServer.IsRunning;
            btnStart.Enabled = !_pipeServer.IsRunning;
            btnSend.Enabled = _pipeServer.IsRunning;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _pipeServer.Disconnect();

            btnStop.Enabled = _pipeServer.IsRunning;
            btnStart.Enabled = !_pipeServer.IsRunning;
            btnSend.Enabled = _pipeServer.IsRunning;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (_pipeServer.ClientsConnectedCount > 0)
            {
                var message = new GenericMessage("server", "all", DateTime.Now, typeof (string), txtMessage.Text);
                _pipeServer.SendMessage(message);
            }
            else
                rtbMessageReceived.Text += "Impossible d'envoyer le message : aucun client" + Environment.NewLine;
        }
    }
}
