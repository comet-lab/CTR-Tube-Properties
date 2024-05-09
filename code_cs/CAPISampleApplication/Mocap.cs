// System
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

// 3rd Party
using Zeroconf;
using Newtonsoft.Json;

// Matrix Algebra Library
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Data.Text;

// NDI
using NDI.CapiSample;
using NDI.CapiSample.Data;
using NDI.CapiSample.Protocol;
using CAPISampleApplication;

namespace CAPISampleApplication
{
    internal class Mocap
    {
        // NDI's library stuff
        public Capi cAPI { get; set; }
        public string revision { get; set; }

        // Set this to true to minimize print statements in the Console
        // Set this to false to see a bunch of stuff get printed (could be helpful for debugging)
        public static bool quietMode = true;

        /// <summary>
        /// Initializes the motion capture sensor
        /// </summary>
        /// <param name="host">Optional param to specify the port/connection info</param>
        public void Init(string? host = null)
        {
            log("C# CAPI Sample v" + Capi.GetVersion());

            // Use the host specified if provided, otherwise:
            // 1st. try to find a serial device.
            // 2nd. Try to find a network device.
            if (String.IsNullOrEmpty(host))
            {
                //log("Finding serial NDI devices.");
                //string[] comPorts = CapiSerial.GetAvailableComPorts();
                //if (comPorts.Length > 0)
                //{
                //    Console.WriteLine(string.Join(",", comPorts));
                //    host = comPorts[0];
                //}
                //else
                {
                    log("Could not find a serial device.");
                    log("Finding an NDI device on the network.");
                    var task = GetIP();
                    task.Wait(5000);

                    if (task.IsCompleted && task.Result != null)
                    {
                        host = task.Result;
                    }
                    else
                    {
                        log("Could not find an NDI device on the network.");
                    }
                }
            }

            if (host == "" || host == "help" || host == "-h" || host == "/?" || host == "--help")
            {
                // host argument was not provided and we couldn't find one
                if (host == "")
                {
                    log("Could not automatically detect an NDI device, please manually specify one.");
                }

                PrintHelp();
                return;
            }

            // Create a new CAPI instance based on the connection type
            if (host.StartsWith("COM") || host.StartsWith("/dev"))
            {
                this.cAPI = new CapiSerial(host);
            }
            else
            {
                this.cAPI = new CapiTcp(host);
            }

            // Use the same log output format as this sample application
            this.cAPI.SetLogger(log);

            if (!this.cAPI.Connect())
            {
                log("Could not connect to " + this.cAPI.GetConnectionInfo());
                PrintHelp();
                return;
            }
            log("Connected");

            // Get the API Revision this will tell us if BX2 is supported.
            this.revision = this.cAPI.GetAPIRevision();
            log("Revision:" + this.revision);

            if (!this.cAPI.Initialize())
            {
                log("Could not initialize.");
                return;
            }
            log("Initialized");

            // The Frame Frequency may not be possible to set on all devices, so an error response is okay.
            this.cAPI.SetUserParameter("Param.Tracking.Frame Frequency", "60");
            this.cAPI.SetUserParameter("Param.Tracking.Track Frequency", "2");

            // Read the final values
            log(this.cAPI.GetUserParameter("Param.Tracking.Frame Frequency"));
            log(this.cAPI.GetUserParameter("Param.Tracking.Track Frequency"));

            // Initialize tool ports
            if (!InitializePorts(this.cAPI))
            {
                return;
            }
        }

        /// <summary>
        /// Get all the passive strays that the motion capture can see
        /// </summary>
        /// <returns>A list of Marker objects, where each Marker is a 
        /// reflective object visible to the sensor and not associated
        /// with a specific tool</returns>
        public List<Marker> FindStrays()
        {
            List<Marker> retVal = new List<Marker>() { };

            if (!cAPI.TrackingStart())
            {
                log("Could not start tracking.");
                return null;
            }
            //log("TrackingStarted");

            if (IsBX2Supported(this.revision))
            {
                for (int i = 0; i < 1; i++)
                {
                    if (!cAPI.IsConnected)
                    {
                        log("Disconnected while tracking.");
                        break;
                    }

                    //Console.WriteLine("Sending BX2 (strays)");

                    List<Tool> strays = cAPI.SendBX2("--3d=strays");

                    //Console.WriteLine("Sent BX2");

                    retVal = strays[^1].markers;

                    //foreach (Tool s in strays)
                    //{
                    //    log(s.ToString());
                    //    log(s.markers.ToString());
                    //    log(JsonConvert.SerializeObject(s.markers));
                    //    //log(JsonConvert.SerializeObject(t.transform.orientation));
                    //    //log(t.transform.position.x.GetType().ToString());
                    //    //foreach (var m in t.markers)
                    //    //{
                    //    //    log(m.ToString());
                    //    //}
                    //    retVal.Add(s.ToString());
                    //}
                }
            }
            else
            {
                throw new Exception("BX2 Not supported");
            }

            if (!cAPI.TrackingStop())
            {
                log("Could not stop tracking.");
            }
            //log("TrackingStopped");

            return retVal;
        }

        /// <summary>
        /// Given a list of strays and the previous position of the desired stray, 
        /// figure out which stray is the correct one
        /// </summary>
        /// <param name="strays">The list of strays observed by the sensor</param>
        /// <param name="position">The previous position of the desired stray,
        /// or the expected position of the desired stray. In camera space.</param>
        /// <returns>The stray in the given list closest to the given position</returns>
        public Marker GetCorrectStray(List<Marker> strays, double[] position)
        {
            int strayIndex = -1;
            double minStrayDist = double.PositiveInfinity;

            // For each stray in the list...
            for (int s = 0; s < strays.Count; s++)
            {
                // Convert the stray's position from mm to m
                Vector<double> sPosition = Vector<double>.Build.Dense(new double[] { strays[s].position.x/(double)1000.0, strays[s].position.y / (double)1000.0, strays[s].position.z / (double)1000.0 });

                // Get the distance between the stray's position and the position inputted by the user
                double newStrayDist = (sPosition - Vector<double>.Build.Dense(position)).L2Norm();

                // If this distance is less than the minimum distance...
                if (newStrayDist < minStrayDist)
                {
                    // ... then this stray is the closest to the correct position, and this stray's distance is the new minimum distance
                    minStrayDist = newStrayDist;
                    strayIndex = s;
                }
            }

            Marker correctStray = strays[strayIndex];

            return correctStray;
        }

        /// <summary>
        /// See GetCorrectStray. This does the same, but prints a bunch of stuff
        /// </summary>
        /// <param name="strays">The list of strays observed by the sensor</param>
        /// <param name="position">The previous position of the desired stray,
        /// or the expected position of the desired stray. In camera space.</param>
        /// <returns>The stray in the given list closest to the given position</returns>
        public Marker GetCorrectStrayVerbose(List<Marker> strays, double[] position)
        {
            int strayIndex = -1;
            double minStrayDist = double.PositiveInfinity;

            Console.WriteLine("getting stray closest to position:");
            Console.WriteLine(Vector<double>.Build.Dense(position).ToString());

            for (int s = 0; s < strays.Count; s++)
            {
                Vector<double> sPosition = Vector<double>.Build.Dense(new double[] { strays[s].position.x / (double)1000.0, strays[s].position.y / (double)1000.0, strays[s].position.z / (double)1000.0 });

                double newStrayDist = (sPosition - Vector<double>.Build.Dense(position)).L2Norm();

                Console.WriteLine("index");
                Console.WriteLine(s.ToString());
                Console.WriteLine("stray position");
                Console.WriteLine(sPosition.ToString());
                Console.WriteLine("distance");
                Console.WriteLine(newStrayDist.ToString());

                if (newStrayDist < minStrayDist)
                {
                    minStrayDist = newStrayDist;
                    strayIndex = s;
                }
            }

            Marker correctStray = strays[strayIndex];

            Console.WriteLine("using this stray:");
            Console.WriteLine(correctStray.position.x);
            Console.WriteLine(correctStray.position.y);
            Console.WriteLine(correctStray.position.z);

            return correctStray;
        }

        /// <summary>
        /// Get all the tools that the motion capture sensor recognizes. A Tool
        /// is a set of 3 or more reflective objects arranged in a predefined geometry
        /// with a predefined central point
        /// </summary>
        /// <returns>A list of all the tools visible to the motion capture sensor.</returns>
        /// <exception cref="Exception"></exception>
        public List<Tool> FindTools()
        {
            List<Tool> retVal = new List<Tool>() { };

            if (!cAPI.TrackingStart())
            {
                log("Could not start tracking.");
                return null;
            }
            //log("TrackingStarted");

            if (IsBX2Supported(this.revision))
            {
                for (int i = 0; i < 1; i++)
                {
                    if (!cAPI.IsConnected)
                    {
                        log("Disconnected while tracking.");
                        break;
                    }

                    //Console.WriteLine("Sending BX2");

                    List<Tool> tools = cAPI.SendBX2("--6d=tools");

                    //Console.WriteLine("Sent BX2");

                    foreach (Tool t in tools)
                    {
                        log(t.ToString());
                        log(JsonConvert.SerializeObject(t.transform.position));
                        log(JsonConvert.SerializeObject(t.transform.orientation));
                        //log(t.transform.position.x.GetType().ToString());
                        //foreach (var m in t.markers)
                        //{
                        //    log(m.ToString());
                        //}
                        retVal.Add(t);
                    }
                }
            }
            else
            {
                throw new Exception("BX2 Not supported");
            }

            if (!cAPI.TrackingStop())
            {
                log("Could not stop tracking.");
            }
            //log("TrackingStopped");

            return retVal;
        }

        /// <summary>
        /// Sever communications with the motion capture.
        /// </summary>
        public void Disconnect()
        {
            if (!this.cAPI.Disconnect())
            {
                log("Could not disconnect.");
                return;
            }
            //log("Disconnected");
        }
        
        /// <summary>
        /// Get the 4x4 HTM from the camera's home frame to the tool's home frame
        /// </summary>
        /// <param name="tool">The tool in question</param>
        /// <returns>A 4x4 HTM from the camera to the tool</returns>
        public Matrix<double> GetToolHTM(Tool tool)
        {
            double x = tool.transform.position.x * 0.001;
            double y = tool.transform.position.y * 0.001;
            double z = tool.transform.position.z * 0.001;

            double q0 = tool.transform.orientation.q0;
            double qx = tool.transform.orientation.qx;
            double qy = tool.transform.orientation.qy;
            double qz = tool.transform.orientation.qz;

            Matrix<double> HTM = DenseMatrix.OfArray(new double[,] {
                        {2 * (Math.Pow(q0, 2) + Math.Pow(qx,2)) - 1, 2 * (qx*qy-q0*qz),2 * (qx*qz+q0*qy),x},
                        {2 * (qx*qy+q0*qz),2 * (Math.Pow(q0, 2) + Math.Pow(qy,2)) - 1,2 * (qy*qz-q0*qx),y},
                        {2 * (qx*qz-q0*qy),2 * (qy*qz+q0*qx),2 * (Math.Pow(q0, 2) + Math.Pow(qz,2)) - 1,z},
                        {0,0,0,1}});


            return HTM;
        }

        /// <summary>
        /// Get the 4x4 HTM from the camera's home frame to the stray's home frame
        /// Because a stray is a single point in space, it has no orientation. So,
        /// only position will be correct
        /// </summary>
        /// <param name="stray">The stray in question</param>
        /// <returns>The 4x4 HTM representing the translation from the camera to the stray</returns>
        public Matrix<double> GetStrayHTM(Marker stray)
        {
            double x = stray.position.x * 0.001;
            double y = stray.position.y * 0.001;
            double z = stray.position.z * 0.001;

            Matrix<double> HTM = DenseMatrix.OfArray(new double[,] {
                        {1,0,0,x},
                        {0,1,0,y},
                        {0,0,1,z},
                        {0,0,0,1}});


            return HTM;
        }

        /// <summary>
        /// Template function provided by the motion capture sensor manufacturer.
        /// Don't use this function.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            log("C# CAPI Sample v" + Capi.GetVersion());

            // Use the host specified if provided, otherwise:
            // 1st. try to find a serial device.
            // 2nd. Try to find a network device.
            Capi cAPI;
            string host = "";
            if (args.Length > 0)
            {
                host = args[0];
            }
            else
            {
                log("Finding serial NDI devices.");
                string[] comPorts = CapiSerial.GetAvailableComPorts();
                if (comPorts.Length > 0)
                {
                    host = comPorts[0];
                }
                else
                {
                    log("Could not find a serial device.");
                    log("Finding an NDI device on the network.");
                    var task = GetIP();
                    task.Wait(5000);

                    if (task.IsCompleted && task.Result != null)
                    {
                        host = task.Result;
                    }
                    else
                    {
                        log("Could not find an NDI device on the network.");
                    }
                }
            }

            if (host == "" || host == "help" || host == "-h" || host == "/?" || host == "--help")
            {
                // host argument was not provided and we couldn't find one
                if (host == "")
                {
                    log("Could not automatically detect an NDI device, please manually specify one.");
                }

                PrintHelp();
                return;
            }

            // Create a new CAPI instance based on the connection type
            if (host.StartsWith("COM") || host.StartsWith("/dev"))
            {
                cAPI = new CapiSerial(host);
            }
            else
            {
                cAPI = new CapiTcp(host);
            }

            Run(cAPI);
        }

        /// <summary>
        /// Print usage information
        /// </summary>
        public static void PrintHelp()
        {
            log("");
            log("usage: CAPISampleApplication.exe [<hostname>]");
            log("where:");
            log("\t<hostname>\t(optional) The measurement device's hostname, IP address, or serial port.");
            log("example hostnames:");
            log("\tConnecting to device by IP address: 169.254.8.50");
            log("\tConnecting to device by hostname: P9-B0103.local");
            log("\tConnecting to serial port varies by operating system:");
            log("\t\tCOM10 (Windows), /dev/ttyUSB0 (Linux), /dev/cu.usbserial-001014FA (Mac)");
        }

        /// <summary>
        /// Timestamped log output function.
        /// </summary>
        /// <param name="message">Message to be logged.</param>
        public static void log(string message)
        {
            if (!quietMode)
            {
                string time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                Console.WriteLine(time + " [CAPISample] " + message);
            }            
        }

        /// <summary>
        /// Find the first device on the network that is broadcasting the _ndi._tcp.local. service.
        /// </summary>
        /// <returns>Task with an IP string result.</returns>
        private static async Task<string> GetIP()
        {
            IReadOnlyList<IZeroconfHost> results = await ZeroconfResolver.ResolveAsync("_ndi._tcp.local.");
            foreach (var result in results)
            {
                return result.IPAddress;
            }

            return null;
        }

        /// <summary>
        /// Helper function to initialize communications with the mocap
        /// NOTE: YOU NEED TO LOAD ANY TOOL OBJECTS YOU WANT TO USE HERE
        /// </summary>
        /// <param name="cAPI">The API object</param>
        /// <returns>True if success</returns>
        private static bool InitializePorts(Capi cAPI)
        {
            // Polaris Section
            // ---
            // Request a new tool port handle so that we can load an SROM
            Port homeFrameTool = cAPI.PortHandleRequest();
            if (homeFrameTool == null)
            {
                log("Could not get available port for tool.");
            }
            else if (!homeFrameTool.LoadSROM("sroms/home_frame2.rom"))
            {
                log("Could not load SROM file for tool.");
                return false;
            }

            //Port tipTool = cAPI.PortHandleRequest();
            //if (tipTool == null)
            //{
            //    log("Could not get available port for tool.");
            //}
            //else if (!tipTool.LoadSROM("sroms/tip.rom"))
            //{
            //    log("Could not load SROM file for tool.");
            //    return false;
            //}
            // ---

            // Initialize all ports not currently initialized
            var ports = cAPI.PortHandleSearchRequest(PortHandleSearchType.NotInit);
            foreach (var port in ports)
            {
                if (!port.Initialize())
                {
                    log("Could not initialize port " + port.PortHandle + ".");
                    return false;
                }

                if (!port.Enable())
                {
                    log("Could not enable port " + port.PortHandle + ".");
                    return false;
                }
            }

            // List all enabled ports
            log("Enabled Ports:");
            ports = cAPI.PortHandleSearchRequest(PortHandleSearchType.Enabled);
            foreach (var port in ports)
            {
                port.GetInfo();
                log(port.ToString());
            }

            return true;
        }

        /// <summary>
        /// Check for BX2 command support.
        /// </summary>
        /// <param name="apiRevision">API revision string returned by CAPI.GetAPIRevision()</param>
        /// <returns>True if supported.</returns>
        private static bool IsBX2Supported(string apiRevision)
        {
            // Refer to the API guide for how to interpret the APIREV response
            char deviceFamily = apiRevision[0];
            int majorVersion = int.Parse(apiRevision.Substring(2, 3));

            // As of early 2017, the only NDI device supporting BX2 is the Vega
            // Vega is a Polaris device with API major version 003
            if (deviceFamily == 'G' && majorVersion >= 3)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Run the CAPI sample regardless of the connection method.
        /// </summary>
        /// <param name="cAPI">The configured CAPI protocol.</param>
        private static void Run(Capi cAPI)
        {
            // Be Verbose
            // cAPI.LogTransit = true;

            // Use the same log output format as this sample application
            cAPI.SetLogger(log);

            if (!cAPI.Connect())
            {
                log("Could not connect to " + cAPI.GetConnectionInfo());
                PrintHelp();
                return;
            }
            log("Connected");

            // Get the API Revision this will tell us if BX2 is supported.
            string revision = cAPI.GetAPIRevision();
            log("Revision:" + revision);

            if (!cAPI.Initialize())
            {
                log("Could not initialize.");
                return;
            }
            log("Initialized");

            // The Frame Frequency may not be possible to set on all devices, so an error response is okay.
            cAPI.SetUserParameter("Param.Tracking.Frame Frequency", "60");
            cAPI.SetUserParameter("Param.Tracking.Track Frequency", "2");

            // Read the final values
            log(cAPI.GetUserParameter("Param.Tracking.Frame Frequency"));
            log(cAPI.GetUserParameter("Param.Tracking.Track Frequency"));

            // Initialize tool ports
            if (!InitializePorts(cAPI))
            {
                return;
            }

            if (!cAPI.TrackingStart())
            {
                log("Could not start tracking.");
                return;
            }
            log("TrackingStarted");

            // Track several frames of data using the BX command.
            for (int i = 0; i < 300; i++)
            {
                if (!cAPI.IsConnected)
                {
                    log("Disconnected while tracking.");
                    break;
                }

                List<Tool> tools = cAPI.SendBX();
                foreach (var t in tools)
                {
                    log(t.ToString());
                }
            }

            // Track several frames of data using the BX2 Command
            if (IsBX2Supported(revision))
            {
                for (int i = 0; i < 300; i++)
                {
                    if (!cAPI.IsConnected)
                    {
                        log("Disconnected while tracking.");
                        break;
                    }

                    List<Tool> tools = cAPI.SendBX2("--6d=tools");
                    foreach (var t in tools)
                    {
                        log(t.ToString());
                        //foreach (var m in t.markers)
                        //{
                        //    log(m.ToString());
                        //}
                    }
                }
            }

            if (!cAPI.TrackingStop())
            {
                log("Could not stop tracking.");
                return;
            }
            log("TrackingStopped");

            if (!cAPI.Disconnect())
            {
                log("Could not disconnect.");
                return;
            }
            log("Disconnected");
        }
    }
}
