//----------------------------------------------------------------------------
//
// Copyright 2020 by Northern Digital Inc.
// All Rights Reserved
//
//----------------------------------------------------------------------------
// By using this Sample Code the licensee agrees to be bound by the
// provisions of the agreement found in the LICENSE.txt file.
using CAPISampleApplication;

// System
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Diagnostics;

// 3rd Party
using Zeroconf;
using Newtonsoft.Json;

// Matrix Algebra Library
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Data.Text;
using MathNet.Numerics.Distributions;

// NDI
using NDI.CapiSample;
using NDI.CapiSample.Data;
using NDI.CapiSample.Protocol;
using CAPISampleApplication;
using MathNet.Numerics.Interpolation;
using System.Timers;

namespace NDI.CapiSampleApplication
{
    /// <summary>
    /// This sample program showcases a common connection process to receive pose data for Aurora and Vega products.
    /// </summary>
    class Program
    {
        public static Robot Robot { get; set; }

        public static double[] JointVars {  get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello world");

            //Tube tube1 = new Tube(2.792 * Math.Pow(10, -3), 3.30 * Math.Pow(10, -3), 1 / 19.8, 90 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            //Tube tube2 = new Tube(2.132 * Math.Pow(10, -3), 2.64 * Math.Pow(10, -3), 1 / 14.7, 170 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            //Tube tube3 = new Tube(1.473 * Math.Pow(10, -3), 1.98 * Math.Pow(10, -3), 1 / 13.7, 250 * Math.Pow(10, -3), 60 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            //Tube[] tubes = new Tube[] { tube1, tube2, tube3 };
            //Robot robot = new Robot(tubes);

            //robot.OpenComms(3);

            //robot.TravelTo(new double[] { 0, 0, 0, 0, 0, 0 });

            //robot.CloseComms();

            try
            {
                StraighteningTest();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }


            Console.WriteLine("done");
            Thread.Sleep(100000000);

            return;
        }

        public static void MocapExample()
        {
            // Init the mocap. You should do this once at the beginning of the program.

            Mocap mocap = new Mocap();

            Console.WriteLine("Initting mocap");

            try
            {
                mocap.Init();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            // You can get the objects the motion capture sees using these functions:

            // This function finds tools (sets of 3+ balls in a pre-determined configuration)
            List<Tool> tools = mocap.FindTools();

            // This function finds any reflective objects not associated with a tool
            List<Marker> markers = mocap.FindStrays();

            // You can get the transformation matrix associated with a tool
            Tool tool1 = tools[0];
            Matrix<double> toolMatrix = mocap.GetToolHTM(tool1);

            // It can sometimes be hard to tell which stray is which.
            // But if you know where you expect the stray to be, you can extract the stray closest
            // to the expected position from the list.
            double[] expectedPosition = new double[] { 1, 2, 3 };
            Marker correctStray = mocap.GetCorrectStray(markers, expectedPosition);

            // You can get the transformation matrix associated with a stray
            Matrix<double> strayMatrix = mocap.GetStrayHTM(correctStray);

            // Before you finish, you need to disconnect the motion capture
            mocap.Disconnect();

        }

        public static void RobotExample()
        {
            // Initialize the robot object
            Tube tube1 = new Tube(2.792 * Math.Pow(10, -3), 3.30 * Math.Pow(10, -3), 1 / 19.8, 90 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube2 = new Tube(2.132 * Math.Pow(10, -3), 2.64 * Math.Pow(10, -3), 1 / 14.7, 170 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube3 = new Tube(1.473 * Math.Pow(10, -3), 1.98 * Math.Pow(10, -3), 1 / 13.7, 250 * Math.Pow(10, -3), 60 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube[] tubes = new Tube[] { tube1, tube2, tube3 };
            Robot robot = new Robot(tubes);

            // Tell the robot to move.
            // You need to open and close comms every time you send a command to the robot.
            double[] target = new double[] { 0, -70, 0, 0, 0, 0 };
            robot.OpenComms(3);
            robot.TravelTo(target);
            robot.CloseComms();

            // Get the FK for a given joint config.
            Matrix<double> fk = robot.Fkin(target);

        }

        public static void EncoderExample()
        {
            // Initialize the encoders
            int encoderCom = 4;
            Encoders encs = new Encoders(encoderCom);

            // Reset the encoders.
            // Remember to open and close comms every time you talk to the encoders.
            encs.OpenComms();
            encs.ResetEncoders();
            encs.CloseComms();

            // Get the current joint values from the encoders.
            // Output of GetAllEncoders() is an array of 6 doubles in the order
            // [t1, t2, t3, r1, r2, r3]. Translation units are mm, rotation units are deg.
            encs.OpenComms();
            double[] currentJoints = encs.GetAllEncoders();
            encs.CloseComms();
        }

        public static void StraighteningTest()
        {
            Tube tube1 = new Tube(2.792 * Math.Pow(10, -3), 3.30 * Math.Pow(10, -3), 1 / 19.8, 90 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube2 = new Tube(2.132 * Math.Pow(10, -3), 2.64 * Math.Pow(10, -3), 1 / 14.7, 170 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube3 = new Tube(1.473 * Math.Pow(10, -3), 1.98 * Math.Pow(10, -3), 1 / 13.7, 250 * Math.Pow(10, -3), 60 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube[] tubes = new Tube[] { tube1, tube2, tube3 };
            Robot robot = new Robot(tubes);

            int cycleCount = 5;
            int timeDelayMS = 50 * 1000;
            double[] target = new double[] { 0, -70, 0, 0, 0, 0 };
            double[] home = new double[] { 0, 0, 0, 0, 0, 0 };

            Console.WriteLine("Beginning straightening test");

            for (int i = 0; i < cycleCount; i++)
            {
                Console.WriteLine("beginning cycle " + i.ToString());
                
                robot.OpenComms(3);
                robot.TravelTo(target);
                robot.CloseComms();

                Thread.Sleep(timeDelayMS);

                robot.OpenComms(3);
                robot.TravelTo(home);
                robot.CloseComms();

                Thread.Sleep(timeDelayMS);
            }
        }

        /// <summary>
        /// Kinematics test
        /// </summary>
        public static void KineTest()
        {
            // Read data from previous tests

            string directory = "./results/";

            string measurementPath = directory + "measurements.csv";
            string encoderReadingsPath = directory + "encoder_readings.csv";

            // Load in the encoder readings

            List<double[]> encoderReadings = new List<double[]>();

            using (TextFieldParser parser = new TextFieldParser(encoderReadingsPath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    double[] doubles = Array.ConvertAll(fields, s => double.Parse(s, CultureInfo.InvariantCulture));
                    encoderReadings.Add(doubles);
                }
            }

            // Load in the mocap readings

            List<double[]> measurements = new List<double[]>();

            using (TextFieldParser parser = new TextFieldParser(measurementPath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    double[] doubles = Array.ConvertAll(fields, s => double.Parse(s, CultureInfo.InvariantCulture));
                    measurements.Add(doubles);
                }
            }

            // Robot setup

            double[] k_initial = new double[] { 5.59476, 8.28509, 10.91136 };

            Tube tube1 = new Tube(2.792 * Math.Pow(10, -3), 3.30 * Math.Pow(10, -3), 1 / k_initial[0], 90 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube2 = new Tube(2.132 * Math.Pow(10, -3), 2.64 * Math.Pow(10, -3), 1 / k_initial[1], 170 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube3 = new Tube(1.473 * Math.Pow(10, -3), 1.98 * Math.Pow(10, -3), 1 / k_initial[2], 255 * Math.Pow(10, -3), 60 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube[] tubes = new Tube[] { tube1, tube2, tube3 };
            Program.Robot = new Robot(tubes);

            double[] q0 = new double[] { 0, 0, 0, 0, 0, 0 };
            Program.JointVars = q0;

            //Console.WriteLine(Vector<double>.Build.Dense(q0));

            //Matrix<double> tipFK = Program.Robot.Fkin(q0);
            //Matrix<double> intermediateFK = Program.Robot.Fkin_Modified(q0);

            //Console.WriteLine(tipFK);
            //Console.WriteLine(intermediateFK);

            for (int i = 0; i < measurements.Count; i++)
            {
                double[] currEncoder = encoderReadings[i];
                double[] currMeasurement = measurements[i];

                Console.WriteLine("curr encoder");
                Console.WriteLine(string.Join(", ", currEncoder));

                Program.JointVars = currEncoder;

                Matrix<double> tipFK = Program.Robot.Fkin(currEncoder);
                Matrix<double> intermediateFK = Program.Robot.Fkin_Modified(currEncoder, 0);

                //Console.WriteLine(tipFK);
                Console.WriteLine("intermediate marker htm");
                Console.WriteLine(intermediateFK);

                var name = Console.ReadLine();

            }
        }

        /// <summary>
        /// Run the filter offline, using data collected previously.
        /// Helpful for testing filter tweaks without having to collect new data on the real robot
        /// </summary>
        public static void OfflineExperiment()
        {
            // place to save data

            List<double[]> filteredStates = new List<double[]>() { };
            List<double[]> predictedStates = new List<double[]>() { };
            List<double[,]> covars = new List<double[,]>() { };

            // Read data from previous tests

            string directory = "./results/";

            string measurementPath = directory + "measurements.csv";
            string encoderReadingsPath = directory + "encoder_readings.csv";

            // Load in the encoder readings and mocap readings

            List<double[]> encoderReadings = ReadListOfArrayFromCSV(encoderReadingsPath);
            List<double[]> measurements = ReadListOfArrayFromCSV(measurementPath);

            // Robot setup

            double[] k_initial = new double[] { 12.79, 15.95 };
            
            //Tube tube1 = new Tube(2.792 * Math.Pow(10, -3), 3.30 * Math.Pow(10, -3), 1 / k_initial[0], 90 * Math.Pow(10, -3), 40 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube2 = new Tube(2.132 * Math.Pow(10, -3), 2.64 * Math.Pow(10, -3), 1 / k_initial[0], 170 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube3 = new Tube(1.473 * Math.Pow(10, -3), 1.98 * Math.Pow(10, -3), 1 / k_initial[1], 255 * Math.Pow(10, -3), 60 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube[] tubes = new Tube[] { tube2, tube3 };
            Program.Robot = new Robot(tubes);

            double[] q0 = new double[] { 0,0,0, 0 };
            Program.JointVars = q0;

            double[] initialXYZOffset1 = new double[] { 0, 0, 0 };
            double[] initialXYZOffset2 = new double[] { 0, 0, 0 };
            double[] initialXYZOffset3 = new double[] { 0, 0, 0 };
            
            // Set up the EKF

            int numStateVars = 14;
            int numMeasurementVars = 6;

            Console.WriteLine("initting ekf");

            double e_n2 = (double)Math.Pow(10, -2);
            double e_n3 = 2.5 * (double)Math.Pow(10, -3);
            double e_n4 = (double)Math.Pow(10, -4);
            double e_n5 = 2.5 * (double)Math.Pow(10, -5);
            double e_n6 = (double)Math.Pow(10, -6);
            double e_n8 = (double)Math.Pow(10, -8);
            double e_n10 = (double)Math.Pow(10, -10);
            double e_n12 = (double)Math.Pow(10, -12);

            Matrix<double> ProcessNoise = Matrix<double>.Build.DenseOfDiagonalArray(new double[] { e_n6, e_n6, e_n6,     e_n6, e_n6, e_n6,
                                                                                                   e_n6, e_n6,
                                                                                                   e_n10, e_n10, e_n10,  e_n10, e_n10, e_n10}); 

            Matrix<double> MeasurementNoise = Matrix<double>.Build.DenseOfDiagonalArray(new double[] { e_n6, e_n6, e_n6, e_n6, e_n6, e_n6 }); 

            Matrix<double> InitialCovariance = Matrix.Build.DenseDiagonal(numStateVars, numStateVars, (double)1.0);
            
            EKF filter = new EKF(numStateVars, numMeasurementVars);
            //UKF filter = new UKF(numStateVars, numMeasurementVars, 1, 0, 3-numStateVars);

            // Figure out the HTM associated with the current joint variables

            //Matrix<double> tube1FK = Program.Robot.Fkin_Modified(q0, 0);
            Matrix<double> tube2FK = Program.Robot.Fkin_Modified(q0, 0);
            Matrix<double> tube3FK = Program.Robot.Fkin(q0);
            
            double[] initialGuessState = new double[] { tube2FK[0, 3], tube2FK[1, 3], tube2FK[2, 3], tube3FK[0, 3], tube3FK[1, 3], tube3FK[2, 3], k_initial[0], k_initial[1], 0,0,0,  0,0,0 };

            // Init the EKF

            filter.Initialize(ProcessNoise, MeasurementNoise, Vector<double>.Build.Dense(initialGuessState), InitialCovariance);

            // EKF State Model

            Func<double[], double[]> stateModel = StateModelTwoTubesTwoBallsFullHoming;
            List<Func<double[], double>> individualModels = new List<Func<double[], double>>() { X1ModelTwoTubesTwoBallsFullHoming, Y1ModelTwoTubesTwoBallsFullHoming, Z1ModelTwoTubesTwoBallsFullHoming, X2ModelTwoTubesTwoBallsFullHoming, Y2ModelTwoTubesTwoBallsFullHoming, Z2ModelTwoTubesTwoBallsFullHoming, K1ModelTwoTubesTwoBallsFullHoming, K2ModelTwoTubesTwoBallsFullHoming, X1OffsetModelTwoTubesTwoBallsFullHoming, Y1OffsetModelTwoTubesTwoBallsFullHoming, Z1OffsetModelTwoTubesTwoBallsFullHoming, X2OffsetModelTwoTubesTwoBallsFullHoming, Y2OffsetModelTwoTubesTwoBallsFullHoming, Z2OffsetModelTwoTubesTwoBallsFullHoming };

            Func<double[], double[]> measurementModel = MeasurementModelThreeTubes;

            for (int i = 0; i < measurements.Count; i++)
            {
                double[] currEncoder = encoderReadings[i];
                double[] currMeasurement = measurements[i];

                Program.JointVars = currEncoder;

                // Run the EKF: prediction step

                filter.Predict(stateModel, individualModels);

                double[] predictedState = filter.CurrState.ToArray();

                // Run the EKF: correction step

                filter.Correct(Vector<double>.Build.Dense(currMeasurement));

                // Extract info from the EKF

                double[] filteredState = filter.CurrState.ToArray();
                double[,] covar = filter.Covariance.ToArray();

                predictedStates.Add(predictedState);
                filteredStates.Add(filteredState);
                covars.Add(covar);
            }

            // save data
            {
                string predictedStatesPath = directory + "predicted_states.csv";
                string filteredStatesPath = directory + "filtered_states.csv";
                string covarPath = directory + "covariances.csv";

                WriteListOfArrayToCSV(filteredStates, filteredStatesPath);
                WriteListOfArrayToCSV(predictedStates, predictedStatesPath);
                WriteListOf2DArrayToCSV(covars, covarPath);
            }
        }

        public static void ThreeTubeThreeBallExperiment()
        {
            // Communication details

            int robotCom = 3;
            int encoderCom = 4;

            // Set up lists to record data

            List<double> times = new List<double>() { };
            List<double[]> encoderReadings = new List<double[]>() { };
            List<double[]> jointTargets = new List<double[]>() { };
            List<double[]> measurements = new List<double[]>() { };
            List<double[]> homeFramePositions = new List<double[]>() { };
            List<double[]> tube3StrayPositions = new List<double[]>() { };
            List<double[]> tube1StrayPositions = new List<double[]>() { };
            List<double[]> tube2StrayPositions = new List<double[]>() { };
            List<double[]> filteredStates = new List<double[]>() { };
            List<double[]> predictedStates = new List<double[]>() { };
            List<double[,]> covars = new List<double[,]>() { };

            // Set up the mocap

            Mocap mocap = new Mocap();

            Console.WriteLine("Initting mocap");

            try
            {
                mocap.Init();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Thread.Sleep(10000000);
            }

            double[] tube1StrayPosition = new double[] { .1065, .0754, -1.399 };
            double[] tube2StrayPosition = new double[] { .1039, .0672, -1.398 };
            double[] tube3StrayPosition = new double[] { .1002, .0556, -1.396 };

            // Encoder setup

            Console.WriteLine("initting encoders");
            Encoders encs = new Encoders(encoderCom);

            // Random number generator

            Random rnd = new Random();

            // Robot setup

            double[] k_initial = new double[] { 4.86, 6.64, 7.51};

            Tube tube1 = new Tube(2.792 * Math.Pow(10, -3), 3.30 * Math.Pow(10, -3), 1 / k_initial[0], 90 * Math.Pow(10, -3), 40 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube2 = new Tube(2.132 * Math.Pow(10, -3), 2.64 * Math.Pow(10, -3), 1 / k_initial[1], 170 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube3 = new Tube(1.473 * Math.Pow(10, -3), 1.98 * Math.Pow(10, -3), 1 / k_initial[2], 255 * Math.Pow(10, -3), 60 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube[] tubes = new Tube[] { tube1, tube2, tube3 };
            Program.Robot = new Robot(tubes);

            double[] q0 = new double[] { 0, 0, 0, 0, 0, 0 };
            Program.JointVars = q0;

            // Set up the EKF

            int numStateVars = 21;
            int numMeasurementVars = 9;

            Console.WriteLine("initting ekf");

            double e_n2 = (double)Math.Pow(10, -2);
            double e_n4 = (double)Math.Pow(10, -4);
            double e_n6 = (double)Math.Pow(10, -6);
            double e_n10 = (double)Math.Pow(10, -10);

            Matrix<double> ProcessNoise = Matrix<double>.Build.DenseOfDiagonalArray(new double[] { e_n6, e_n6, e_n6,     e_n6, e_n6, e_n6,       e_n6, e_n6, e_n6,
                                                                                                   e_n6, e_n6, e_n6,
                                                                                                   e_n10, e_n10, e_n10,  e_n10, e_n10, e_n10,    e_n10, e_n10, e_n10 });

            Matrix<double> MeasurementNoise = Matrix<double>.Build.DenseOfDiagonalArray(new double[] { e_n6, e_n6, e_n6, e_n6, e_n6, e_n6, e_n6, e_n6, e_n6 });

            Matrix<double> InitialCovariance = Matrix.Build.DenseDiagonal(numStateVars, numStateVars, 1.0);

            EKF ekf = new EKF(numStateVars, numMeasurementVars);

            // Figure out the HTM associated with the current joint variables

            Matrix<double> tube1HTM = Program.Robot.Fkin_Modified(q0, 0);
            Matrix<double> tube2HTM = Program.Robot.Fkin_Modified(q0, 1);
            Matrix<double> tube3HTM = Program.Robot.Fkin(q0);

            double[] initialGuessState = new double[] { tube1HTM[0, 3], tube1HTM[1, 3], tube1HTM[2, 3], tube2HTM[0, 3], tube2HTM[1, 3], tube2HTM[2, 3], tube3HTM[0, 3], tube3HTM[1, 3], tube3HTM[2, 3], k_initial[0], k_initial[1], k_initial[2], 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            // Init the EKF

            ekf.Initialize(ProcessNoise, MeasurementNoise, Vector<double>.Build.Dense(initialGuessState), InitialCovariance);

            // EKF State Model

            Func<double[], double[]> stateModel = StateModelThreeTubesThreeBallsFullHoming;
            List<Func<double[], double>> individualModels = new List<Func<double[], double>>() { X1ModelThreeTubesThreeBallsFullHoming, Y1ModelThreeTubesThreeBallsFullHoming, Z1ModelThreeTubesThreeBallsFullHoming, X2ModelThreeTubesThreeBallsFullHoming, Y2ModelThreeTubesThreeBallsFullHoming, Z2ModelThreeTubesThreeBallsFullHoming, X3ModelThreeTubesThreeBallsFullHoming, Y3ModelThreeTubesThreeBallsFullHoming, Z3ModelThreeTubesThreeBallsFullHoming, K1ModelThreeTubesThreeBallsFullHoming, K2ModelThreeTubesThreeBallsFullHoming, K3ModelThreeTubesThreeBallsFullHoming, X1OffsetModelThreeTubesThreeBallsFullHoming, Y1OffsetModelThreeTubesThreeBallsFullHoming, Z1OffsetModelThreeTubesThreeBallsFullHoming, X2OffsetModelThreeTubesThreeBallsFullHoming, Y2OffsetModelThreeTubesThreeBallsFullHoming, Z2OffsetModelThreeTubesThreeBallsFullHoming, X3OffsetModelThreeTubesThreeBallsFullHoming, Y3OffsetModelThreeTubesThreeBallsFullHoming, Z3OffsetModelThreeTubesThreeBallsFullHoming };

            // Calculate the joint configurations

            List<double[]> jointConfigs = new List<double[]>() { };

            //int numTestPoints = 25;

            //Console.WriteLine("getting joint configs.");

            //for (int i = 0; i < numTestPoints; i++)
            //{
            //    int rho1 = rnd.Next(0, 25);
            //    int rho2 = rnd.Next(rho1, rho1 + 45);
            //    int rho3 = rnd.Next(rho2, Math.Min(rho2 + 45, rho1 + 75));

            //    jointConfigs.Add(new double[] { rho1, rho2, rho3, 0, 0, 0});
            //}

            //jointConfigs.Add(new double[] { 0, 0, 0, 0, 0, 0 });
            for(int i = 0; i < 10; i++)
            {
                jointConfigs.Add(new double[] { 0, 0, 30, 0, 0, 0 });
                jointConfigs.Add(new double[] { 0, 30, 30, 0, 0, 0 });
                jointConfigs.Add(new double[] { 0, 30, 60, 0, 0, 0 });
                jointConfigs.Add(new double[] { 20, 30, 60, 0, 0, 0 });
                jointConfigs.Add(new double[] { 20, 40, 60, 0, 0, 0 });
                jointConfigs.Add(new double[] { 0, 0, 0, 0, 0, 0 });
                jointConfigs.Add(new double[] { 0, 30, 30, 0, 0, 0 });
                jointConfigs.Add(new double[] { 30, 30, 60, 0, 0, 0 });
                jointConfigs.Add(new double[] { 0, 0, 0, 0, 0, 0 });
            }
            // Reset the encoders

            encs.OpenComms();

            encs.ResetEncoders();

            encs.CloseComms();

            Thread.Sleep(1000);

            encs.OpenComms();

            double[] allJointsArray = encs.GetAllEncoders();

            encs.CloseComms();

            Thread.Sleep(1000);

            // Start the timer

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // For each joint configuration...
            for (int position = 0; position < jointConfigs.Count; position++)
            {
                double[] currentJointConfig = jointConfigs[position];

                Console.WriteLine("Moving to position " + position.ToString());
                foreach (var jointConfig in currentJointConfig)
                {
                    Console.Write(jointConfig.ToString() + ",");
                }
                Console.WriteLine();

                // Tell the robot to go to the joint target

                Program.Robot.OpenComms(robotCom);

                Program.Robot.TravelTo(currentJointConfig);

                Program.Robot.CloseComms();

                // Get the latest time

                long millis = sw.ElapsedMilliseconds;

                times.Add(millis);

                // Read the current joint space configuration using the encoders

                double[] prevEncoders = allJointsArray;

                encs.OpenComms();

                allJointsArray = encs.GetAllEncoders();

                encs.CloseComms();

                // Filter out the crap
                for (int i = 0; i < prevEncoders.Length; i++)
                {
                    if (Math.Abs(allJointsArray[i] / prevEncoders[i]) > 10 && Math.Abs(allJointsArray[i]) > 10)
                    {
                        allJointsArray[i] = prevEncoders[i];
                    }
                }

                Program.JointVars = allJointsArray;

                encoderReadings.Add(allJointsArray);
                jointTargets.Add(jointConfigs[position]);

                // Get the difference between the target position and the current position

                Vector<double> targetJoints = Vector<double>.Build.Dense(jointConfigs[position]);
                Vector<double> currentJointsVector = Vector<double>.Build.Dense(allJointsArray);

                Vector<double> diff = currentJointsVector - targetJoints;

                double diffNorm = diff.L2Norm();
                double prevNorm = diffNorm + 1000;

                // Figure out the HTM associated with the current joint variables

                //encoderFK = Program.Robot.Fkin(currentJointsArray);

                // Get the mocap data

                List<Marker> strays = mocap.FindStrays();
                List<Tool> tools = mocap.FindTools();

                // Figure out which stray to use

                Marker tube1Stray = mocap.GetCorrectStray(strays, tube1StrayPosition);
                Marker tube2Stray = mocap.GetCorrectStray(strays, tube2StrayPosition);
                Marker tube3Stray = mocap.GetCorrectStray(strays, tube3StrayPosition);

                while((tube1Stray.position.x == tube2Stray.position.x) || (tube3Stray.position.x == tube2Stray.position.x) || (tube1Stray.position.x == tube3Stray.position.x))
                {
                    strays = mocap.FindStrays();

                    tube1Stray = mocap.GetCorrectStray(strays, tube1StrayPosition);
                    tube2Stray = mocap.GetCorrectStray(strays, tube2StrayPosition);
                    tube3Stray = mocap.GetCorrectStray(strays, tube3StrayPosition);
                }

                // Do HTM math to figure out where the mocap thinks the tip position is

                //Console.WriteLine(strays[2].position.ToString());

                Matrix<double> tube1Matrix = mocap.GetStrayHTM(tube1Stray);
                Matrix<double> tube2Matrix = mocap.GetStrayHTM(tube2Stray);
                Matrix<double> tube3Matrix = mocap.GetStrayHTM(tube3Stray);
                Matrix<double> homeMatrix = mocap.GetToolHTM(tools[0]);

                double[] homeFramePosition = new double[] { homeMatrix[0, 3], homeMatrix[1, 3], homeMatrix[2, 3] };
                tube1StrayPosition = new double[] { tube1Matrix[0, 3], tube1Matrix[1, 3], tube1Matrix[2, 3] };
                tube2StrayPosition = new double[] { tube2Matrix[0, 3], tube2Matrix[1, 3], tube2Matrix[2, 3] };
                tube3StrayPosition = new double[] { tube3Matrix[0, 3], tube3Matrix[1, 3], tube3Matrix[2, 3] };

                homeFramePositions.Add(homeFramePosition);
                tube1StrayPositions.Add(tube1StrayPosition);
                tube2StrayPositions.Add(tube2StrayPosition);
                tube3StrayPositions.Add(tube3StrayPosition);

                double[] prevHomeFramePos = homeFramePosition;

                Matrix<double> mocapTube1FK = homeMatrix.Inverse() * tube1Matrix;
                Matrix<double> mocapTube2FK = homeMatrix.Inverse() * tube2Matrix;
                Matrix<double> mocapTube3FK = homeMatrix.Inverse() * tube3Matrix;

                double[] measurement = new double[] { mocapTube1FK[0, 3], mocapTube1FK[1, 3], mocapTube1FK[2, 3], mocapTube2FK[0, 3], mocapTube2FK[1, 3], mocapTube2FK[2, 3], mocapTube3FK[0, 3], mocapTube3FK[1, 3], mocapTube3FK[2, 3] };

                measurements.Add(measurement);

                // Run the EKF: prediction step

                ekf.Predict(stateModel, individualModels);

                double[] predictedState = ekf.CurrState.ToArray();

                // Run the EKF: correction step

                ekf.Correct(Vector<double>.Build.Dense(measurement));

                // Extract info from the EKF

                double[] filteredState = ekf.CurrState.ToArray();
                double[,] covar = ekf.Covariance.ToArray();

                predictedStates.Add(predictedState);
                filteredStates.Add(filteredState);
                covars.Add(covar);

                // While we aren't there yet...

                while (prevNorm - diffNorm > 0.01 || diffNorm > 5)
                {
                    millis = sw.ElapsedMilliseconds;

                    // Read the current joint space configuration using the encoders

                    prevEncoders = allJointsArray;

                    encs.OpenComms();

                    allJointsArray = encs.GetAllEncoders();

                    encs.CloseComms();

                    // Filter out the crap
                    for (int i = 0; i < prevEncoders.Length; i++)
                    {
                        if (Math.Abs(allJointsArray[i] / prevEncoders[i]) > 10 && Math.Abs(allJointsArray[i]) > 10)
                        {
                            Console.WriteLine("bad encoder " + i.ToString());
                            allJointsArray[i] = prevEncoders[i];
                        }
                    }

                    Program.JointVars = allJointsArray;

                    // Get the difference between the target position and the current position

                    targetJoints = Vector<double>.Build.Dense(jointConfigs[position]);
                    currentJointsVector = Vector<double>.Build.Dense(allJointsArray);

                    diff = currentJointsVector - targetJoints;

                    prevNorm = diffNorm;
                    diffNorm = diff.L2Norm();

                    Console.WriteLine(diffNorm);
                    Console.WriteLine(currentJointsVector.ToString());

                    // Figure out the HTM associated with the current joint variables

                    //encoderFK = Program.Robot.Fkin(currentJointsArray);

                    // Get the mocap data

                    strays = mocap.FindStrays();
                    tools = mocap.FindTools();

                    // Figure out which stray in the list of strays is the right one by finding the stray closest to the prev stray position

                    tube1Stray = mocap.GetCorrectStray(strays, tube1StrayPosition);
                    tube2Stray = mocap.GetCorrectStray(strays, tube2StrayPosition);
                    tube3Stray = mocap.GetCorrectStray(strays, tube3StrayPosition);

                    // Do HTM math to figure out where the mocap thinks the tip position is

                    //Console.WriteLine(strays[2].position.ToString());

                    tube1Matrix = mocap.GetStrayHTM(tube1Stray);
                    tube2Matrix = mocap.GetStrayHTM(tube2Stray);
                    tube3Matrix = mocap.GetStrayHTM(tube3Stray);
                    homeMatrix = mocap.GetToolHTM(tools[0]);

                    homeFramePosition = new double[] { homeMatrix[0, 3], homeMatrix[1, 3], homeMatrix[2, 3] };

                    double homeFrameChange = homeFramePosition[0] - prevHomeFramePos[0];

                    // If we get a ridiculous value for the home frame position, we don't want to use it.
                    // Instead, we skip to the next timestep
                    if (Math.Abs(homeFrameChange) < Math.Abs(0.1 * prevHomeFramePos[0]) && (tube1Stray.position.x != tube2Stray.position.x) && (tube3Stray.position.x != tube2Stray.position.x) && (tube1Stray.position.x != tube3Stray.position.x))
                    {
                        tube1StrayPosition = new double[] { tube1Matrix[0, 3], tube1Matrix[1, 3], tube1Matrix[2, 3] };
                        tube2StrayPosition = new double[] { tube2Matrix[0, 3], tube2Matrix[1, 3], tube2Matrix[2, 3] };
                        tube3StrayPosition = new double[] { tube3Matrix[0, 3], tube3Matrix[1, 3], tube3Matrix[2, 3] };

                        mocapTube1FK = homeMatrix.Inverse() * tube1Matrix;
                        mocapTube2FK = homeMatrix.Inverse() * tube2Matrix;
                        mocapTube3FK = homeMatrix.Inverse() * tube3Matrix;
                        
                        measurement = new double[] { mocapTube1FK[0, 3], mocapTube1FK[1, 3], mocapTube1FK[2, 3], mocapTube2FK[0, 3], mocapTube2FK[1, 3], mocapTube2FK[2, 3], mocapTube3FK[0, 3], mocapTube3FK[1, 3], mocapTube3FK[2, 3] };

                        prevHomeFramePos = homeFramePosition;

                        // Run the EKF: prediction step

                        ekf.Predict(stateModel, individualModels);

                        predictedState = ekf.CurrState.ToArray();

                        // Run the EKF: correction step

                        ekf.Correct(Vector<double>.Build.Dense(measurement));

                        // Extract info from the EKF

                        filteredState = ekf.CurrState.ToArray();
                        covar = ekf.Covariance.ToArray();

                        // Save data

                        times.Add(millis);

                        encoderReadings.Add(allJointsArray);
                        jointTargets.Add(jointConfigs[position]);

                        homeFramePositions.Add(homeFramePosition);
                        tube1StrayPositions.Add(tube1StrayPosition);
                        tube2StrayPositions.Add(tube2StrayPosition);
                        tube3StrayPositions.Add(tube3StrayPosition);

                        measurements.Add(measurement);

                        predictedStates.Add(predictedState);
                        filteredStates.Add(filteredState);
                        covars.Add(covar);
                    }
                    else
                    {
                        Console.WriteLine("SKIPPING STEP");
                    }
                }
                Thread.Sleep(250);
            }

            // Save results
            {
                // Write the results to csvs

                string timesPath = "./results/" + "times.csv";
                string predictedStatesPath = "./results/" + "predicted_states.csv";
                string filteredStatesPath = "./results/" + "filtered_states.csv";
                string covarPath = "./results/" + "covariances.csv";
                string measurementPath = "./results/" + "measurements.csv";
                string jointTargetsPath = "./results/" + "joint_targets.csv";
                string encoderReadingsPath = "./results/" + "encoder_readings.csv";
                string jointConfigsPath = "./results/" + "joint_configs.csv";
                string homeFramePath = "./results/" + "home_frame_positions.csv";
                string tube3Path = "./results/" + "tube3_stray_positions.csv";
                string tube1Path = "./results/" + "tube1_stray_positions.csv";
                string tube2Path = "./results/" + "tube2_stray_positions.csv";

                WriteListToCSV(times, timesPath);
                WriteListOfArrayToCSV(homeFramePositions, homeFramePath);
                WriteListOfArrayToCSV(tube3StrayPositions, tube3Path);
                WriteListOfArrayToCSV(tube1StrayPositions, tube1Path);
                WriteListOfArrayToCSV(tube2StrayPositions, tube2Path);
                WriteListOfArrayToCSV(jointConfigs, jointConfigsPath);
                WriteListOfArrayToCSV(filteredStates, filteredStatesPath);
                WriteListOfArrayToCSV(predictedStates, predictedStatesPath);
                WriteListOfArrayToCSV(measurements, measurementPath);
                WriteListOfArrayToCSV(jointTargets, jointTargetsPath);
                WriteListOfArrayToCSV(encoderReadings, encoderReadingsPath);
                WriteListOf2DArrayToCSV(covars, covarPath);
            }

        }

        /// <summary>
        /// Encoder characterization test
        /// </summary>
        /// <param name="EncoderCOM"></param>
        /// <param name="RobotCOM"></param>
        public static void EncoderCharacterization(int EncoderCOM, int RobotCOM)
        {
            Tube tube1 = new Tube(2.792 * Math.Pow(10, -3), 3.30 * Math.Pow(10, -3), 1 / 19.8, 90 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube2 = new Tube(2.132 * Math.Pow(10, -3), 2.64 * Math.Pow(10, -3), 1 / 14.7, 170 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube3 = new Tube(1.473 * Math.Pow(10, -3), 1.98 * Math.Pow(10, -3), 1 / 13.7, 250 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube[] tubes = new Tube[] { tube1, tube2, tube3 };
            Robot robot = new Robot(tubes);

            Encoders encs = new Encoders();

            robot.OpenComms(RobotCOM);

            robot.TravelTo(new double[] { 0, 0, 0, 0, 0, 0 });

            Thread.Sleep(1000);

            robot.CloseComms();

            encs.OpenComms(EncoderCOM);

            encs.ResetEncoders();

            encs.CloseComms();


            for (int i = 0; i < 105; i += 5)
            {
                robot.OpenComms(RobotCOM);

                robot.TravelTo(new double[] { 0, 0, i, 0, 0, 0 });

                Thread.Sleep(7000);

                robot.CloseComms();

                encs.OpenComms(EncoderCOM);

                double[] encReadings = encs.GetAllEncoders();
                double value = encReadings[2];

                Console.WriteLine(value.ToString());

                encs.CloseComms();

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// EKF Simulation experiment
        /// </summary>
        public static void EKFSimulations()
        {
            Random rnd = new Random();

            double[] q0 = new double[] { 0, 0, 0, 0, 0, 0 };

            int numStateVars = 21;
            int numMeasurementVars = 9;

            double e_n2 = (double)Math.Pow(10, -2);
            double e_n4 = (double)Math.Pow(10, -4);
            double e_n5 = 2.5 * (double)Math.Pow(10, -5);
            double e_n6 = (double)Math.Pow(10, -6);
            double e_n7 = 2.5 * (double)Math.Pow(10, -7);
            double e_n8 = (double)Math.Pow(10, -8);
            double e_n10 = (double)Math.Pow(10, -10);

            // Set up the list of noise values from which we will draw permutations
            // "True" noises are the noise values we will use when simlating the true states
            // "EKF" noises are the noises the filter will use when estimating states

            double[] trueActuationNoises    = new double[] { e_n8, e_n7, e_n6, e_n5, e_n4 };
            double[] trueSensorNoises       = new double[] { e_n8, e_n7, e_n6, e_n5, e_n4 };
            double[] ekfActuationNoises     = new double[] { e_n8, e_n7, e_n6, e_n5, e_n4 };
            double[] ekfSensorNoises        = new double[] { e_n8, e_n7, e_n6, e_n5, e_n4 };

            // For each permutation...
            for (int a = 0; a < trueActuationNoises.Length; a++)
            {
                for (int b = 0;  b < trueSensorNoises.Length; b++)
                {
                    for(int c = 0; c < ekfActuationNoises.Length; c++)
                    {
                        for(int d = 0; d < ekfSensorNoises.Length; d++)
                        {
                            // For now, ensure the ekf uses the same noise value as the true states
                            if(a==c && b==d)
                            {
                                // Run 100 trials of the robot moving to random targets as the ekf tries to learn the curvatures.
                                // Motion is in-plane only, no rotation allowed.

                                string configString = a.ToString() + "_" + b.ToString() + "_" + c.ToString() + "_" + d.ToString();
                                Console.WriteLine("Beginning trials for configuration: " + configString.ToString());

                                // Set up distributions to model the "true" noise of the system

                                double mean = 0;
                                double actuationXYZStDev = Math.Sqrt(trueActuationNoises[a]);
                                double actuationOffsetStDev = 1 * Math.Pow(10, -6);
                                double actuationKStDev = 1 * Math.Pow(10, -3);
                                double measurementXYZStDev = Math.Sqrt(trueSensorNoises[b]);

                                Normal actuationXYZDist = new Normal(mean, actuationXYZStDev);
                                Normal actuationOffsetDist = new Normal(mean, actuationOffsetStDev);
                                Normal actuationKDist = new Normal(mean, actuationKStDev);
                                Normal measurementXYZDist = new Normal(mean, measurementXYZStDev);

                                // EKF constants

                                Matrix<double> ProcessNoise = Matrix<double>.Build.DenseOfDiagonalArray(new double[] { ekfActuationNoises[c], ekfActuationNoises[c], ekfActuationNoises[c],     ekfActuationNoises[c], ekfActuationNoises[c], ekfActuationNoises[c], ekfActuationNoises[c], ekfActuationNoises[c], ekfActuationNoises[c],
                                                                                                                   e_n4, e_n4, e_n4,
                                                                                                                   e_n10, e_n10, e_n10,  e_n10, e_n10, e_n10, e_n10, e_n10, e_n10});

                                Matrix<double> MeasurementNoise = Matrix<double>.Build.DenseOfDiagonalArray(new double[] { ekfSensorNoises[d], ekfSensorNoises[d], ekfSensorNoises[d], ekfSensorNoises[d], ekfSensorNoises[d], ekfSensorNoises[d], ekfSensorNoises[d], ekfSensorNoises[d], ekfSensorNoises[d] });


                                Matrix<double> InitialCovariance = Matrix.Build.DenseDiagonal(numStateVars, numStateVars, 1.0);

                                EKF ekf = new EKF(numStateVars, numMeasurementVars);

                                for (int trialNum = 0; trialNum < 100; trialNum++)
                                {
                                    // place to save data

                                    List<double[]> measurements = new List<double[]>() { };
                                    List<double[]> jointTargets = new List<double[]>() { };
                                    List<double[]> trueStates = new List<double[]>() { };
                                    List<double[]> filteredStates = new List<double[]>() { };
                                    List<double[]> predictedStates = new List<double[]>() { };
                                    List<double[,]> covars = new List<double[,]>() { };
                                    List<double[]> encoderReadings = new List<double[]>();
                                    List<double> times = new List<double>();

                                    // Select noise/offsets

                                    // use these 2 lines for noisy simulation
                                    Matrix<double> actuationNoise = Matrix<double>.Build.DenseIdentity(numStateVars);
                                    Matrix<double> sensorNoise = Matrix<double>.Build.DenseIdentity(numMeasurementVars);

                                    // Use this line for a 3d translation 
                                    Vector<double> translationOffset = (0.01 * Vector<double>.Build.Dense(new double[] { rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 })) - Vector<double>.Build.Dense(new double[] { 0.005, 0.005, 0.005, 0.005, 0.005, 0.005, 0.005, 0.005, 0.005, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

                                    // Set up the robot

                                    double[] k_real_initial = new double[] { rnd.Next(5, 50), rnd.Next(5, 50), rnd.Next(5, 50) };
                                    double[] k_real_final = new double[] { rnd.Next(Math.Max(5, (int)k_real_initial[0] - 20), Math.Min(50, (int)k_real_initial[0] + 20)), rnd.Next(Math.Max(5, (int)k_real_initial[1] - 20), Math.Min(50, (int)k_real_initial[1] + 20)), rnd.Next(Math.Max(5, (int)k_real_initial[2] - 20), Math.Min(50, (int)k_real_initial[2] + 20)) };
                                    double[] k_guess = new double[] { rnd.Next(Math.Max(5, (int)k_real_initial[0] - 5), Math.Min(50, (int)k_real_initial[0] + 5)), rnd.Next(Math.Max(5, (int)k_real_initial[1] - 5), Math.Min(50, (int)k_real_initial[1] + 5)), rnd.Next(Math.Max(5, (int)k_real_initial[2] - 5), Math.Min(50, (int)k_real_initial[2] + 5)) };

                                    Tube tube1 = new Tube(2.792 * Math.Pow(10, -3), 3.30 * Math.Pow(10, -3), 1 / k_real_initial[0], 90 * Math.Pow(10, -3), 40 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6)); 
                                    Tube tube2 = new Tube(2.132 * Math.Pow(10, -3), 2.64 * Math.Pow(10, -3), 1 / k_real_initial[1], 170 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
                                    Tube tube3 = new Tube(1.473 * Math.Pow(10, -3), 1.98 * Math.Pow(10, -3), 1 / k_real_initial[2], 255 * Math.Pow(10, -3), 60 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
                                    Tube[] tubes = new Tube[] { tube1, tube2, tube3 };
                                    Program.Robot = new Robot(tubes);

                                    // get the true initial state

                                    Program.JointVars = q0;

                                    Matrix<double> tube1FK = Program.Robot.Fkin_Modified(q0, 0);
                                    Matrix<double> tube2FK = Program.Robot.Fkin_Modified(q0, 1);
                                    Matrix<double> tube3FK = Program.Robot.Fkin(q0);

                                    double[] initialTrueState = new double[] { tube1FK[0, 3], tube1FK[1, 3], tube1FK[2, 3], tube2FK[0, 3], tube2FK[1, 3], tube2FK[2, 3], tube3FK[0, 3], tube3FK[1, 3], tube3FK[2, 3], k_real_initial[0], k_real_initial[1], k_real_initial[2], translationOffset[0], translationOffset[1], translationOffset[2], translationOffset[3], translationOffset[4], translationOffset[5], translationOffset[6], translationOffset[7], translationOffset[8] };

                                    // Add noise and a 3d translation offset to the true state

                                    double[] randomCoeffsActuation = new double[] { actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationKDist.Sample(), actuationKDist.Sample(), actuationKDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample() };
                                    Vector<double> actuationNoiseToAdd = actuationNoise * Vector<double>.Build.Dense(randomCoeffsActuation);

                                    double[] initialNoisyState = (Vector<double>.Build.Dense(initialTrueState) + translationOffset + actuationNoiseToAdd).ToArray();

                                    trueStates.Add(initialNoisyState);

                                    // Get the noisy measurement

                                    double[] randomCoeffsSense = new double[] { measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample() };
                                    Vector<double> sensorNoiseToAdd = sensorNoise * Vector<double>.Build.Dense(randomCoeffsSense);
                                    
                                    double[] initialNoisyMeasurements = (Vector<double>.Build.Dense(new double[] { initialNoisyState[0], initialNoisyState[1], initialNoisyState[2], initialNoisyState[3], initialNoisyState[4], initialNoisyState[5], initialNoisyState[6], initialNoisyState[7], initialNoisyState[8] }) + sensorNoiseToAdd).ToArray();
                                    
                                    measurements.Add(initialNoisyMeasurements);

                                    // Get the initial guess state/seed the ekf

                                    Program.Robot.Tubes[0].k = k_guess[0];
                                    Program.Robot.Tubes[1].k = k_guess[1];
                                    Program.Robot.Tubes[2].k = k_guess[2];

                                    Matrix<double> tube1FK_guess = Program.Robot.Fkin_Modified(q0, 0);
                                    Matrix<double> tube2FK_guess = Program.Robot.Fkin_Modified(q0, 2);
                                    Matrix<double> tube3FK_guess = Program.Robot.Fkin(q0);

                                    double[] initialGuessState = new double[] { tube1FK_guess[0, 3], tube1FK_guess[1, 3], tube1FK_guess[2, 3], tube2FK_guess[0, 3], tube2FK_guess[1, 3], tube2FK_guess[2, 3], tube3FK_guess[0, 3], tube3FK_guess[1, 3], tube3FK_guess[2, 3], k_guess[0], k_guess[1], k_guess[2], 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                                    predictedStates.Add(initialGuessState);
                                    filteredStates.Add(initialGuessState);
                                    covars.Add(InitialCovariance.ToArray());
                                    encoderReadings.Add(q0);
                                    times.Add(0);

                                    Func<double[], double[]> stateModel = StateModelThreeTubesThreeBallsFullHoming;
                                    List<Func<double[], double>> individualModels = new List<Func<double[], double>>() {X1ModelThreeTubesThreeBallsFullHoming, Y1ModelThreeTubesThreeBallsFullHoming, Z1ModelThreeTubesThreeBallsFullHoming, X2ModelThreeTubesThreeBallsFullHoming, Y2ModelThreeTubesThreeBallsFullHoming, Z2ModelThreeTubesThreeBallsFullHoming, X3ModelThreeTubesThreeBallsFullHoming, Y3ModelThreeTubesThreeBallsFullHoming, Z3ModelThreeTubesThreeBallsFullHoming, K1ModelThreeTubesThreeBallsFullHoming, K2ModelThreeTubesThreeBallsFullHoming, K3ModelThreeTubesThreeBallsFullHoming, X1OffsetModelThreeTubesThreeBallsFullHoming, Y1OffsetModelThreeTubesThreeBallsFullHoming, Z1OffsetModelThreeTubesThreeBallsFullHoming, X2OffsetModelThreeTubesThreeBallsFullHoming, Y2OffsetModelThreeTubesThreeBallsFullHoming, Z2OffsetModelThreeTubesThreeBallsFullHoming, X3OffsetModelThreeTubesThreeBallsFullHoming, Y3OffsetModelThreeTubesThreeBallsFullHoming, Z3OffsetModelThreeTubesThreeBallsFullHoming};

                                    ekf.Initialize(ProcessNoise, MeasurementNoise, Vector<double>.Build.Dense(initialGuessState), InitialCovariance);

                                    // Get the joint space targets

                                    int numTestPoints = 25;

                                    int prevRho1 = 0;
                                    int prevRho2 = 0;
                                    int prevRho3 = 0;

                                    List<int> numTimeSteps = new List<int>();
                                    int totalTimeSteps = 0;

                                    double seconds = 0;
                                    int timeSteps = 0;

                                    for (int i = 0; i < numTestPoints; i++)
                                    {
                                        int rho1 = rnd.Next(0, 45);
                                        int rho2 = rnd.Next(rho1, rho1 + 45);
                                        int rho3 = rnd.Next(rho2, Math.Min(rho2 + 45, rho1+75));

                                        jointTargets.Add(new double[] { rho1, rho2, rho3, 0, 0, 0 });

                                        seconds = (double)Math.Max(Math.Abs(rho2 - prevRho2), Math.Max(Math.Abs(rho3 - prevRho3), Math.Abs(rho1 - prevRho1))) * (double)4.0 / (double)3.0;
                                        timeSteps = (int)Math.Round(seconds * 5);

                                        numTimeSteps.Add(timeSteps);
                                        totalTimeSteps += timeSteps;

                                        prevRho1 = rho1;
                                        prevRho2 = rho2;
                                        prevRho3 = rho3;
                                    }

                                    jointTargets.Add(new double[] { 0, 0, 0, 0, 0, 0 });

                                    seconds = (double)Math.Max(Math.Abs(prevRho2), Math.Abs(prevRho3)) * (double)4.0 / (double)3.0;
                                    timeSteps = (int)Math.Round(seconds * 5);

                                    numTimeSteps.Add(timeSteps);
                                    totalTimeSteps += timeSteps;

                                    // Simulate the robot's travel to each target

                                    prevRho1 = 0;
                                    prevRho2 = 0;
                                    prevRho3 = 0;

                                    for (int target = 0; target < jointTargets.Count; target++)
                                    {
                                        int rho1Start = prevRho1;
                                        int rho2Start = prevRho2;
                                        int rho3Start = prevRho3;
                                        int rho1End = (int)jointTargets[target][0];
                                        int rho2End = (int)jointTargets[target][1];
                                        int rho3End = (int)jointTargets[target][2];
                                        int steps = numTimeSteps[target];

                                        prevRho1 = rho1End;
                                        prevRho2 = rho2End;
                                        prevRho3 = rho3End;

                                        for (int step = 0; step < steps; step++)
                                        {
                                            times.Add(times[^1] + 1);

                                            // Get the joint variables
                                            double[] currEncoder = new double[] { rho1Start + (rho1End - rho1Start) * step / (double)steps, rho2Start + (rho2End - rho2Start) * step / (double)steps, rho3Start + (rho3End - rho3Start) * step / (double)steps, 0, 0, 0 };
                                            Program.JointVars = currEncoder;
                                            encoderReadings.Add(Program.JointVars);

                                            // Simulate the true state
                                            double[] newState = stateModel(trueStates[^1]);
                                            newState[9] = k_real_initial[0] + (k_real_final[0] - k_real_initial[0]) * (double)times[^1] / (double)totalTimeSteps;
                                            newState[10] = k_real_initial[1] + (k_real_final[1] - k_real_initial[1]) * (double)times[^1] / (double)totalTimeSteps;
                                            newState[11] = k_real_initial[2] + (k_real_final[2] - k_real_initial[2]) * (double)times[^1] / (double)totalTimeSteps;

                                            // Add noise to the true state
                                            
                                            randomCoeffsActuation = new double[] { actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationXYZDist.Sample(), actuationKDist.Sample(), actuationKDist.Sample(), actuationKDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample(), actuationOffsetDist.Sample() };
                                            actuationNoiseToAdd = actuationNoise * Vector<double>.Build.Dense(randomCoeffsActuation);
                                            
                                            double[] newNoisyState = (Vector<double>.Build.Dense(newState) + actuationNoiseToAdd).ToArray();
                                            
                                            trueStates.Add(newNoisyState);
                                            
                                            // Get the noisy measurement

                                            randomCoeffsSense = new double[] { measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample(), measurementXYZDist.Sample() };
                                            sensorNoiseToAdd = sensorNoise * Vector<double>.Build.Dense(randomCoeffsSense);

                                            double[] newNoisyMeasurements = (Vector<double>.Build.Dense(new double[] { newNoisyState[0], newNoisyState[1], newNoisyState[2], newNoisyState[3], newNoisyState[4], newNoisyState[5], newNoisyState[6], newNoisyState[7], newNoisyState[8] }) + sensorNoiseToAdd).ToArray();

                                            measurements.Add(newNoisyMeasurements);

                                            // Run the EKF: prediction step

                                            ekf.Predict(stateModel, individualModels);

                                            double[] predictedState = ekf.CurrState.ToArray();

                                            // Run the EKF: correction step

                                            ekf.Correct(Vector<double>.Build.Dense(newNoisyMeasurements));

                                            // Extract info from the EKF

                                            double[] filteredState = ekf.CurrState.ToArray();
                                            double[,] covar = ekf.Covariance.ToArray();

                                            predictedStates.Add(predictedState);
                                            filteredStates.Add(filteredState);
                                            covars.Add(covar);

                                        }
                                    }

                                    string directory = "./results/thesis_sim_3tube/";
                                    directory += configString + "/";
                                    string subdirectory = directory + trialNum.ToString() + "/";
                                    Directory.CreateDirectory(subdirectory);

                                    string filteredStatesPath = subdirectory + "filtered_states.csv";
                                    string trueStatesPath = subdirectory + "true_states.csv";
                                    string measurementPath = subdirectory + "measurements.csv";
                                    string jointVarsPath = subdirectory + "joint_vars.csv";
                                    string covarPath = subdirectory + "covariances.csv";
                                    string predictionsPath = subdirectory + "predicted_states.csv";
                                    string encoderReadingsPath = subdirectory + "encoder_readings.csv";

                                    WriteListOfArrayToCSV(filteredStates, filteredStatesPath);
                                    WriteListOfArrayToCSV(predictedStates, predictionsPath);
                                    WriteListOfArrayToCSV(trueStates, trueStatesPath);
                                    WriteListOfArrayToCSV(measurements, measurementPath);
                                    WriteListOfArrayToCSV(jointTargets, jointVarsPath);
                                    WriteListOfArrayToCSV(encoderReadings, encoderReadingsPath);
                                    WriteListOf2DArrayToCSV(covars, covarPath);

                                    Console.WriteLine(trialNum.ToString());

                                    if (a == 0 && b == 0 && c == 0 && d == 0 && trialNum == 0)
                                    {
                                        Console.WriteLine("everything ok?");
                                        Console.ReadLine();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Test the encoders
        /// </summary>
        public static void TestEncoders()
        {

            Encoders encs = new Encoders(4);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            encs.OpenComms();

            string value = encs.GetEncoderString("r");

            //value = encs.GetEncoderValue("r2");
            //Console.WriteLine(value.ToString());
            //value = encs.GetEncoderValue("r3");
            //Console.WriteLine(value.ToString());
            //value = encs.GetEncoderValue("t1");
            //Console.WriteLine(value.ToString());
            //value = encs.GetEncoderValue("t2");
            //Console.WriteLine(value.ToString());
            //value = encs.GetEncoderValue("t3");
            //Console.WriteLine(value.ToString());

            encs.CloseComms();

            long time = sw.ElapsedMilliseconds;

            string[] values = value.Split(',');
            string[] motors = new string[] { "t1", "t2", "t3", "r1", "r2", "r3" };

            Console.WriteLine("Time elapsed:");
            Console.WriteLine(time);

            Console.WriteLine("Values b4 conversion:");
            foreach (string s in values)
            {
                Console.WriteLine(s);
            }

            Console.WriteLine("Values after conversion:");
            for (int i = 0; i < 6; i++)
            {
                double converted = encs.ConvertEncoderValue(Double.Parse(values[i]), motors[i]);
                Console.WriteLine(converted);
            }


        }

        public static void WriteListOf2DArrayToCSV(List<double[,]> listOf2DArray, string filepath)
        {
            using (var file = File.CreateText(filepath))
            {
                var iLength = listOf2DArray[0].GetLength(0);
                var jLength = listOf2DArray[0].GetLength(1);

                foreach (var twoDArray in listOf2DArray)
                {
                    for (int i = 0; i < iLength; i++)
                    {
                        for (int j = 0; j < jLength; j++)
                        {
                            file.Write("{0},", twoDArray[i, j]);
                        }
                        file.WriteLine();
                    }
                }
            }
        }

        public static void WriteListOfArrayToCSV(List<double[]> listOfArray, string filepath)
        {
            using (var file = File.CreateText(filepath))
            {
                foreach (var arr in listOfArray)
                {
                    file.WriteLine(string.Join(",", arr));
                }
            }
        }

        public static void WriteListToCSV(List<double> list, string filepath)
        {
            using (var file = File.CreateText(filepath))
            {
                foreach (var t in list)
                {
                    file.WriteLine(t);
                }
            }
        }

        public static List<double[]> ReadListOfArrayFromCSV(string filepath)
        {
            List<double[]> retVal = new List<double[]>();

            using (TextFieldParser parser = new TextFieldParser(filepath))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");

                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    double[] doubles = Array.ConvertAll(fields, s => double.Parse(s, CultureInfo.InvariantCulture));
                    retVal.Add(doubles);
                }
            }

            return retVal;
        }

        public static double[] MeasurementModelThreeTubes(double[] predictedState)
        {
            double[] expectedMeasurement = new double[] { predictedState[0], predictedState[1], predictedState[2], predictedState[3], predictedState[4], predictedState[5], predictedState[6], predictedState[7], predictedState[8] };

            return expectedMeasurement;
        }

        

        public static double[] StateModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double k1 = currState[6];
            double k2 = currState[7];

            Program.Robot.Tubes[0].k = k1;
            Program.Robot.Tubes[1].k = k2;

            double x0_1 = currState[8];
            double y0_1 = currState[9];
            double z0_1 = currState[10];
            double x0_2 = currState[11];
            double y0_2 = currState[12];
            double z0_2 = currState[13];

            Matrix<double> Kine1 = Program.Robot.Fkin_Modified(Program.JointVars, 0);

            Matrix<double> Offset1 = DenseMatrix.OfArray(new double[,] {
                {1,0,0,x0_1 },
                {0,1,0,y0_1 },
                {0,0,1,z0_1 },
                {0,0,0,1 } });

            Matrix<double> HTM1 = Offset1 * Kine1;

            double x1 = HTM1[0, 3];
            double y1 = HTM1[1, 3];
            double z1 = HTM1[2, 3];

            Matrix<double> Kine2 = Program.Robot.Fkin(Program.JointVars);

            Matrix<double> Offset2 = DenseMatrix.OfArray(new double[,] {
                {1,0,0,x0_2 },
                {0,1,0,y0_2 },
                {0,0,1,z0_2 },
                {0,0,0,1 } });

            Matrix<double> HTM2 = Offset2 * Kine2;

            double x2 = HTM2[0, 3];
            double y2 = HTM2[1, 3];
            double z2 = HTM2[2, 3];

            double[] predictedState = new double[] { x1, y1, z1, x2, y2, z2, k1, k2, x0_1, y0_1, z0_1, x0_2, y0_2, z0_2};

            return predictedState;
        }

        public static double X1ModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[0];
        }

        public static double Y1ModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[1];
        }

        public static double Z1ModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[2];
        }

        public static double X2ModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[3];
        }

        public static double Y2ModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[4];
        }

        public static double Z2ModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[5];
        }

        public static double K1ModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[6];
        }

        public static double K2ModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[7];
        }

        public static double X1OffsetModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[8];
        }

        public static double Y1OffsetModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[9];
        }

        public static double Z1OffsetModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[10];
        }

        public static double X2OffsetModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[11];
        }

        public static double Y2OffsetModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[12];
        }

        public static double Z2OffsetModelTwoTubesTwoBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelTwoTubesTwoBallsFullHoming(currState);
            return predictedState[13];
        }

        public static double[] StateModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double k1 = currState[9];
            double k2 = currState[10];
            double k3 = currState[11];

            Program.Robot.Tubes[0].k = k1;
            Program.Robot.Tubes[1].k = k2;
            Program.Robot.Tubes[2].k = k3;

            double x0_1 = currState[12];
            double y0_1 = currState[13];
            double z0_1 = currState[14];
            double x0_2 = currState[15];
            double y0_2 = currState[16];
            double z0_2 = currState[17];
            double x0_3 = currState[18];
            double y0_3 = currState[19];
            double z0_3 = currState[20];


            Matrix<double> Kine1 = Program.Robot.Fkin_Modified(Program.JointVars, 0);

            Matrix<double> Offset1 = DenseMatrix.OfArray(new double[,] {
                {1,0,0,x0_1 },
                {0,1,0,y0_1 },
                {0,0,1,z0_1 },
                {0,0,0,1 } });

            Matrix<double> HTM1 = Offset1 * Kine1;

            double x1 = HTM1[0, 3];
            double y1 = HTM1[1, 3];
            double z1 = HTM1[2, 3];

            Matrix<double> Kine2 = Program.Robot.Fkin_Modified(Program.JointVars, 1);

            Matrix<double> Offset2 = DenseMatrix.OfArray(new double[,] {
                {1,0,0,x0_2 },
                {0,1,0,y0_2 },
                {0,0,1,z0_2 },
                {0,0,0,1 } });

            Matrix<double> HTM2 = Offset2 * Kine2;

            double x2 = HTM2[0, 3];
            double y2 = HTM2[1, 3];
            double z2 = HTM2[2, 3];

            Matrix<double> Kine3 = Program.Robot.Fkin(Program.JointVars);

            Matrix<double> Offset3 = DenseMatrix.OfArray(new double[,] {
                {1,0,0,x0_3 },
                {0,1,0,y0_3 },
                {0,0,1,z0_3 },
                {0,0,0,1 } });

            Matrix<double> HTM3 = Offset3 * Kine3;

            double x3 = HTM3[0, 3];
            double y3 = HTM3[1, 3];
            double z3 = HTM3[2, 3];

            double[] predictedState = new double[] { x1, y1, z1, x2, y2, z2, x3, y3, z3, k1, k2, k3, x0_1, y0_1, z0_1, x0_2, y0_2, z0_2, x0_3, y0_3, z0_3};

            return predictedState;
        }

        public static double X1ModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[0];
        }

        public static double Y1ModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[1];
        }

        public static double Z1ModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[2];
        }

        public static double X2ModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[3];
        }

        public static double Y2ModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[4];
        }

        public static double Z2ModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[5];
        }

        public static double X3ModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[6];
        }

        public static double Y3ModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[7];
        }

        public static double Z3ModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[8];
        }

        public static double K1ModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[9];
        }

        public static double K2ModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[10];
        }

        public static double K3ModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[11];
        }

        public static double X1OffsetModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[12];
        }

        public static double Y1OffsetModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[13];
        }

        public static double Z1OffsetModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[14];
        }

        public static double X2OffsetModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[15];
        }

        public static double Y2OffsetModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[16];
        }

        public static double Z2OffsetModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[17];
        }

        public static double X3OffsetModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[18];
        }

        public static double Y3OffsetModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[19];
        }

        public static double Z3OffsetModelThreeTubesThreeBallsFullHoming(double[] currState)
        {
            double[] predictedState = StateModelThreeTubesThreeBallsFullHoming(currState);
            return predictedState[20];
        }

        /// <summary>
        /// Test mocap kine
        /// </summary>
        public static void TestMocapKinematics()
        {
            Mocap mocap = new Mocap();

            //Console.WriteLine("Waiting 8 seconds");
            //Thread.Sleep(8000);

            Console.WriteLine("Initting mocap");

            try
            {
                mocap.Init();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Thread.Sleep(10000000);
            }

            Console.WriteLine("Getting strays");

            List<Marker> strays = mocap.FindStrays();

            Console.WriteLine("Found strays. Finding tools.");

            List<Tool> tools = mocap.FindTools();

            Console.WriteLine("Disconnecting mocap");

            mocap.Disconnect();

            Console.WriteLine("mocap disconnected");

            Console.WriteLine(tools.Count);

            //foreach(Tool t in tools)
            //{
            //    Console.WriteLine(t.ToString());
            //}

            Console.WriteLine("using this stray");
            Console.WriteLine(strays[0].ToString());

            Console.WriteLine("stray.position is:");
            Console.WriteLine(strays[0].position.ToString());

            Console.WriteLine("all strays are:");

            foreach (Marker s in strays)
            {
                Console.WriteLine("stray is:");
                Console.WriteLine(s.ToString());

                Console.WriteLine("stray.position is:");
                Console.WriteLine(s.position.ToString());
            }


            Matrix<double> tipMatrix = mocap.GetStrayHTM(strays[0]);

            Matrix<double> homeMatrix = mocap.GetToolHTM(tools[0]);

            Matrix<double> mocapFK = homeMatrix.Inverse() * tipMatrix;

            Console.WriteLine(tipMatrix.ToString());

            Console.WriteLine(homeMatrix.ToString());

            Console.WriteLine(homeMatrix.Inverse().ToString());

            Console.WriteLine("mocap:");

            Console.WriteLine(mocapFK.ToString());

            double[] currentQ = new double[] { 0, 0, 0, 0, 0, 0 };

            Tube tube1 = new Tube(2.792 * Math.Pow(10, -3), 3.30 * Math.Pow(10, -3), 1 / 6.205, 90 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube2 = new Tube(2.132 * Math.Pow(10, -3), 2.64 * Math.Pow(10, -3), 1 / 7.845, 170 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube3 = new Tube(1.473 * Math.Pow(10, -3), 1.98 * Math.Pow(10, -3), 1 / 10.55, 250 * Math.Pow(10, -3), 60 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube[] tubes = new Tube[] { tube1, tube2, tube3 };
            Robot robot = new Robot(tubes);

            Matrix<double> theoreticalFK = robot.Fkin(currentQ);

            Console.WriteLine("model:");

            Console.WriteLine(theoreticalFK.ToString());

            Console.WriteLine("difference:");

            Matrix<double> difference = theoreticalFK - mocapFK;

            Console.WriteLine(difference.ToString());
        }

        /// <summary>
        /// Test robot motion
        /// </summary>
        public static void MoveRobot()
        {
            Tube tube1 = new Tube(2.792 * Math.Pow(10, -3), 3.30 * Math.Pow(10, -3), 1 / 19.8, 90 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube2 = new Tube(2.132 * Math.Pow(10, -3), 2.64 * Math.Pow(10, -3), 1 / 14.7, 170 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube tube3 = new Tube(1.473 * Math.Pow(10, -3), 1.98 * Math.Pow(10, -3), 1 / 13.7, 250 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
            Tube[] tubes = new Tube[] { tube1, tube2, tube3 };
            Robot robot = new Robot(tubes);

            robot.OpenComms(3);

            robot.TravelTo(new double[] { 0, 0, 0, 0, 0, 0 });

            Thread.Sleep(5000);

            robot.CloseComms();
        }

        /// <summary>
        /// 
        /// </summary>
        public static void KinematicVerification()
        {
            try
            {
                Tube tube1 = new Tube(2.792 * Math.Pow(10, -3), 3.30 * Math.Pow(10, -3), 1 / 19.8, 90 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
                Tube tube2 = new Tube(2.132 * Math.Pow(10, -3), 2.64 * Math.Pow(10, -3), 1 / 14.7, 170 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
                Tube tube3 = new Tube(1.473 * Math.Pow(10, -3), 1.98 * Math.Pow(10, -3), 1 / 13.7, 250 * Math.Pow(10, -3), 50 * Math.Pow(10, -3), 1935 * Math.Pow(10, 6));
                Tube[] tubes = new Tube[] { tube1, tube2, tube3 };
                Robot robot = new Robot(tubes);


                for (int i = 0; i < 100; i++)
                {

                    Console.WriteLine("Trial " + i.ToString() + " -------");

                    // Read the joint variable configuration from the csv file

                    string line = "";
                    using (StreamReader reader = new StreamReader("../../../../../../kine_verify/q" + (i + 1).ToString() + ".csv"))
                    {
                        line = reader.ReadLine();
                    }

                    string[] stringValues = line.Split(',');

                    double[] q_var = new double[stringValues.Length];

                    for (int j = 0; j < stringValues.Length; j++)
                    {
                        q_var[j] = Double.Parse(stringValues[j]);
                    }


                    // Perform the forward kinematics

                    Matrix<double> T_Csharp = robot.Fkin(q_var);

                    // Get the HTM that MATLAB calculated for this joint configuration

                    Matrix<double> T_matlab = DelimitedReader.Read<double>("../../../../../../kine_verify/T" + (i + 1).ToString() + ".csv", false, ",", false);

                    // Compare the 2 FK outputs

                    Matrix<double> diff = T_Csharp - T_matlab;

                    Console.WriteLine(diff.ToString());

                }

            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }
}
