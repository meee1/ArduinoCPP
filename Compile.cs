using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;

namespace ArduinoCPP
{
    public partial class Compile : Form
    {
        public Dictionary<string, string> chips = new Dictionary<string, string>();

        public Compile()
        {
            InitializeComponent();

            CMB_comport.Items.AddRange(GetPortNames());

            FB_arduino.FileName = Application.ExecutablePath;

            chips.Add("atmega2560", "/hardware/arduino/variants/mega");
            chips.Add("atmega1280", "/hardware/arduino/variants/mega");
            chips.Add("atmega328p", "/hardware/arduino/variants/standard");

            CMB_mcu.DataSource = new List<string>(chips.Keys);
        }

        private void BUT_CompileIt_Click(object sender, EventArgs e)
        {

            //string test = Directory.GetParent(Path.GetDirectoryName(FBpde.FileName)).FullName;
            //Compiler.getFunctionsFromHeaders(Directory.GetFiles(test, "*.*", SearchOption.AllDirectories));
            //return;


            Compiler.cwd = Path.GetDirectoryName(FBpde.FileName);
            Compiler.mainpde = Path.GetFileName(FBpde.FileName);
            Compiler.mcu = CMB_mcu.Text;
            Compiler.variant = chips[CMB_mcu.Text];
            Compiler.librarydir = findLibraries(Path.GetDirectoryName(FBpde.FileName));
            Compiler.HAL_BOARD = CMB_hal_board.Text;
    

            if (!Directory.Exists(Compiler.librarydir))
            {
                MessageBox.Show("Cant find libraries here "+ Compiler.librarydir,"Libraries");
                return;
            }
            Compiler.arduinodir = Path.GetDirectoryName(FB_arduino.FileName);

            try
            {
                Compiler.Progress += new Compiler.ProgressEventHandler(Compiler_Progress);
                TXT_hex.Text = Compiler.doCompile();
            }
            catch (Exception ex) { textBox1.Text = ex.Message.Replace("\n", "\r\n"); /* MessageBox.Show(ex.Message);*/ }
        }

        void Compiler_Progress(string status,int progress)
        {
            if (progress > 0)
            {
                toolStripProgressBar1.Value = progress;
            }
            if (status != "")
            {
                toolStripStatusLabel1.Text = status;
            }

            statusStrip1.Refresh();
        }

        string findLibraries(string pde)
        {
                string[] dirs = Directory.GetDirectories(pde);

                foreach (string dir in dirs)
                {
                    if (dir.ToLower().EndsWith("libraries"))
                        return dir;
                }

                try
                {
                    return findLibraries(Directory.GetParent(pde).FullName);
                }
                catch { MessageBox.Show("No Valid library directory"); return ""; }
        }

        private void BUT_UploadIt_Click(object sender, EventArgs e)
        {
            string board = CMB_mcu.Text.Substring(CMB_mcu.Text.Length - 4, 4);

            upFirmware.Progress += new upFirmware.ProgressEventHandler(Compiler_Progress);

            upFirmware.UploadFlash(TXT_hex.Text,board,CMB_comport.Text);

        }

        private string[] GetPortNames()
        {
            string[] devs = new string[0];

     
            {
                if (Directory.Exists("/dev/"))
                    devs = Directory.GetFiles("/dev/", "*ACM*");
            }

            string[] ports = SerialPort.GetPortNames();

            string[] all = new string[devs.Length + ports.Length];

            devs.CopyTo(all, 0);
            ports.CopyTo(all, devs.Length);

            return all;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            new Intillisense.FormMain().Show();
        }

        private void CMB_comport_Click(object sender, EventArgs e)
        {
            CMB_comport.Items.Clear();
            CMB_comport.Items.AddRange(GetPortNames());
        }

    }
}
