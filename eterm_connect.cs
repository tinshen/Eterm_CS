using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Eterm_CS
{
    public partial class eterm_connect : Form
    {
        public string Eterm_host;
        public string Eterm_port;
        public string Eterm_user;
        public string Eterm_pwd;
        public eterm_connect()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Eterm_host = host.Text;
            Eterm_port = port.Text;
            Eterm_user = user.Text;
            Eterm_pwd = pwd.Text;
            Close();
        }

        private void eterm_connect_Load(object sender, EventArgs e)
        {
            host.Text = Eterm_host;
            port.Text = Eterm_port;
            user.Text = Eterm_user;
            pwd.Text = Eterm_pwd;
        }
    }
}
