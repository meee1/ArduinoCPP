using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO.Ports;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml;
using System.Net;
using ArdupilotMega;
using ArdupilotMega.Comms;
using ArdupilotMega.Arduino;

namespace ArduinoCPP
{
    class upFirmware
    {
        public delegate void ProgressEventHandler(string status, int progress);

        public static event ProgressEventHandler Progress;

        static void updateprogress(string status, int percent)
        {
            if (Progress != null)
                Progress(status, percent);
        }

        public static void UploadFlash(string filename, string board, string portname)
        {
            byte[] FLASH = new byte[1];
            StreamReader sr = null;
            try
            {
                updateprogress("Reading Hex File", 0);
                sr = new StreamReader(filename);
                FLASH = readIntelHEXv2(sr);
                sr.Close();
                Console.WriteLine("\n\nSize: {0}\n\n", FLASH.Length);
            }
            catch (Exception ex) { if (sr != null) { sr.Dispose(); } updateprogress("Failed read HEX",0); MessageBox.Show("Failed to read firmware.hex : " + ex.Message); return; }
            IArduinoComms  port = new ArduinoSTK();

            if (board == "1280")
            {
                if (FLASH.Length > 126976)
                {
                    MessageBox.Show("Firmware is to big for a 1280, Please upgrade!!");
                    return;
                }
                //port = new ArduinoSTK();
                port.BaudRate = 57600;
            }
            else if (board == "2560" || board == "2560-2")
            {
                port = new ArduinoSTKv2();
                port.BaudRate = 115200;
            }
            port.DataBits = 8;
            port.StopBits = StopBits.One;
            port.Parity = Parity.None;
            port.DtrEnable = true;

            try
            {
                port.PortName = portname;

                port.Open();

                if (port.connectAP())
                {
                    Console.WriteLine("starting");
                    updateprogress("Uploading " + FLASH.Length + " bytes to APM", 0);

                    // this is enough to make ap_var reset
                    //port.upload(new byte[256], 0, 2, 0);

                    port.Progress += new ArdupilotMega.Arduino.ProgressEventHandler(port_Progress);

                    if (!port.uploadflash(FLASH, 0, FLASH.Length, 0))
                    {
                  
                        if (port.IsOpen)
                            port.Close();
                        throw new Exception("Upload failed. Lost sync. Try Arduino!!");
                    }

                    port.Progress -= port_Progress;

                    updateprogress("", 100);

                    Console.WriteLine("Uploaded");

                    int start = 0;
                    short length = 0x100;

                    byte[] flashverify = new byte[FLASH.Length + 256];

                    updateprogress("Verify APM", 0);

                    while (start < FLASH.Length)
                    {
                        updateprogress("",  (int)((start / (float)FLASH.Length) * 100));
                        port.setaddress(start);
                        Console.WriteLine("Downloading " + length + " at " + start);
                        port.downloadflash(length).CopyTo(flashverify, start);
                        start += length;
                    }

                    updateprogress("", 100);

                    for (int s = 0; s < FLASH.Length; s++)
                    {
                        if (FLASH[s] != flashverify[s])
                        {
                            MessageBox.Show("Upload succeeded, but verify failed: exp " + FLASH[s].ToString("X") + " got " + flashverify[s].ToString("X") + " at " + s);
                            break;
                        }
                    }
                }
                else
                {
                    updateprogress("Failed upload",-1);
                    MessageBox.Show("Communication Error - no connection");
                }
                port.Close();

        

                Application.DoEvents();


                updateprogress("Done", 100);
            }
            catch (Exception ex) { updateprogress("Failed upload",-1); MessageBox.Show("Check port settings or Port in use? " + ex.ToString()); port.Close(); }

        }

        static void port_Progress(int progress, string status)
        {
            updateprogress("", progress);
        }

        static byte[] readIntelHEXv2(StreamReader sr)
        {
            byte[] FLASH = new byte[1024 * 1024];

            int optionoffset = 0;
            int total = 0;
            bool hitend = false;

            while (!sr.EndOfStream)
            {
                updateprogress("Reading Hex File", (int)(((float)sr.BaseStream.Position / (float)sr.BaseStream.Length) * 100));

                string line = sr.ReadLine();

                if (line.StartsWith(":"))
                {
                    int length = Convert.ToInt32(line.Substring(1, 2), 16);
                    int address = Convert.ToInt32(line.Substring(3, 4), 16);
                    int option = Convert.ToInt32(line.Substring(7, 2), 16);
                    Console.WriteLine("len {0} add {1} opt {2}", length, address, option);

                    if (option == 0)
                    {
                        string data = line.Substring(9, length * 2);
                        for (int i = 0; i < length; i++)
                        {
                            byte byte1 = Convert.ToByte(data.Substring(i * 2, 2), 16);
                            FLASH[optionoffset + address] = byte1;
                            address++;
                            if ((optionoffset + address) > total)
                                total = optionoffset + address;
                        }
                    }
                    else if (option == 2)
                    {
                        optionoffset = (int)Convert.ToUInt16(line.Substring(9, 4), 16) << 4;
                    }
                    else if (option == 1)
                    {
                        hitend = true;
                    }
                    int checksum = Convert.ToInt32(line.Substring(line.Length - 2, 2), 16);

                    byte checksumact = 0;
                    for (int z = 0; z < ((line.Length - 1 - 2) / 2); z++) // minus 1 for : then mins 2 for checksum itself
                    {
                        checksumact += Convert.ToByte(line.Substring(z * 2 + 1, 2), 16);
                    }
                    checksumact = (byte)(0x100 - checksumact);

                    if (checksumact != checksum)
                    {
                        MessageBox.Show("The hex file loaded is invalid, please try again.");
                        throw new Exception("Checksum Failed - Invalid Hex");
                    }
                }
                //Regex regex = new Regex(@"^:(..)(....)(..)(.*)(..)$"); // length - address - option - data - checksum
            }

            if (!hitend)
            {
                MessageBox.Show("The hex file did no contain an end flag. aborting");
                throw new Exception("No end flag in file");
            }

            Array.Resize<byte>(ref FLASH, total);

            return FLASH;
        }
    }
}
