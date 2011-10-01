using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Specialized;
using System.Threading;

namespace ChatServer
{
    public delegate void registerHandler(string id, string pass);
    public delegate void SetTextCallback(object sender, clientConnectionEventArgs e);
    public partial class Form1 : Form
    {
        private string[] idS;
		private string port;
		private chatServer chatSer;

		/// <summary>
		/// Required designer variable.
		/// </summary>
	

        public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			chatSer = new chatServer();
            //port = ConfigurationManager.AppSettings["ServerPort"];
            port = "9050";
            label4.Text = port;
            chatSer.port = Convert.ToInt32(port);
			chatSer.clientConnected += new clientConnectedHandler(showIDs);
			//list box 2
			idS = chatSer.fetchIDs();
			foreach(string item in idS)
			{
				if(item != null)
				{
					listBox2.Items.Add(item);
				}
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/*protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}*/

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		/*private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.listBox2 = new System.Windows.Forms.ListBox();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.label4 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(152, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Port";
			// 
			// listBox1
			// 
			this.listBox1.Location = new System.Drawing.Point(176, 96);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(80, 134);
			this.listBox1.TabIndex = 3;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(152, 64);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 16);
			this.label2.TabIndex = 4;
			this.label2.Text = "Online users";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 64);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(96, 16);
			this.label3.TabIndex = 5;
			this.label3.Text = "Registered users";
			// 
			// listBox2
			// 
			this.listBox2.Location = new System.Drawing.Point(40, 96);
			this.listBox2.Name = "listBox2";
			this.listBox2.Size = new System.Drawing.Size(80, 134);
			this.listBox2.TabIndex = 6;
			// 
			// linkLabel1
			// 
			this.linkLabel1.Location = new System.Drawing.Point(16, 280);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(104, 16);
			this.linkLabel1.TabIndex = 7;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Register new User";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(216, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(56, 16);
			this.label4.TabIndex = 8;
			// 
			// chatServerForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 309);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.listBox2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.listBox1);
			this.Controls.Add(this.label1);
			this.Name = "chatServerForm";
			this.Text = "Chat";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.chatServerForm_Closing);
			this.ResumeLayout(false);

		}*/
		#endregion
        
        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			chatSer.close();
		}
		private void showIDs(object sender,clientConnectionEventArgs e)
		{
			if(e.Connected)
			{
				listBox1.Items.Remove(e.ID);
			}
			else
			{
                if (listBox1.Items.Contains(e.ID) == false)
                {
                    //listBox1.Items.Add(e.ID);
                    if (this.listBox1.InvokeRequired)
                    {
                        // It's on a different thread, so we use Invoke.
                        SetTextCallback d = new SetTextCallback(showIDs);
                        this.Invoke(d, this, e);
                        // new object[] { text + " (Invoke)" }
                    }
                    else
                    {
                        // It's on the same thread, no need for Invoke
                        this.listBox1.Items.Add(e.ID);
                    }

                }
			}
		}
		private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			newUser p = new newUser();
			p.register += new registerHandler(register);
			p.ShowDialog();
		}
		private void register(string id,string pass)
		{
			string[] ids = new string[100];
			ids = chatSer.insertNewChatter(id,pass);
			
			foreach(string item in ids)
				if(item != null)
					if(listBox2.Items.Contains(item) == false)
						listBox2.Items.Add(item);
		}
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
