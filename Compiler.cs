using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ArduinoCPP
{
    class Compiler
    {
        public delegate void ProgressEventHandler(string status, int progress);

        public static event ProgressEventHandler Progress;

        static List<string> librarys = new List<string>();

        public static string arduinodir = Path.GetDirectoryName(Application.ExecutablePath);

        public static string cwd = "c:/Users/hog/Desktop/DIYDrones/ardupilot-mega/ArduPlane";
        public static string mainpde = "ArduPlane.pde";
        public static string librarydir = "c:/Users/hog/Desktop/DIYDrones/ardupilot-mega/libraries";

        static string arduinocoredir = arduinodir + "/hardware/arduino/cores/arduino";
        static string arduinolibdir = arduinodir + "/libraries";

        public static string mcu = "atmega2560";
        public static string variant = "";

        static string tmp = @"c:\temp\build-" + Path.GetRandomFileName();

        public static string avrgcc = "c:/arduino-1.0.1/hardware/tools/avr/bin/avr-gcc.exe";
        public static string avrgpp = "c:/arduino-1.0.1/hardware/tools/avr/bin/avr-g++.exe";
        public static string avrar = "c:/arduino-1.0.1/hardware/tools/avr/bin/avr-ar.exe";
        public static string avrobjcopy = "c:/arduino-1.0.1/hardware/tools/avr/bin/avr-objcopy.exe";

        public static string HAL_BOARD = "";

        static string SketchName = "";
        
        static string maincpp = "";

        public static string doCompile()
        {
            tmp = @"c:\temp\build-" + Path.GetRandomFileName();

            Directory.CreateDirectory(tmp);

            if (!File.Exists(avrgcc))
                avrgcc = arduinodir + Path.DirectorySeparatorChar + "hardware/tools/avr/bin/avr-gcc.exe";
            if (!File.Exists(avrgpp))
                avrgpp = arduinodir + Path.DirectorySeparatorChar + "hardware/tools/avr/bin/avr-g++.exe";
            if (!File.Exists(avrar))
                avrar = arduinodir + Path.DirectorySeparatorChar + "hardware/tools/avr/bin/avr-ar.exe";
            if (!File.Exists(avrobjcopy))
                avrobjcopy = arduinodir + Path.DirectorySeparatorChar + "hardware/tools/avr/bin/avr-objcopy.exe";

            arduinocoredir = arduinodir + "/hardware/arduino/cores/arduino";
            arduinolibdir = arduinodir + "/libraries";
            variant = arduinodir + variant;

            maincpp = Path.GetFileNameWithoutExtension(mainpde) + ".cpp";

            SketchName = Path.GetFileNameWithoutExtension(mainpde);

            librarys = new List<string>();

            updateprogress("Main cpp", 0);

            // create main sketch cpp file
            createCPP(cwd, mainpde, tmp);

            updateprogress("Compile main cpp", 20);

            // scan and compile main cpp
            scandir(tmp, true);
            // compile other cpp files in dir
            scandir(cwd, true);

            updateprogress("Compile user libs", 40);

            // scan and compile all user librarys
            foreach (var lib in getIncludes(librarydir))
            {
                scandir(lib, true);
                if (Directory.Exists(lib + "\\utility"))
                {
                    scandir(lib + "\\utility", true);
                }
            }

            updateprogress("compile system libs", 60);

            // scan and compile all arduino librarys
            foreach (var lib in getIncludes(arduinolibdir))
            {
                scandir(lib, false);
                if (Directory.Exists(lib + "\\utility"))
                {
                    scandir(lib + "\\utility", true);
                }
            }

            updateprogress("Compile Core", 80);

            string objects = "";
            List<string> core = new List<string>();
            /*
            //scan and compile arduino core
            core = scandir(arduinocoredir, false);

            // create ar from core
            foreach (var objs in core)
            {
                objects = objects + " " + tmp + Path.DirectorySeparatorChar + objs;
            }
             */
            startProcessar(arduinocoredir, "core", objects);
            
            updateprogress("Link", 90);

            // final link
            string[] files = Directory.GetFiles(tmp,"*.o",SearchOption.AllDirectories);
            objects = "";
            foreach (var objs in files)
            {
                if (Path.GetExtension(objs).ToLower().Equals(".o"))
                {
                    if (core.Contains(Path.GetFileName(objs)))
                        continue;

                    objects = objects + " " + objs;
                }
            }
            objects = objects + " " + tmp + "\\core.a";
            startProcessgccfinal(tmp, Path.GetFileNameWithoutExtension(mainpde), objects);

            // hex file
            startProcesshex(tmp, Path.GetFileNameWithoutExtension(mainpde), "");

            Console.WriteLine("Hex: " + tmp + "\\" + Path.GetFileNameWithoutExtension(mainpde) + ".hex");
            Console.WriteLine("Finished!!");

            updateprogress("Done", 100);

            return tmp + "\\" + Path.GetFileNameWithoutExtension(mainpde) + ".hex";
        }

        static void updateprogress(string status, int percent)
        {
            if (Progress != null)
                Progress(status, percent);
        }

        // scan a directory and compile all .cpp .c and .S
        static List<string> scandir(string dir, bool userlibs)
        {
            List<string> objdone = new List<string>();

            string[] files = Directory.GetFiles(dir);

            string libstring = " -I\"" + cwd + "\" ";
            if (Directory.Exists(dir + "\\utility"))
                libstring += "-I\"" + dir + "\\utility\" ";

            if (userlibs)
            {
                    foreach (var lib in librarys)
                    {
                        string path = librarydir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(lib);

                        if (Directory.Exists(path))
                        {
                            libstring = libstring + " -I\"" + path + "\"";
                            continue;
                        }

                        path = arduinolibdir + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(lib);

                        if (Directory.Exists(path))
                        {
                            libstring = libstring + " -I\"" + path + "\"";
                            continue;
                        }
                    }
            }

            libstring = " -I\"" + arduinocoredir + "\" \"-I" + variant + "\" " + libstring;


            //throw new Exception("crap");
            foreach (var file in files)
            {
                // file in : file out
                string dirout = tmp;

                if (Path.GetExtension(file).ToLower().Equals(".cpp"))
                {
                    startProcessgpp(dir, Path.GetFileNameWithoutExtension(file), libstring);
                    objdone.Add(Path.GetFileNameWithoutExtension(file) + ".o");
                }
                if (Path.GetExtension(file).ToLower().Equals(".s"))
                {
                    startProcessgccasm(dir, Path.GetFileNameWithoutExtension(file), libstring);
                    objdone.Add(Path.GetFileNameWithoutExtension(file) + ".o");
                }
                if (Path.GetExtension(file).ToLower().Equals(".c"))
                {
                    startProcessgcc(dir, Path.GetFileNameWithoutExtension(file), libstring);
                    objdone.Add(Path.GetFileNameWithoutExtension(file) + ".o");
                }
            }

            return objdone;
        }

        static void startProcess(string filename, string args)
        {
            System.Diagnostics.Process P = new System.Diagnostics.Process();
            P.StartInfo.WorkingDirectory = Path.GetDirectoryName(filename);
            P.StartInfo.FileName = filename;
            P.StartInfo.Arguments = args;
            //P.StartInfo.RedirectStandardOutput = true;
            P.StartInfo.RedirectStandardError = true;
            //P.StartInfo.UseShellExecute = false;
            P.StartInfo.UseShellExecute = false;
            P.Start();

            Console.WriteLine("Started "+ filename + args);

            string output = "";

            while (!P.HasExited)
            {
                System.Threading.Thread.Sleep(100);
                Console.Write(".");
                output = output + P.StandardError.ReadLine();
            }

            if (P.ExitCode != 0)
            {
                output = output + P.StandardError.ReadToEnd();
                Console.WriteLine("ERROR!!");

                throw new Exception("Error compiling\n" + output);
            }

            Console.WriteLine(output);
        }

        static void startProcessgpp(string srcdir, string filename, string libs)
        {
            Console.WriteLine("g++ " + filename);
            updateprogress(Path.GetFileNameWithoutExtension(filename), -1);

            string desto = tmp + "\\" + filename + ".o";
            if (srcdir.ToLower().Contains("libraries"))
            {
                string dir = tmp + "\\" + srcdir.Substring(srcdir.IndexOf("libraries"));
                Directory.CreateDirectory(dir);
                desto = dir + "\\" + filename + ".o";
            }

            //                 /avr-g++.exe -g -mmcu=atmega2560 -mcall-prologues  -DF_CPU=16000000L -DARDUINO=100 -DSKETCH=\"ArduCopter\" -DEXCLUDECORE  -DCONFIG_HAL_BOARD=HAL_BOARD_APM2 -Wa,-adhlns=../tmp/ArduCopter//ArduCopter.lst -Os -Wformat -Wall -Wshadow -Wpointer-arith -Wcast-align -Wwrite-strings -Wformat=2 -Wno-reorder -MD -MT ../tmp/ArduCopter//ArduCopter.o -ffunction-sections -fdata-sections -fno-exceptions -fsigned-char -c -o ../tmp/ArduCopter//ArduCopter.o ../tmp/ArduCopter//ArduCopter.cpp -I. -I..//libraries/AC_PID -I..//libraries/APM_PI -I..//libraries/AP_ADC -I..//libraries/AP_ADC_AnalogSource -I..//libraries/AP_AHRS -I..//libraries/AP_Airspeed -I..//libraries/AP_Baro -I..//libraries/AP_Buffer -I..//libraries/AP_Camera -I..//libraries/AP_Common -I..//libraries/AP_Compass -I..//libraries/AP_Curve -I..//libraries/AP_Declination -I..//libraries/AP_GPS -I..//libraries/AP_HAL -I..//libraries/AP_HAL_AVR -I..//libraries/AP_HAL_AVR_SITL -I..//libraries/AP_HAL_Empty -I..//libraries/AP_HAL_PX4 -I..//libraries/AP_HAL_SMACCM -I..//libraries/AP_InertialNav -I..//libraries/AP_InertialSensor -I..//libraries/AP_LeadFilter -I..//libraries/AP_Limits -I..//libraries/AP_Math -I..//libraries/AP_Menu -I..//libraries/AP_Motors -I..//libraries/AP_Mount -I..//libraries/AP_OpticalFlow -I..//libraries/AP_Param -I..//libraries/AP_Progmem -I..//libraries/AP_RangeFinder -I..//libraries/AP_Relay -I..//libraries/AP_Scheduler -I..//libraries/DataFlash -I..//libraries/Filter -I..//libraries/GCS_MAVLink -I..//libraries/RC_Channel -I..//libraries/SITL -I..//libraries/memcheck  
            startProcess(avrgpp, " -g -mmcu=" + mcu + " -mcall-prologues  -DF_CPU=16000000L -DARDUINO=100 -DSKETCH=\"" + SketchName + "\" -DEXCLUDECORE  -DCONFIG_HAL_BOARD="+HAL_BOARD+" -Wa,-adhlns=" + desto.Replace(".o",".lst")+ " -Os -Wformat -Wall -Wshadow -Wpointer-arith -Wcast-align -Wwrite-strings -Wformat=2 -Wno-reorder -MD -MT " + desto + " -ffunction-sections -fdata-sections -fno-exceptions -fsigned-char -c -o " + desto + " \"" + srcdir + "\\" + filename + ".cpp\" " + libs);

        }

        static void startProcessgccasm(string srcdir, string filename, string libs)
        {
            Console.WriteLine("asm " + filename);
            updateprogress(Path.GetFileNameWithoutExtension(filename), -1);

            string desto = tmp + "\\" + filename + ".o";
            if (srcdir.ToLower().Contains("libraries"))
            {
                string dir = tmp + "\\" + srcdir.Substring(srcdir.IndexOf("libraries"));
                Directory.CreateDirectory(dir);
                desto = dir + "\\" + filename + ".o";
            }
            ///         avr-gcc.exe -g -mmcu=atmega2560 -mcall-prologues  -DF_CPU=16000000L -DARDUINO=100 -DSKETCH=\"ArduCopter\"         -DEXCLUDECORE  -DCONFIG_HAL_BOARD=HAL_BOARD_APM2 -Wa,-adhlns=../tmp/ArduCopter//libraries/AP_HAL_AVR/utility/ftoa_engine.lst -MD -MT ../tmp/ArduCopter//libraries/AP_HAL_AVR/utility/ftoa_engine.o -x assembler-with-cpp  -c -o ../tmp/ArduCopter//libraries/AP_HAL_AVR/utility/ftoa_engine.o ..//libraries/AP_HAL_AVR/utility/ftoa_engine.S -I..//libraries/AP_HAL_AVR/utility//utility -I..//libraries/AC_PID -I..//libraries/APM_PI -I..//libraries/AP_ADC -I..//libraries/AP_ADC_AnalogSource -I..//libraries/AP_AHRS -I..//libraries/AP_Airspeed -I..//libraries/AP_Baro -I..//libraries/AP_Buffer -I..//libraries/AP_Camera -I..//libraries/AP_Common -I..//libraries/AP_Compass -I..//libraries/AP_Curve -I..//libraries/AP_Declination -I..//libraries/AP_GPS -I..//libraries/AP_HAL -I..//libraries/AP_HAL_AVR -I..//libraries/AP_HAL_AVR_SITL -I..//libraries/AP_HAL_Empty -I..//libraries/AP_HAL_PX4 -I..//libraries/AP_HAL_SMACCM -I..//libraries/AP_InertialNav -I..//libraries/AP_InertialSensor -I..//libraries/AP_LeadFilter -I..//libraries/AP_Limits -I..//libraries/AP_Math -I..//libraries/AP_Menu -I..//libraries/AP_Motors -I..//libraries/AP_Mount -I..//libraries/AP_OpticalFlow -I..//libraries/AP_Param -I..//libraries/AP_Progmem -I..//libraries/AP_RangeFinder -I..//libraries/AP_Relay -I..//libraries/AP_Scheduler -I..//libraries/DataFlash -I..//libraries/Filter -I..//libraries/GCS_MAVLink -I..//libraries/RC_Channel -I..//libraries/SITL -I..//libraries/memcheck  
            startProcess(avrgcc, " -g -mmcu=" + mcu + " -mcall-prologues  -DF_CPU=16000000L -DARDUINO=100 -DSKETCH=\"" + SketchName + "\" -DEXCLUDECORE  -DCONFIG_HAL_BOARD=" + HAL_BOARD + " -Wa,-adhlns=" + desto.Replace(".o", ".lst") + " -MD -MT " + desto + " -x assembler-with-cpp  -c  -o " + desto + " \"" + srcdir + "\\" + filename + ".S\" " + libs);

        }

        static void startProcessgcc(string srcdir, string filename, string libs)
        {
            Console.WriteLine("gcc " + filename);
            updateprogress(Path.GetFileNameWithoutExtension(filename), -1);

            string desto = tmp + "\\" + filename + ".o";
            if (srcdir.ToLower().Contains("libraries"))
            {
                string dir = tmp + "\\" + srcdir.Substring(srcdir.IndexOf("libraries"));
                Directory.CreateDirectory(dir);
                desto = dir + "\\" + filename + ".o";
            }

            ///cygdrive/c/arduino-1.0.1/hardware/tools/avr/bin/avr-gcc.exe -g -mmcu=atmega2560 -mcall-prologues  -DF_CPU=16000000L -DARDUINO=100 -DSKETCH=\"ArduCopter\" -DEXCLUDECORE  -DCONFIG_HAL_BOARD=HAL_BOARD_APM2 -Wa,-adhlns=../tmp/ArduCopter//libraries/AP_HAL_AVR/utility/pins_arduino_mega.lst -Os -Wformat -Wall -Wshadow -Wpointer-arith -Wcast-align -Wwrite-strings -Wformat=2 -MD -MT ../tmp/ArduCopter//libraries/AP_HAL_AVR/utility/pins_arduino_mega.o -ffunction-sections -fdata-sections -fsigned-char -c -o ../tmp/ArduCopter//libraries/AP_HAL_AVR/utility/pins_arduino_mega.o ..//libraries/AP_HAL_AVR/utility/pins_arduino_mega.c -I..//libraries/AP_HAL_AVR/utility//utility -I..//libraries/AC_PID -I..//libraries/APM_PI -I..//libraries/AP_ADC -I..//libraries/AP_ADC_AnalogSource -I..//libraries/AP_AHRS -I..//libraries/AP_Airspeed -I..//libraries/AP_Baro -I..//libraries/AP_Buffer -I..//libraries/AP_Camera -I..//libraries/AP_Common -I..//libraries/AP_Compass -I..//libraries/AP_Curve -I..//libraries/AP_Declination -I..//libraries/AP_GPS -I..//libraries/AP_HAL -I..//libraries/AP_HAL_AVR -I..//libraries/AP_HAL_AVR_SITL -I..//libraries/AP_HAL_Empty -I..//libraries/AP_HAL_PX4 -I..//libraries/AP_HAL_SMACCM -I..//libraries/AP_InertialNav -I..//libraries/AP_InertialSensor -I..//libraries/AP_LeadFilter -I..//libraries/AP_Limits -I..//libraries/AP_Math -I..//libraries/AP_Menu -I..//libraries/AP_Motors -I..//libraries/AP_Mount -I..//libraries/AP_OpticalFlow -I..//libraries/AP_Param -I..//libraries/AP_Progmem -I..//libraries/AP_RangeFinder -I..//libraries/AP_Relay -I..//libraries/AP_Scheduler -I..//libraries/DataFlash -I..//libraries/Filter -I..//libraries/GCS_MAVLink -I..//libraries/RC_Channel -I..//libraries/SITL -I..//libraries/memcheck  
            //        /avr-gcc.exe -g -mmcu=atmega2560  -mcall-prologues  -DF_CPU=16000000L -DARDUINO=100 -DSKETCH=\"ArduCopter\"         -DEXCLUDECORE  -DCONFIG_HAL_BOARD=HAL_BOARD_APM2 -Wa,-adhlns=../t                                -Os -Wformat -Wall -Wshadow -Wpointer-arith -Wcast-align -Wwrite-strings -Wformat=2 -MD -MT ../tmp/ArduCopter//libraries/AP_HAL_AVR/utility/pins_arduino_mega.o -ffunction-sections -fdata-sections -fsigned-char -c -o ../tmp/ArduCopter//libraries/AP_HAL_AVR/utility/pins_arduino_mega.o ..//libraries/AP_HAL_AVR/utility/pins_arduino_mega.c -I..//libraries/AP_HAL_AVR/utility//utility -I..//libraries/AC_PID -I..//libraries/APM_PI -I..//libraries/AP_ADC -I..//libraries/AP_ADC_AnalogSource -I..//libraries/AP_AHRS -I..//libraries/AP_Airspeed -I..//libraries/AP_Baro -I..//libraries/AP_Buffer -I..//libraries/AP_Camera -I..//libraries/AP_Common -I..//libraries/AP_Compass -I..//libraries/AP_Curve -I..//libraries/AP_Declination -I..//libraries/AP_GPS -I..//libraries/AP_HAL -I..//libraries/AP_HAL_AVR -I..//libraries/AP_HAL_AVR_SITL -I..//libraries/AP_HAL_Empty -I..//libraries/AP_HAL_PX4 -I..//libraries/AP_HAL_SMACCM -I..//libraries/AP_InertialNav -I..//libraries/AP_InertialSensor -I..//libraries/AP_LeadFilter -I..//libraries/AP_Limits -I..//libraries/AP_Math -I..//libraries/AP_Menu -I..//libraries/AP_Motors -I..//libraries/AP_Mount -I..//libraries/AP_OpticalFlow -I..//libraries/AP_Param -I..//libraries/AP_Progmem -I..//libraries/AP_RangeFinder -I..//libraries/AP_Relay -I..//libraries/AP_Scheduler -I..//libraries/DataFlash -I..//libraries/Filter -I..//libraries/GCS_MAVLink -I..//libraries/RC_Channel -I..//libraries/SITL -I..//libraries/memcheck  
            startProcess(avrgcc, " -g -mmcu=" + mcu + " -mcall-prologues  -DF_CPU=16000000L -DARDUINO=100 -DSKETCH=\"" + SketchName + "\" -DEXCLUDECORE  -DCONFIG_HAL_BOARD=" + HAL_BOARD + " -Wa,-adhlns=" + desto.Replace(".o", ".lst") + " -Os -Wformat -Wall -Wshadow -Wpointer-arith -Wcast-align -Wwrite-strings -Wformat=2 -MD -MT " + desto + " -ffunction-sections -fdata-sections -fsigned-char -c  -o " + desto + " \"" + srcdir + "\\" + filename + ".c\" " + libs);
        }

        static void startProcessar(string srcdir, string filename, string objects)
        {
            Console.WriteLine("ar " + filename);

            startProcess(avrar, " -rcs " + tmp + "\\" + filename + ".a ");

        }

        static void startProcessgccfinal(string srcdir, string filename, string objects)
        {
            Console.WriteLine("gccln " + filename);

            string destelf=tmp + "\\" + filename + ".elf";
            ///       avr-gcc.exe  -g -mmcu=atmega2560  -mcall-prologues  -Os -Wformat -Wall -Wshadow -Wpointer-arith -Wcast-align -Wwrite-strings -Wformat=2 -Wl,--gc-sections -Wl,-Map -Wl,../tmp/ArduCopter//ArduCopter.map -Wl,-m,avr6 -Wl,--relax -o ../tmp/ArduCopter//ArduCopter.elf ../tmp/ArduCopter//ArduCopter.o ../tmp/ArduCopter//libraries/AC_PID/AC_PID.o ../tmp/ArduCopter//libraries/APM_PI/APM_PI.o ../tmp/ArduCopter//libraries/AP_ADC/AP_ADC_ADS7844.o ../tmp/ArduCopter//libraries/AP_ADC/AP_ADC_HIL.o ../tmp/ArduCopter//libraries/AP_ADC/AP_ADC.o ../tmp/ArduCopter//libraries/AP_ADC_AnalogSource/AP_ADC_AnalogSource.o ../tmp/ArduCopter//libraries/AP_AHRS/AP_AHRS_HIL.o ../tmp/ArduCopter//libraries/AP_AHRS/AP_AHRS_MPU6000.o ../tmp/ArduCopter//libraries/AP_AHRS/AP_AHRS.o ../tmp/ArduCopter//libraries/AP_AHRS/AP_AHRS_DCM.o ../tmp/ArduCopter//libraries/AP_Airspeed/AP_Airspeed.o ../tmp/ArduCopter//libraries/AP_Baro/AP_Baro_BMP085.o ../tmp/ArduCopter//libraries/AP_Baro/AP_Baro_MS5611.o ../tmp/ArduCopter//libraries/AP_Baro/AP_Baro_BMP085_hil.o ../tmp/ArduCopter//libraries/AP_Baro/AP_Baro_PX4.o ../tmp/ArduCopter//libraries/AP_Baro/AP_Baro.o ../tmp/ArduCopter//libraries/AP_Camera/AP_Camera.o ../tmp/ArduCopter//libraries/AP_Common/c++.o ../tmp/ArduCopter//libraries/AP_Compass/Compass.o ../tmp/ArduCopter//libraries/AP_Compass/AP_Compass_PX4.o ../tmp/ArduCopter//libraries/AP_Compass/AP_Compass_HMC5843.o ../tmp/ArduCopter//libraries/AP_Compass/AP_Compass_HIL.o ../tmp/ArduCopter//libraries/AP_Curve/AP_Curve.o ../tmp/ArduCopter//libraries/AP_Declination/AP_Declination.o ../tmp/ArduCopter//libraries/AP_GPS/AP_GPS_MTK.o ../tmp/ArduCopter//libraries/AP_GPS/AP_GPS_MTK19.o ../tmp/ArduCopter//libraries/AP_GPS/AP_GPS_HIL.o ../tmp/ArduCopter//libraries/AP_GPS/AP_GPS_SIRF.o ../tmp/ArduCopter//libraries/AP_GPS/AP_GPS_406.o ../tmp/ArduCopter//libraries/AP_GPS/AP_GPS_MTK16.o ../tmp/ArduCopter//libraries/AP_GPS/AP_GPS_NMEA.o ../tmp/ArduCopter//libraries/AP_GPS/AP_GPS_Auto.o ../tmp/ArduCopter//libraries/AP_GPS/AP_GPS_UBLOX.o ../tmp/ArduCopter//libraries/AP_GPS/GPS.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/SPIDeviceManager_APM1.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/Util.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/AnalogIn_Common.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/SPIDevice_SPI3.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/Scheduler.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/RCInput_APM1.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/SPIDeviceManager_APM2.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/HAL_AVR_APM1_Class.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/Console.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/RCInput_APM2.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/I2CDriver.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/HAL_AVR_APM2_Class.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/RCOutput_APM1.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/SPIDevice_SPI0.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/Scheduler_Timer.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/RCOutput_APM2.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/Storage.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/UARTDriver.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/SPIDevice_SPI2.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/Semaphores.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/GPIO.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/AnalogIn_ADC.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/RCInput.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/Util.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/Storage.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/sitl_barometer.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/AnalogIn.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/HAL_AVR_SITL_Class.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/Console.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/sitl_compass.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/sitl_gps.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/Scheduler.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/sitl_ins.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/RCOutput.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/SITL_State.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/UARTDriver.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/HAL_Empty_Class.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/RCInput.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/Util.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/AnalogIn.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/Scheduler.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/Console.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/RCOutput.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/SPIDriver.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/I2CDriver.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/PrivateMember.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/Storage.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/UARTDriver.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/Semaphores.o ../tmp/ArduCopter//libraries/AP_HAL_Empty/GPIO.o ../tmp/ArduCopter//libraries/AP_HAL_PX4/RCInput.o ../tmp/ArduCopter//libraries/AP_HAL_PX4/HAL_PX4_Class.o ../tmp/ArduCopter//libraries/AP_HAL_PX4/Console.o ../tmp/ArduCopter//libraries/AP_HAL_PX4/Scheduler.o ../tmp/ArduCopter//libraries/AP_HAL_PX4/Storage.o ../tmp/ArduCopter//libraries/AP_HAL_PX4/UARTDriver.o ../tmp/ArduCopter//libraries/AP_HAL_PX4/RCOutput.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/RCInput.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/Util.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/AnalogIn.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/Scheduler.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/Console.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/RCOutput.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/SPIDriver.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/I2CDriver.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/AP_HAL_SMACCM_Main.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/PrivateMember.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/Storage.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/HAL_SMACCM_Class.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/Semaphores.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/UARTDriver.o ../tmp/ArduCopter//libraries/AP_HAL_SMACCM/GPIO.o ../tmp/ArduCopter//libraries/AP_InertialNav/AP_InertialNav.o ../tmp/ArduCopter//libraries/AP_InertialSensor/AP_InertialSensor_PX4.o ../tmp/ArduCopter//libraries/AP_InertialSensor/AP_InertialSensor_Stub.o ../tmp/ArduCopter//libraries/AP_InertialSensor/AP_InertialSensor.o ../tmp/ArduCopter//libraries/AP_InertialSensor/AP_InertialSensor_UserInteract_Stream.o ../tmp/ArduCopter//libraries/AP_InertialSensor/AP_InertialSensor_MPU6000.o ../tmp/ArduCopter//libraries/AP_InertialSensor/AP_InertialSensor_Oilpan.o ../tmp/ArduCopter//libraries/AP_LeadFilter/AP_LeadFilter.o ../tmp/ArduCopter//libraries/AP_Limits/AP_Limits.o ../tmp/ArduCopter//libraries/AP_Limits/AP_Limit_Geofence.o ../tmp/ArduCopter//libraries/AP_Limits/AP_Limit_GPSLock.o ../tmp/ArduCopter//libraries/AP_Limits/AP_Limit_Altitude.o ../tmp/ArduCopter//libraries/AP_Limits/AP_Limit_Module.o ../tmp/ArduCopter//libraries/AP_Math/location.o ../tmp/ArduCopter//libraries/AP_Math/vector3.o ../tmp/ArduCopter//libraries/AP_Math/AP_Math.o ../tmp/ArduCopter//libraries/AP_Math/matrix3.o ../tmp/ArduCopter//libraries/AP_Math/polygon.o ../tmp/ArduCopter//libraries/AP_Math/vector2.o ../tmp/ArduCopter//libraries/AP_Math/quaternion.o ../tmp/ArduCopter//libraries/AP_Menu/AP_Menu.o ../tmp/ArduCopter//libraries/AP_Motors/AP_Motors_Class.o ../tmp/ArduCopter//libraries/AP_Motors/AP_MotorsTri.o ../tmp/ArduCopter//libraries/AP_Motors/AP_MotorsOctaQuad.o ../tmp/ArduCopter//libraries/AP_Motors/AP_MotorsHexa.o ../tmp/ArduCopter//libraries/AP_Motors/AP_MotorsMatrix.o ../tmp/ArduCopter//libraries/AP_Motors/AP_MotorsHeli.o ../tmp/ArduCopter//libraries/AP_Motors/AP_MotorsOcta.o ../tmp/ArduCopter//libraries/AP_Motors/AP_MotorsQuad.o ../tmp/ArduCopter//libraries/AP_Motors/AP_MotorsY6.o ../tmp/ArduCopter//libraries/AP_Mount/AP_Mount.o ../tmp/ArduCopter//libraries/AP_OpticalFlow/AP_OpticalFlow_ADNS3080.o ../tmp/ArduCopter//libraries/AP_OpticalFlow/AP_OpticalFlow.o ../tmp/ArduCopter//libraries/AP_Param/AP_Param.o ../tmp/ArduCopter//libraries/AP_Progmem/AP_Progmem_AVR.o ../tmp/ArduCopter//libraries/AP_RangeFinder/AP_RangeFinder_MaxsonarI2CXL.o ../tmp/ArduCopter//libraries/AP_RangeFinder/AP_RangeFinder_MaxsonarXL.o ../tmp/ArduCopter//libraries/AP_RangeFinder/RangeFinder.o ../tmp/ArduCopter//libraries/AP_RangeFinder/AP_RangeFinder_SharpGP2Y.o ../tmp/ArduCopter//libraries/AP_Relay/AP_Relay.o ../tmp/ArduCopter//libraries/AP_Scheduler/AP_Scheduler.o ../tmp/ArduCopter//libraries/DataFlash/DataFlash_APM2.o ../tmp/ArduCopter//libraries/DataFlash/DataFlash_Empty.o ../tmp/ArduCopter//libraries/DataFlash/DataFlash.o ../tmp/ArduCopter//libraries/DataFlash/DataFlash_SITL.o ../tmp/ArduCopter//libraries/DataFlash/DataFlash_APM1.o ../tmp/ArduCopter//libraries/Filter/DerivativeFilter.o ../tmp/ArduCopter//libraries/GCS_MAVLink/GCS_MAVLink.o ../tmp/ArduCopter//libraries/RC_Channel/RC_Channel_aux.o ../tmp/ArduCopter//libraries/RC_Channel/RC_Channel.o ../tmp/ArduCopter//libraries/SITL/SITL.o ../tmp/ArduCopter//libraries/memcheck/memcheck.o ../tmp/ArduCopter//libraries/AP_HAL/utility/Print.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/utility/ISRRegistry.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/utility/print_vprintf.o ../tmp/ArduCopter//libraries/AP_HAL_AVR_SITL/utility/print_vprintf.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/utility/pins_arduino_mega.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/utility/ftoa_engine.o ../tmp/ArduCopter//libraries/AP_HAL_AVR/utility/ultoa_invert.o ../tmp/ArduCopter//arduino/core.a -lm
            startProcess(avrgcc, " -g -mmcu=" + mcu + " -mcall-prologues  -Os -Wformat -Wall -Wshadow -Wpointer-arith -Wcast-align -Wwrite-strings -Wformat=2 -Wl,--gc-sections -Wl,-Map -Wl," + destelf.Replace(".elf", ".map") + " -Wl,-m,avr6 -Wl,--relax -o " + destelf + " "+objects + " -lm");

        }

        static void startProcesshex(string srcdir, string filename, string objects)
        {
            Console.WriteLine("hex " + filename);

            startProcess(avrobjcopy, " -O ihex -R .eeprom " + tmp + "\\" + filename + ".elf " + tmp + "\\" + filename + ".hex ");

        }

        // create main arduino cpp
        static void createCPP(string dir, string mainpde, string builddir)
        {
            string[] files = Directory.GetFiles(dir);

            Directory.CreateDirectory(builddir);

            List<string> temp = new List<string>(files);

            temp.Sort(StringComparer.Ordinal);

            temp.Insert(0, dir + "\\" + mainpde);

            files = temp.ToArray();

            maincpp = builddir + "\\" + maincpp;

            StreamWriter sw = new StreamWriter(maincpp);

            bool includes = true;
            bool toggle = false;

            List<string> done = new List<string>();

            foreach (string file in files)
            {
                if (done.Contains(file))
                {
                    continue;
                }
                done.Add(file);

                string ext = Path.GetExtension(file);

                int lineno = 0;

                if (ext.ToLower() == ".pde" || ext.ToLower() == ".ino")
                {
                    sw.WriteLine("\n#line 1 \"" + file.Replace("\\", "/") + "\"");

                  //  sw.WriteLine();

                    StreamReader sr = new StreamReader(file);

                    if (includes)
                    {
                        while (!sr.EndOfStream)
                        {
                            // check comment or define
                            string line = sr.ReadLine();
                            lineno++;

                            if (!includes)
                            {
                                sw.Write(line + "\n");
                                continue;
                            }

                            if (Regex.IsMatch(line, @"\s*(/\*|\*/)"))
                            {

                                toggle = !toggle;
                                sw.Write(line + "\n");
                                if (Regex.IsMatch(line, @"\*/"))
                                    toggle = false;
                                //print "Comment line toggle\n";
                                continue;
                            }

                            if (Regex.IsMatch(line, @"^#.*$"))
                            {
                                sw.Write(line + "\n");

                                Match match = Regex.Match(line, @"^#\s*include <(.*)>");
                                if (match.Length > 0)
                                {
                                    librarys.Add(match.Groups[1].Value);
                                }

                                continue;
                            }

                            if (toggle == true || Regex.IsMatch(line, @"^\s*\/\/") || Regex.IsMatch(line, @"^\s*$"))
                            {
                                sw.Write(line + "\n");
                                continue;
                            }

                            if (Regex.IsMatch(line, @"\s*\w+") && toggle == false)
                            {
                                //print "includes line toggle";
                                doHeaders(files, sw);

                               // sw.Write("#line " + lineno + " \"" + file.Replace("\\", "/") + "\"\n");
                                sw.Write(line + "\n");
                                includes = false;
                                continue;
                            }

                            //print "WTF line";
                        }

                        includes = false;
                    }
                    else
                    {
                        sw.Write(sr.ReadToEnd());
                    }

                }
            }
            sw.Close();
        }

        static string[] getIncludes(string dir)
        {
            string[] files = Directory.GetDirectories(dir);

            List<string> temp = new List<string>();

            foreach (var str in files)
            {
                if (librarys.Contains(Path.GetFileName(str) + ".h"))
                    temp.Add(str);
            }

            temp.Sort(StringComparer.Ordinal);

            return temp.ToArray();
        }

        static void doHeaders(string[] files, StreamWriter sw)
        {

          //  sw.Write("#line 1 \"autogenerated\"\n");
          //  sw.Write("#include \"Arduino.h\"\n");

            List<string> done = new List<string>();

            foreach (string file in files)
            {
                if (done.Contains(file))
                {
                    continue;
                }
                done.Add(file);

                string ext = Path.GetExtension(file);

                if (ext.ToLower() == ".pde" || ext.ToLower() == ".ino")
                {
                    StreamReader sr = new StreamReader(file);

                    string allfile = sr.ReadToEnd();

                    string[] data = allfile.Split(new char[] { '{' }, StringSplitOptions.RemoveEmptyEntries);



                    foreach (string feline in data)
                    {
                        string line = feline.Trim();
                        //                         type      |  qualifiers   | name     | args                   |   end line

                        //[\n|^]\s*([\w_]+\s+([\w_\*\&]+\s*)*([\w_]+\s*)\([\s\w_,\&\*\[\]]*\)\s*)$

                        //Console.WriteLine(line);

                        //line = "test\nstatic void update_lights()";

                        Regex regex = new Regex(@"\n\s*(([\w_]+)\s+([\w_\*\&]+\s+)*([\w_]+)\s*\(([\s\w_,\&\*\[\]]*)\))\s*$", RegexOptions.CultureInvariant);

                        Regex regex2 = new Regex(@"^\s*(([\w_]+)\s+([\w_\*\&]+\s+)*([\w_]+)\s*\(([\s\w_,\&\*\[\]]*)\))\s*$", RegexOptions.CultureInvariant);

                        int lastpos = line.LastIndexOf('\n');

                        string t2 = line.TrimEnd();// line.Substring(lastpos + 1).Trim();

                        if (!t2.Contains("(") || !t2.Contains(")"))
                        {
                            //Console.WriteLine("FAIL: "+t2);
                            continue;
                        }

                        Console.WriteLine("PASS: " + t2);
                        Console.WriteLine(file);
                        MatchCollection matchs = regex.Matches(t2);
                        if (matchs.Count == 0)
                        {
                            matchs = regex2.Matches(t2);
                        }

                        if (matchs.Count > 0)
                        {
                            Match match = matchs[0];

                            if (match.Length > 0)
                            {
                                for (int a = 0; a < match.Groups.Count; a++)
                                {
                                    Console.Write(a + ":" + match.Groups[a].Value + "  ");
                                }
                                string proto = match.Groups[0].Value;
                                proto = proto.Trim();
                              //  proto = proto.Replace("\r\n", " ");
                                Console.Write(file + " - " + proto + "\n");
                                sw.Write(proto + ";\n");
                                sw.Flush();
                            }
                        }
                    }
                }
            }
        }

        public static string[] getFunctionsFromHeaders(string[] files)
        {
            List<string> headers = new List<string>();

            List<string> done = new List<string>();

            foreach (string file in files)
            {
                if (file.EndsWith("keywords.txt"))
                    File.Delete(file);
            }

            foreach (string file in files)
            {
                if (done.Contains(file))
                {
                    continue;
                }
                done.Add(file);

                string ext = Path.GetExtension(file);

                if (ext.ToLower() == ".h" && file.Contains("libraries"))
                {
                    StreamReader sr = new StreamReader(file);

                    StreamWriter sw = new StreamWriter(Path.GetDirectoryName(file) + Path.DirectorySeparatorChar + "keywords.txt", true);

                    string allfile = sr.ReadToEnd();

                    string[] data = allfile.Split(new char[] { ';', '{' }, StringSplitOptions.RemoveEmptyEntries);



                    foreach (string feline in data)
                    {
                        string line = feline.Trim();
                        //                         type      |  qualifiers   | name     | args                   |   end line

                        //[\n|^]\s*([\w_]+\s+([\w_\*\&]+\s*)*([\w_]+\s*)\([\s\w_,\&\*\[\]]*\)\s*)$

                        //Console.WriteLine(line);

                        //line = "test\nstatic void update_lights()";

                        Regex regex = new Regex(@"\n\s*(([\w_]+)\s+([\w_\*\&]+\s+)*([\w_]+)\s*\(([\s\w_,\&\*\[\]]*)\))\s*$", RegexOptions.CultureInvariant);

                        Regex regex2 = new Regex(@"^\s*(([\w_]+)\s+([\w_\*\&]+\s+)*([\w_]+)\s*\(([\s\w_,\&\*\[\]]*)\))\s*$", RegexOptions.CultureInvariant);

                        Regex regex3 = new Regex(@"^\s*class\s+([\w_]+)\s*", RegexOptions.Multiline);


                        int lastpos = line.LastIndexOf('\n');

                        string t2 = line.TrimEnd();// line.Substring(lastpos + 1).Trim();

                        if (!t2.Contains("(") || !t2.Contains(")"))
                        {
                            //Console.WriteLine("FAIL: "+t2);
                            if (t2.Contains("class"))
                            {

                            }
                            else
                            {
                                continue;
                            }
                        }

                        Console.WriteLine("PASS: " + t2);
                        Console.WriteLine(file);
                        MatchCollection matchs = regex.Matches(t2);
                        if (matchs.Count == 0)
                        {
                            matchs = regex2.Matches(t2);
                            if (matchs.Count == 0)
                            {
                                matchs = regex3.Matches(t2);

                                if (matchs.Count > 0)
                                {
                                    sw.WriteLine(matchs[0].Groups[1].Value + "\tKEYWORD1");
                                    continue;
                                }
                            }
                        }

                        if (matchs.Count > 0)
                        {
                            Match match = matchs[0];

                            if (match.Length > 0)
                            {
                                for (int a = 0; a < match.Groups.Count; a++)
                                {
                                    Console.Write(a + ":" + match.Groups[a].Value + "  ");
                                }
                                string proto = match.Groups[0].Value;
                                proto = proto.Trim();
                                proto = proto.Replace("\r\n", " ");
                                Console.Write(file + " - " + proto + "\n");
                                sw.WriteLine(match.Groups[4].Value + "\tKEYWORD2");
                            }
                        }
                    }

                    sw.Close();
                }
            }


            return headers.ToArray();

        }
    }
}