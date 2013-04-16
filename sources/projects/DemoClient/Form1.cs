using System;
using System.Windows.Forms;
using AsyncNamedPipes;
using AsyncNamedPipes.Event;

namespace DemoClient
{
    public partial class Form1 : Form
    {
        private readonly NamedPipeClient _pipeClient;

        public Form1()
        {
            InitializeComponent();

            _pipeClient = new NamedPipeClient("pipeDemo");
            _pipeClient.MessageReceived += MessageReceived;
        }

        private void MessageReceived(object sender, MessageEventArgs args)
        {
            throw new NotImplementedException();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            var success = _pipeClient.Connect(1000);
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {

        }
    }
}
