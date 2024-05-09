using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.Text;

// Matrix Algebra Library
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace CAPISampleApplication
{
    internal class Robot
    {
        // Number of tubes in the CTR []
        public int NumTubes { get; set; }

        public double[] LLs {  get; set; }

        public double[] Phi { get; set; }

        public double[] Kappa { get; set; }

        // List of tubes
        public Tube[] Tubes { get; set; }

        // COM Port to which the robot control board is connected
        public SerialPort port { get; set; }

        // Determines which tubes are present in which links
        public int[,] Numerators { get; set; }
        public int[,] Denominators { get; set; }

        // Constructor
        public Robot(Tube[] tubes)
        {
            this.Tubes = tubes;
            this.NumTubes = tubes.Length;
        }

        /// <summary>
        /// Initializes communications with the robot 
        /// </summary>
        /// <param name="COMPort">The integer number of the COM Port that the robot is plugged into</param>
        /// <returns>true if the communication is successfully established, false otherwise</returns>
        public bool OpenComms(int COMPort)
        {
            try
            {
                this.port = new SerialPort("COM" + COMPort.ToString(), 250000);
                this.port.Open();
            }
            catch(Exception e)
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
        /// Command the robot to move to a specific location in joint space
        /// </summary>
        /// <param name="q_var">The joint space target location for the robot,
        /// in the form [rho1 rho2 rho3 theta1 theta2 theta3], where rho values
        /// are in mm and theta values are in deg</param>
        public void TravelTo(double[] q_var)
        {
            string gCode = "G0 ";

            gCode += "X" + (q_var[0] * 16.0).ToString() + " ";
            gCode += "Y" + (q_var[1] * 16.0).ToString() + " ";
            gCode += "Z" + (q_var[2] * 16.0).ToString() + " ";

            gCode += "A" + (q_var[3]).ToString() + " ";
            gCode += "B" + (q_var[4]).ToString() + " ";
            gCode += "C" + (q_var[5]).ToString() + " ";

            this.port.WriteLine(gCode);

            return;

        }

        /// <summary>
        /// Calculate the forward kinematics of a CTR
        /// </summary>
        /// <param name="q_var">The CTR's joint variables, in the 
        /// form [rho1 ... rhon theta1 ... thetan], where rho values
        /// are in mm and theta values are in deg.</param>
        /// <returns>A 4x4 HTM representing the transform from the home frame 
        /// to the robot's tip</returns>
        public Matrix<double> Fkin(double[] q_var)
        {
            // Extract the translation (rho) and rotation (theta) values
            double[] rho = GetRhoValues(q_var);
            double[] theta = GetTheta(q_var);

            // Get the link lengths
            List<double[]> get_links_output = GetLinks(rho);
            this.LLs = get_links_output[0];

            // Get the rotation and curvature of each link
            List<double[]> arc_params = CalculatePhiAndKappa(theta, rho);
            this.Phi = arc_params[0];
            this.Kappa = arc_params[1];

            // Use the arc params to complete the robot-independent mapping
            Matrix<double> T = CalculateTransform(this.LLs, this.Phi, this.Kappa);

            // Account for the fact that we offset the translations so that rho1 is 0.
            // We need to pre-multiply by the smallest translation to get to the first 
            // ttransition point.
            double minTranslation = q_var[0];

            for(int i = 0; i < this.NumTubes; i++)
            {
                if (q_var[i] < minTranslation)
                {
                    minTranslation = q_var[i];
                }
            }

            Matrix<double> T_rho1 = Matrix<double>.Build.DenseIdentity(4);
            T_rho1[2,3] = minTranslation * Math.Pow(10,-3);

            T = T_rho1 * T;

            // Return the final transformation matrix
            return T;
        }

        /// <summary>
        /// Calculates the forward kinematics to the tip of a given tube of a CTR
        /// </summary>
        /// <param name="q_var">Current joint vaiables</param>
        /// <param name="tube">The index of the tube for which the FK is desired</param>
        /// <returns>HTM from robot base to tip of given tube</returns>
        public Matrix<double> Fkin_Modified(double[] q_var, int tube)
        {
            // Extract the translation (rho) and rotation (theta) values
            double[] rho = GetRhoValues(q_var);
            double[] theta = GetTheta(q_var);

            // Get the link lengths
            List<double[]> get_links_output = GetLinks(rho);
            this.LLs = get_links_output[0];
            double[] indices = get_links_output[1];

            // Get the rotation and curvature of each link
            List<double[]> arc_params = CalculatePhiAndKappa(theta, rho);
            this.Phi = arc_params[0];
            this.Kappa = arc_params[1];

            // Use the arc params to complete the robot-independent mapping
            Matrix<double> T = CalculateTransform_Modified(this.LLs, this.Phi, this.Kappa, indices, tube);

            // Account for the fact that we offset the translations so that rho1 is 0.
            // We need to pre-multiply by the smallest translation to get to the first 
            // ttransition point.
            double minTranslation = q_var[0];

            for (int i = 0; i < this.NumTubes; i++)
            {
                if (q_var[i] < minTranslation)
                {
                    minTranslation = q_var[i];
                }
            }

            Matrix<double> T_rho1 = Matrix<double>.Build.DenseIdentity(4);
            T_rho1[2, 3] = minTranslation * Math.Pow(10, -3);

            T = T_rho1 * T;

            // Return the final transformation matrix
            return T;
        }

        /// <summary>
        /// Convert a 3x3 rotation matrix to a quaternion
        /// </summary>
        /// <param name="rotMatrix">The 3x3 rotation matrix</param>
        /// <returns>A quaternion equivalent to the input matrix</returns>
        public Vector<double> RotToQuat(Matrix<double> rotMatrix)
        {            
            double m00 = rotMatrix[0,0];
            double m01 = rotMatrix[0,1];
            double m02 = rotMatrix[0,2];
            double m10 = rotMatrix[1, 0];
            double m11 = rotMatrix[1, 1];
            double m12 = rotMatrix[1, 2];
            double m20 = rotMatrix[2, 0];
            double m21 = rotMatrix[2, 1];
            double m22 = rotMatrix[2, 2];

            double qw = -1.0;
            double qx = -1.0;
            double qy = -1.0;
            double qz = -1.0;

            double tr = m00 + m11 + m22;

            if (tr > 0)
            {
                double S = Math.Sqrt(tr + 1.0) * 2.0; // S=4*qw 
                qw = 0.25 * S;
                qx = (m21 - m12) / S;
                qy = (m02 - m20) / S;
                qz = (m10 - m01) / S;
            }
            else if ((m00 > m11) & (m00 > m22))
            {
                double S = Math.Sqrt(1.0 + m00 - m11 - m22) * 2.0; // S=4*qx 
                qw = (m21 - m12) / S;
                qx = 0.25 * S;
                qy = (m01 + m10) / S;
                qz = (m02 + m20) / S;
            }
            else if (m11 > m22)
            {
                double S = Math.Sqrt(1.0 + m11 - m00 - m22) * 2.0; // S=4*qy
                qw = (m02 - m20) / S;
                qx = (m01 + m10) / S;
                qy = 0.25 * S;
                qz = (m12 + m21) / S;
            }
            else
            {
                double S = Math.Sqrt(1.0 + m22 - m00 - m11) * 2.0; // S=4*qz
                qw = (m10 - m01) / S;
                qx = (m02 + m20) / S;
                qy = (m12 + m21) / S;
                qz = 0.25 * S;
            }

            Vector<double> quat = Vector<double>.Build.Dense(new double[] {qw, qx, qy, qz});

            return quat;
        }

        /// <summary>
        /// Extract rho values from a vector of CTR joint variables
        /// Take care to convert mm to m, and to subtract the value 
        /// of the first joint variable from each value so that rho1
        /// is always 0.
        /// </summary>
        /// <param name="q_var">The array of joint variables, in the form
        /// [rho1 ... rhon theta1 ... thetan] </param>
        /// <returns>An array of tube translations, offset so that the first
        /// tube's translation is 0</returns>
        public double[] GetRhoValues(double[] q_var)
        {
            // Initialize an array to hold the rho values
            double[] rho = new double[this.NumTubes];

            // Fill in the array with the joint variables, converting
            // mm to m and subtracting by the first joint variable
            for(int i = 0; i < this.NumTubes; i++)
            {
                rho[i] = (q_var[i] - q_var[0]) * Math.Pow(10, -3);
            }

            return rho;
        }

        /// <summary>
        /// Gets the length of each link of a CTR, in order, given
        /// a vector of input translations
        /// </summary>
        /// <param name="rho">A vector of inpuit translations, 
        /// with the assumption that rho1 is 0 (see get_rho_values)</param>
        /// <returns>A double[] with the link lengths and a double[] 
        /// with the order of the transition points (in the order 
        /// [tube1Curve ... tubeNCurve tube1End ... tubeNEnd]).</returns>
        public List<double[]> GetLinks(double[] rho)
        {
            // An array to hold the locations of the 2n transition points
            double[] transition_pts = new double[2 * this.NumTubes];
            
            // An array to hold the 2n-1 link lengths
            double[] link_lengths = new double[2 * this.NumTubes - 1];

            // An array to track the order of the transition points
            double[] indices = new double[2 * this.NumTubes];


            for(int i = 0; i < this.NumTubes; i++) 
            {
                // The first transition point occurs when the curved section of the tube
                // starts. Because we assume the zero position to be when all curved
                // sections are aligned, this transition point occurs at whatever 
                // translation value is assigned to the tube.
                transition_pts[i] = rho[i];

                // The second transition point occurs at the end of the tube. This 
                // transition point is always a distance equal to the tube's curved
                // section's arc length away from the first transition point.
                transition_pts[i + this.NumTubes] = rho[i] + this.Tubes[i].d;

                // indices should be an array starting at 0 and ending at 2n-1
                indices[i] = i;
                indices[i + this.NumTubes] = i + this.NumTubes;
            }


            // Sort the transition points. Indices will be rearranged such that 
            // indices[j] contains the original (pre-sort) index of transition_pts[j]
            Array.Sort(transition_pts, indices);

            // Use the sorted array of transition points to calculate the link lengths
            for(int i = 0; i < 2*this.NumTubes - 1; i++)
            {
                link_lengths[i] = transition_pts[i+1] - transition_pts[i];
            }

            // Assemble the return values
            List<double[]> retVal = new List<double[]>() { link_lengths, indices };

            return retVal;
        }
            
        /// <summary>
        /// Extract theta values from a vector of joint variables 
        /// and convert them from degrees to radians
        /// </summary>
        /// <param name="q_var">A vector of joint variables, in the 
        /// form [rho1 ... rhon theta1 ... thetan] </param>
        /// <returns>A vector of theta values (in radians) that 
        /// represent the rotations applied to the CTR's tubes.</returns>
        public double[] GetTheta(double[] q_var) 
        {
            double[] theta = new double[NumTubes];

            for(int i=0; i<NumTubes; i++)
            {
                theta[i] = q_var[i+this.NumTubes] * Math.PI / 180.0;
            }

            return theta;
        }

        /// <summary>
        /// Calculate the values of phi and kappa for every link in the CTR
        /// </summary>
        /// <param name="theta">The base rotations applied to each tube</param>
        /// <param name="rho">The translations applied to each tube</param>
        /// <returns>A double[] of phi (link rotation) values and a double[]
        /// of kappa (link curvature) values, assembled in a list</returns>
        public List<double[]> CalculatePhiAndKappa(double[] theta, double[] rho)
        {
            // Get the in-plane parameters (chi and gamma)
            List<double[]> in_plane_params = InPlaneParam(theta, rho);
            double[] chi = in_plane_params[0];
            double[] gamma = in_plane_params[1];

            // Initialize arrays to hold the values of phi and kappa for each link
            double[] phi = new double[2 * NumTubes - 1];
            double[] K = new double[2 * NumTubes - 1];

            // For each link...
            for(int i = 0; i< 2*NumTubes-1; i++)
            {
                // Calculate phi and kappa using chi and gamma
                double phi_i = Math.Atan2(gamma[i], chi[i]);
                double K_i = Math.Sqrt(chi[i] * chi[i] + gamma[i] * gamma[i]);

                // The phi for each link is calculated wrt to the phi of the 
                // previous link.
                if(i == 0)
                {
                    phi[i] = phi_i;
                }
                else
                {
                    phi[i] = phi_i - phi[i - 1];
                }

                K[i] = K_i;
            }

            // Assemble and return the list
            return new List<double[]>() { phi, K};
        }

        /// <summary>
        /// Calculates the 4x4 homogeneous transformation matrix associated
        /// with the base->tip transformation of a concentric tube robot
        /// based on the robot-independent arc parameters
        /// </summary>
        /// <param name="s">A double[] of the CTR's links' arc lengths, in order 
        /// from base to tip</param>
        /// <param name="phi">A double[] of the CTR's links' base rotations, in 
        /// order from base to tip</param>
        /// <param name="K">A double[] of the CTR's links' curvatures, in order
        /// from base to tip</param>
        /// <returns>A 4x4 HTM that represents the transformation from the 
        /// robot's base to its tip</returns>
        public Matrix<double> CalculateTransform(double[] s, double[] phi, double[] K) 
        {
            // Initialize an identity matrix to hold the accumulating transform
            Matrix<double> T = Matrix<double>.Build.DenseIdentity(4);

            // Calculate a 4x4 HTM based on the arc parameters associated with each link
            for(int i=0; i<2*NumTubes-1;i++)
            {
                Matrix<double> tt;

                // We need to handle the case where curvature is 0 separately because 
                // the standard HTM for continuum robots divides by K
                if (K[i] == 0)
                {
                    tt = DenseMatrix.OfArray(new double[,] {
                        {Math.Cos(phi[i]),-Math.Sin(phi[i]),0,0},
                        {Math.Sin(phi[i]),Math.Cos(phi[i]),0,0},
                        {0,0,1,s[i]},
                        {0,0,0,1 } });
                }
                else
                {
                    tt = DenseMatrix.OfArray(new double[,] {
                        {Math.Cos(phi[i])*Math.Cos(K[i]*s[i]), -Math.Sin(phi[i]), Math.Cos(phi[i])*Math.Sin(K[i]*s[i]),Math.Cos(phi[i])*(1-Math.Cos(K[i]*s[i]))/K[i]},
                        {Math.Sin(phi[i])*Math.Cos(K[i]*s[i]), Math.Cos(phi[i]), Math.Sin(phi[i])*Math.Sin(K[i]*s[i]),Math.Sin(phi[i])*(1-Math.Cos(K[i]*s[i]))/K[i]},
                        {-Math.Sin(K[i]*s[i]),0,Math.Cos(K[i]*s[i]),Math.Sin(K[i]*s[i])/K[i]},
                        {0,0,0,1 } });
                }

                T = T * tt;
            }

            return T;
        }

        /// <summary>
        /// Calculates the 4x4 homogeneous transformation matrix associated
        /// with the base->tube tip transformation of a concentric tube robot
        /// for a given tube based on the robot-independent arc parameters
        /// </summary>
        /// <param name="s">A double[] of the CTR's links' arc lengths, in order 
        /// from base to tip</param>
        /// <param name="phi">A double[] of the CTR's links' base rotations, in 
        /// order from base to tip</param>
        /// <param name="K">A double[] of the CTR's links' curvatures, in order
        /// from base to tip</param>
        /// <param name="indices">Order of the links</param>
        /// <param name="tubeIndex">Desired tube</param>
        /// <returns>A 4x4 HTM that represents the transformation from the 
        /// robot's base to its tip</returns>
        public Matrix<double> CalculateTransform_Modified(double[] s, double[] phi, double[] K, double[] indices, int tubeIndex)
        {
            // Initialize an identity matrix to hold the accumulating transform
            Matrix<double> T = Matrix<double>.Build.DenseIdentity(4);

            double tapeOffset = 0;
            if (tubeIndex == 0) tapeOffset = 0.000;
            if (tubeIndex == 1) tapeOffset = 0.000;

            int indexToStop = Array.IndexOf(indices, this.NumTubes+tubeIndex) - 1;
            double arcLengthToSubtract = tapeOffset;
            double arcLengthAccum = s[indexToStop];

            while (arcLengthAccum < tapeOffset)
            {
                arcLengthToSubtract -= s[indexToStop];
                indexToStop = indexToStop - 1;
                arcLengthAccum += s[indexToStop];
            }

            // Calculate a 4x4 HTM based on the arc parameters associated with each link
            for (int i = 0; i <= indexToStop; i++)
            {
                Matrix<double> tt;

                double s_i = s[i];

                if(i == indexToStop)
                {
                    s_i -= arcLengthToSubtract;
                }

                // We need to handle the case where curvature is 0 separately because 
                // the standard HTM for continuum robots divides by K
                if (K[i] == 0)
                {
                    tt = DenseMatrix.OfArray(new double[,] {
                        {Math.Cos(phi[i]),-Math.Sin(phi[i]),0,0},
                        {Math.Sin(phi[i]),Math.Cos(phi[i]),0,0},
                        {0,0,1,s_i},
                        {0,0,0,1 } });
                }
                else
                {
                    tt = DenseMatrix.OfArray(new double[,] {
                        {Math.Cos(phi[i])*Math.Cos(K[i]*s_i), -Math.Sin(phi[i]), Math.Cos(phi[i])*Math.Sin(K[i]*s_i),Math.Cos(phi[i])*(1-Math.Cos(K[i]*s_i))/K[i]},
                        {Math.Sin(phi[i])*Math.Cos(K[i]*s_i), Math.Cos(phi[i]), Math.Sin(phi[i])*Math.Sin(K[i]*s_i),Math.Sin(phi[i])*(1-Math.Cos(K[i]*s_i))/K[i]},
                        {-Math.Sin(K[i]*s_i),0,Math.Cos(K[i]*s_i),Math.Sin(K[i]*s_i)/K[i]},
                        {0,0,0,1 } });
                }

                T = T * tt;
            }

            return T;
        }

        /// <summary>
        /// Calculate the in-plane parameters associated with the links of
        /// a CTR. 
        /// </summary>
        /// <param name="theta">The vector of tube rotations applied to the CTR</param>
        /// <param name="rho">The vector of tube translations applied to the CTR</param>
        /// <returns>A List of double[] where the first double[] contains the 
        /// chi values for each of the CTR's links and the second contains
        /// the gamma values for each of the CTR's links.</returns>
        public List<double[]> InPlaneParam(double[] theta, double[] rho)
        {
            // Get the link lengths and link order from the GetLinks function
            List<double[]> LinksOutput = GetLinks(rho);
            double[] link_lengths = LinksOutput[0];
            double[] indices = LinksOutput[1];

            // Initialize 2d arrays to hold Boolean values to track which tubes
            // contribute to which links
            int[,] numerators = new int[this.NumTubes, 2* this.NumTubes];
            int[,] denominators = new int[this.NumTubes, 2 * this.NumTubes];

            this.Numerators = numerators;
            this.Denominators = denominators;

            // For each tube...
            for (int i = 0; i < this.NumTubes; i++)
            {
                int numeratorStart = Array.IndexOf(indices, i);
                int numeratorEnd = Array.IndexOf(indices, i+this.NumTubes) - 1;
                int denominatorEnd = numeratorEnd;

                // If the tube's straight section is part of this link,
                // then the tube's material properties contribute to the 
                // denominators of chi and gamma, but not the numerators.
                // If the tube's curved section is part of this link,
                // then the tube's material properties contribute to the 
                // numerators and denominators of chi and gamma.
                for (int j = 0; j < 2*this.NumTubes; j++)
                {
                    if(j >= numeratorStart && j <= numeratorEnd)
                    {
                        numerators[i, j] = 1;
                    }
                    else
                    {
                        numerators[i, j] = 0;
                    }

                    if(j <= denominatorEnd)
                    {
                        denominators[i, j] = 1;
                    }
                    else
                    {
                        denominators[i, j] = 0;
                    }
                }
            }

            // Initialize arrays to hold the chi and gamma values
            double[] chi = new double[2 * this.NumTubes - 1];
            double[] gamma = new double[2 * this.NumTubes - 1];

            // For each link...
            for (int i = 0; i < 2*this.NumTubes - 1; i++)
            {
                // Initialize accumulator values
                double chi_i_numerator = 0.0;
                double chi_i_denominator = 0.0;
                double gamma_i_numerator = 0.0;
                double gamma_i_denominator = 0.0;

                // For each tube, add the tube's material properties to the accumulator
                // (provided the tube is present in the link)
                // Recall that the numerators and denominators arrays contain the logic to
                // determine which tubes contribute to which links
                for (int j =0; j < this.NumTubes; j++)
                {
                    chi_i_numerator += this.Tubes[j].E * this.Tubes[j].I * this.Tubes[j].k * Math.Cos(theta[j]) * numerators[j, i];
                    chi_i_denominator += this.Tubes[j].E * this.Tubes[j].I * denominators[j, i];

                    gamma_i_numerator += this.Tubes[j].E * this.Tubes[j].I * this.Tubes[j].k * Math.Sin(theta[j]) * numerators[j, i];
                    gamma_i_denominator += this.Tubes[j].E * this.Tubes[j].I * denominators[j, i];
                }

                // Calculate and store the chi and gamma values
                double chi_i = chi_i_numerator / chi_i_denominator;
                double gamma_i = gamma_i_numerator / gamma_i_denominator;

                chi[i] = chi_i;
                gamma[i] = gamma_i;
            }

            // Assemble the arrays in a list to return
            List<double[]> retVal = new List<double[]>() { chi, gamma };

            return retVal;

        }
    }
}
