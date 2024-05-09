using System;
using System.Collections.Generic;
using System.Text;

// Matrix Algebra Library
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Differentiation;

namespace CAPISampleApplication
{
    internal class EKF
    {
        // Number of variables in the state vector
        public int NumStateVars { get; set; }

        // Number of variables in the measurement vector
        public int NumMeasurementVars { get; set; }

        // (NumStateVars x NumStateVars) matrix that describes uncertainty in the state model
        public Matrix<double> ProcessNoise { get; set; }

        // (NumMeasurementVars x NumMeasurementVars) matrix that describes uncertainty in the measurement model
        public Matrix<double> MeasurementNoise { get; set; }

        // (NumStateVars x 1) vector of current state variables
        public Vector<double> CurrState { get; set; }

        // (NumMeasurementVars x 1) vector of current measurement variables
        public Vector<double> CurrMeasurement { get; set; }

        // (NumStateVars x NumStateVars) current covariance
        public Matrix<double> Covariance { get; set; }

        /// <summary>
        /// Constructor to create an EKF object
        /// </summary>
        /// <param name="NumStateVars">Number of state variables to track</param>
        /// <param name="NumMeasurementVars">Number of measurement variables to track</param>
        public EKF(int NumStateVars, int NumMeasurementVars)
        {
            this.NumStateVars = NumStateVars;
            this.NumMeasurementVars = NumMeasurementVars;
        }

        /// <summary>
        /// Initialize the EKF
        /// </summary>
        /// <param name="ProcessNoise">The uncertainty in the state model. (NumStateVars x NumStateVars)</param>
        /// <param name="MeasurementNoise">The uncertainty in the sensor model. (NumMeasurementVars x NumMeasurementVars)</param>
        /// <param name="InitialState">Initial estimate of the state. (NumStateVars x 1)</param>
        /// <param name="InitialCovariance">Initial uncertainty. (NumStateVars x NumStateVars)</param>
        /// <exception cref="Exception">If the dimensions are wrong.</exception>
        public void Initialize(Matrix<double> ProcessNoise, Matrix<double> MeasurementNoise, Vector<double> InitialState, Matrix<double> InitialCovariance)
        {
            // Check matrix/vector dimensions
            if (ProcessNoise.RowCount != NumStateVars ||
                ProcessNoise.ColumnCount != NumStateVars ||
                MeasurementNoise.RowCount != NumMeasurementVars ||
                MeasurementNoise.ColumnCount != NumMeasurementVars ||
                InitialState.Count != NumStateVars ||
                InitialCovariance.RowCount != NumStateVars ||
                InitialCovariance.ColumnCount != NumStateVars)
            {
                throw new Exception("Input matrices are wrong size.");
            }

            // Assign inputs to class fields
            this.CurrState = InitialState;
            this.ProcessNoise = ProcessNoise;
            this.MeasurementNoise = MeasurementNoise;
            this.Covariance = InitialCovariance;
        }

        /// <summary>
        /// Run EKF prediction step
        /// </summary>
        /// <param name="stateModel">Function contatining the state model. 
        /// Inputs vector of length NumStateVars (the prev state) and outputs vector of length NumStateVars (the new state).</param>
        /// <param name="individualModels">List of functions, each of which calculate a single state variable based on the prev state. 
        /// List should be of length NumStateVars. Each function in list must input vector of length NumStateVars and output a double.</param>
        public void Predict(Func<double[], double[]> stateModel, List<Func<double[], double>> individualModels)
        {
            // Predict the new state based on the actuation model
            
            double[] predicted_state = stateModel(CurrState.ToArray());

            // Calculate the (estimated) jacobian of the actuation model
            
            NumericalJacobian jacob = new NumericalJacobian();

            List<double[]> jacob_rows = new List<double[]>(); 

            for(int i = 0; i < NumStateVars; i++)
            {
                double[] jacob_row = jacob.Evaluate(individualModels[i], CurrState.ToArray());

                jacob_rows.Add(jacob_row);

            }

            Matrix<double> jacob_evaluated= Matrix<double>.Build.DenseOfRowArrays(jacob_rows);

            // Update the current state to match the predicted state

            this.CurrState = Vector<double>.Build.Dense(predicted_state);

            // Update covariance to match predicted covariance 

            this.Covariance = jacob_evaluated * this.Covariance * jacob_evaluated.Transpose() + ProcessNoise;

            return;
        }

        /// <summary>
        /// Run EKF correction step, assuming linear measurement model
        /// </summary>
        /// <param name="Observation"></param>
        public void Correct(Vector<double> Observation)
        {
            // Calculate the Jacobian of the measurement function

            // B/c the sensor gives us state variables directly, this is trivial to calculate
            Matrix<double> measurement_jacobian = Matrix<double>.Build.Dense(this.NumMeasurementVars, this.NumStateVars);
            measurement_jacobian[0, 0] = 1;
            measurement_jacobian[1, 1] = 1;
            measurement_jacobian[2, 2] = 1;
            measurement_jacobian[3, 3] = 1;
            measurement_jacobian[4, 4] = 1;
            measurement_jacobian[5, 5] = 1;
            measurement_jacobian[6, 6] = 1;
            measurement_jacobian[7, 7] = 1;
            measurement_jacobian[8, 8] = 1;

            // Calculate the Kalman gain

            Matrix<double> KalmanGainInner = measurement_jacobian * this.Covariance * measurement_jacobian.Transpose() + this.MeasurementNoise;

            Matrix<double> KalmanGain = this.Covariance * measurement_jacobian.Transpose() * KalmanGainInner.Inverse();

            // Calculate the corrected state and covariance

            this.CurrState = this.CurrState + KalmanGain * (Observation - measurement_jacobian * this.CurrState);

            this.Covariance = (Matrix<double>.Build.DenseIdentity(NumStateVars) - KalmanGain * measurement_jacobian) * this.Covariance;

            return;
        }
    }
}
