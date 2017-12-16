using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ArduinoCPP
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.Run(new Compile());          
        }

    }
}

namespace ArdupilotMega
{
    class MainV2
    {
        public static bool MONO = false;
    }
}
