using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

namespace CAPISampleApplication
{
    // This class handles communication with the CTR's encoders and 
    // conversion of encoder outputs to usable data
    internal class Encoders
    {
        // This internal class stores the characteristics of a specific motor encoder
        // For translation motors:  encoder output = m * millimeters traveled   + b
        // For rotation motors:     encoder output = m * degrees traveled       + b
        internal class EncoderChars
        {
            public double m { get; set; }
            public double b { get; set; }

            public EncoderChars(double slope, double intercept)
            {
                m = slope;
                b = intercept;
            }
        }
        
        // These six EncoderChars objects were created based on experimental 
        // encoder characterizations (plotting mm or deg traveled vs encoder counts reported
        // and performing linear regression). Future projects that involve different hardware
        // should re-do the encoder characterizations for best accuracy.
        private EncoderChars r1 = new EncoderChars(-6.2224, -0.683);
        private EncoderChars r2 = new EncoderChars(-6.2501, 0.1);
        private EncoderChars r3 = new EncoderChars(-6.209, 2.529);

        private EncoderChars t1 = new EncoderChars(-673.98, 4.3402);
        private EncoderChars t2 = new EncoderChars(-675, -0.1999);
        private EncoderChars t3 = new EncoderChars(-675, 0.3668);

        // The COM Port via which we talk to the encoders
        public SerialPort port { get; set; }

        /// <summary>
        /// Constructor for Encoders class
        /// </summary>
        public Encoders()
        {

        }

        /// <summary>
        /// Constructor for Encoders class
        /// </summary>
        /// <param name="COMPort">COM Port through which the program communicates to the encoders</param>
        public Encoders(int COMPort)
        {
            // Create a SerialPort object to handle comms
            this.port = new SerialPort("COM" + COMPort.ToString(), 9600);
        }

        /// <summary>
        /// Initializes communications with the encoders 
        /// </summary>
        /// <param name="COMPort">The integer number of the COM Port that the robot is plugged into</param>
        /// <returns>true if the communication is successfully established, false otherwise</returns>
        public bool OpenComms(int COMPort)
        {
            try
            {
                this.port = new SerialPort("COM" + COMPort.ToString(), 9600);
                this.port.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            return true;

        }

        /// <summary>
        /// Initializes communications with the encoders 
        /// </summary>
        /// <returns>true if the communication is successfully established, false otherwise</returns>
        public bool OpenComms()
        {
            try
            {
                this.port.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            return true;

        }

        /// <summary>
        /// Severs communications with the robot 
        /// </summary>
        /// <returns>true if the communication is successfully killed, false otherwise</returns>
        public bool CloseComms()
        {
            try
            {
                this.port.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
            }

            return true;

        }

        /// <summary>
        /// Get the raw string output from the encoders.
        /// </summary>
        /// <param name="valToSend">The string that will be sent to
        /// the encoders via serial wire.</param>
        /// <returns>The response from the encoders. 
        /// Will be an array (length 6) of doubles, separated by commas.
        /// Array will contain encoder readings in the order t1,t2,t3,r1,r2,r3.</returns>
        public string GetEncoderString(string valToSend)
        {
            this.port.Write(valToSend);

            string value = this.port.ReadLine();

            return value;
        }

        /// <summary>
        /// Gets the joint space configuration of the current robot by reading the encoders
        /// and converting the output into joint space (units in mm for translation, deg for rotation)
        /// </summary>
        /// <returns>An array of doubles containing the current joint variables of the robot.</returns>
        public double[] GetAllEncoders()
        {
            // Get the raw readings from the encoders
            string allEncs = GetEncoderString("r");

            // Separate the string into 6 substrings (one per motor)
            string[] encList = allEncs.Split(',');

            // Initialize an array to hold the output
            double[] retVal = new double[] {0,0,0,0,0,0};

            // An array denoting the order in which encoder readings are returned
            string[] motors = new string[] { "t1", "t2", "t3", "r1", "r2", "r3" };

            // Convert the raw encoder readings into mm and deg
            for (int i = 0; i < 6; i++)
            {
                // Convert the string into a double
                double val = Double.Parse(encList[i]);

                // Convert the encoder reading into mm (translation) or deg (rotation)
                retVal[i] = ConvertEncoderValue(val, motors[i]);
            }

            // Return the array of 6 joint space coords
            return retVal;
        }

        /// <summary>
        /// Resets all encoders to 0.0
        /// </summary>
        /// <returns>True</returns>
        public bool ResetEncoders()
        {
            this.port.Write("x");

            string value = this.port.ReadLine();

            return true;
        }

        /// <summary>
        /// Convert a raw encoder reading to a joint space value (mm for translation, deg for rotation)
        /// </summary>
        /// <param name="rawRead">The raw encoder reading</param>
        /// <param name="motor">The motor associated with the reading. Must be one of "t1" "t2" "t3" "r1" "r2" "r3"</param>
        /// <returns>The converted reading</returns>
        /// <exception cref="Exception">If the motor is invalid</exception>
        public double ConvertEncoderValue(double rawRead, string motor)
        {
            EncoderChars encoderChars = null;

            // Select the right EncoderChars object
            if      (motor.Equals("r1")) encoderChars = this.r1;
            else if (motor.Equals("r2")) encoderChars = this.r2;
            else if (motor.Equals("r3")) encoderChars = this.r3;
            else if (motor.Equals("t1")) encoderChars = this.t1;
            else if (motor.Equals("t2")) encoderChars = this.t2;
            else if (motor.Equals("t3")) encoderChars = this.t3;
            else    throw new Exception("invalid motor code.");

            // Convert the encoder reading to mm or deg using the selected EncoderChars regression results
            double converted = (rawRead - encoderChars.b) / encoderChars.m;

            return converted;
        }

    }
}
