using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace ArduinoCPP
{
    public partial class FileBrowse : UserControl
    {
        OpenFileDialog ofd = new OpenFileDialog();

        public string filter { get { return ofd.Filter; } set { ofd.Filter = value; } }
        public string label { get { return label1.Text; } set { label1.Text = value; } }
        public string FileName { get { return textBox1.Text; } set { textBox1.Text = value; } }

        public FileBrowse()
        {
            InitializeComponent();
            filter = "All Files|*.*";
            label = "File";
        }

        private void button1_Click(object sender, EventArgs e)
        {

            ofd.ShowDialog();

            if (ofd.FileName != "")
            {
                textBox1.Text = ofd.FileName;
            }
        }
    }
}
