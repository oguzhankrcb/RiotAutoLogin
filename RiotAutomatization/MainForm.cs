using RiotAutomatization.API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RiotAutomatization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        LeagueClient LeagueClient;

        private void MainForm_Load(object sender, EventArgs e)
        {
            LeagueClient = new LeagueClient();
        }

        private async void Login_Click(object sender, EventArgs e)
        {
            if (LeagueClient.Connect())
            {
                string rchText = "{\"username\":\"xxx\",\"password\":\"qqq\",\"persistLogin\":false}";

                rchText = rchText.Replace("xxx", textBox1.Text);
                rchText = rchText.Replace("qqq", textBox2.Text);

                string resp = await LeagueClient.MakeHttpRequest("put", "rso-auth/v1/session/credentials", rchText);
               
                if (resp.Contains("auth_failure"))
                {
                    MessageBox.Show("Wrong username or password!", "Error!"
                        , MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Please open League Of Legends Client!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       
    }
}
