using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChatServer
{
    public partial class newUser : Form
    {
        //public delegate void registerHandler(string id, string pass);
        public newUser()
        {
            InitializeComponent();
        }

        private void linkLabel1_MouseClick(object sender, MouseEventArgs e)
        {
            register(textBox1.Text, textBox2.Text);
            this.Close();
        }
        public event registerHandler register;
    }
}
