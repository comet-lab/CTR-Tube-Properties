using System;
using System.Collections.Generic;
using System.Text;

// Matrix Algebra Library
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Differentiation;

namespace CAPISampleApplication
{
    internal class UKF
    {
        // Number of variables in the state vector
        public int NumStateVars { get; set; }

        // Number of variables in the measurement
        public int NumMeasurementVars { get; set; }

        // Number of sigma points to use in the UKF
        public int NumSigmaPoints { get; set; }

        // Lambda value to use in the UKF
        public double Lambda { get; set; }

        // Weights to be assigned to the sigma points' means
        public double[] MeanWeights { get; set; }

        // Weights to be assigned to the sigma points' covariances
        public double[] CovarianceWeights { get; set; }

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
        /// Constructor to create a UKF object
        /// </summary>
        /// <param name="NumStateVars">Number of state variables to track</param>
        /// <param name="NumMeasurementVars">Number of measurement variables to tract</param>
        /// <param name="alpha">Tuning param</param>
        /// <param name="beta">Tuning param</param>
        /// <param name="kappa">Tuning param</param>
        /// <exception cref="Exception">If tuning parameters are invalid</exception>
        public UKF(int NumStateVars, int NumMeasurementVars, double alpha, double beta, double kappa)
        {
            // Assign inputs to class fields
            this.NumStateVars = NumStateVars;
            this.NumMeasurementVars = NumMeasurementVars;
            this.NumSigmaPoints = 2 * NumStateVars + 1;
            
            this.MeanWeights = new double[NumSigmaPoints];
            this.CovarianceWeights = new double[NumSigmaPoints];

            // Calculate lambda based on tuning params
            this.Lambda = Math.Pow(alpha, 2) * (NumStateVars + kappa) - NumStateVars;

            // The sum of mean weights should be 1. Same for covariance weights. We use this as a check to make sure we are doing it right.
            double meanWeightSum = 0;
            double covarianceWeightSum = 0;

            // Assign each sigma point a mean weight and covariance weight for use in the weighted avgs
            for (int i = 0; i < NumSigmaPoints; i++)
            {
                if (i == 0)
                {
                    this.MeanWeights[i] = Lambda / (NumStateVars + Lambda);
                    this.CovarianceWeights[i] = this.MeanWeights[i] + 1 - Math.Pow(alpha, 2) + beta;
                }
                else
                {
                    this.MeanWeights[i] = 1 / (2.0 * (NumStateVars + Lambda));
                    this.CovarianceWeights[i] = 1 / (2.0 * (NumStateVars + Lambda));
                }

                meanWeightSum += this.MeanWeights[i];
                covarianceWeightSum += this.CovarianceWeights[i];
            }

            // Ensure the weights add up to 1
            if (Math.Abs(meanWeightSum - 1) > Math.Pow(10, -8))
            {
                Console.WriteLine(meanWeightSum.ToString());
                throw new Exception("Mean weight sum is incorrect.");
            }
            if (Math.Abs(covarianceWeightSum - 1) > Math.Pow(10, -8))
            {
                Console.WriteLine(covarianceWeightSum.ToString());
                throw new Exception("Covariance weight sum is incorrect.");
            }
        }

        /// <summary>
        /// Initialize the UKF
        /// </summary>
        /// <param name="ProcessNoise">The uncertainty in the state model. (NumStateVars x NumStateVars)</param>
        /// <param name="MeasurementNoise">The uncertainty in the sensor model. (NumMeasurementVars x NumMeasurementVars)</param>
        /// <param name="InitialState">Initial estimate of the state. (NumStateVars x 1)</param>
        /// <param name="InitialCovariance">Initial uncertainty. (NumStateVars x NumStateVars)</param>
        /// <exception cref="Exception">If dimensions are wrong</exception>
        public void Initialize(Matrix<double> ProcessNoise, Matrix<double> MeasurementNoise, Vector<double> InitialState, Matrix<double> InitialCovariance)
        {
            // Check dimensions
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

            // Assign inputs to class fields.
            this.CurrState = InitialState;
            this.ProcessNoise = ProcessNoise;
            this.MeasurementNoise = MeasurementNoise;
            this.Covariance = InitialCovariance;
        }

        /// <summary>
        /// Perform the UKF prediction step
        /// </summary>
        /// <param name="stateModel">The state model. 
        /// Must input a vector of length NumStateVars (representing the previous state) and output a vector of the same length (the new state).</param>
        public void Predict(Func<double[], double[]> stateModel)
        {
            // Step 1: Assemble sigma points

            Matrix<double> SigmaPoints = CalculateSigmaPoints(this.CurrState, this.Covariance);

            // Step 2: Pass sigma points through actuation model

            List<double[]> TransformedSigmaPointsColumns = new List<double[]>() { };
            for (int i = 0; i < NumSigmaPoints; i++)
            {
                TransformedSigmaPointsColumns.Add(stateModel(SigmaPoints.Column(i).ToArray()));
            }

            Matrix<double> TransformedSigmaPoints = Matrix<double>.Build.DenseOfColumnArrays(TransformedSigmaPointsColumns);

            // Step 3: Use the weights and the transformed sigma points to calculate the predicted mean

            Vector<double> PredictedMean = Vector<double>.Build.Dense(NumStateVars);

            for(int i = 0; i < NumSigmaPoints; i++)
            {
                PredictedMean = PredictedMean + MeanWeights[i] * TransformedSigmaPoints.Column(i);
            }

            this.CurrState = PredictedMean;

            // Step 4: Use the weights and the transformed sigma points to calculate the predicted covariance

            Matrix<double> PredictedCovariance = Matrix<double>.Build.Dense(NumStateVars, NumStateVars);

            for(int i = 0; i < NumSigmaPoints; i++)
            {
                PredictedCovariance = PredictedCovariance + CovarianceWeights[i] * Matrix<double>.Build.DenseOfColumnVectors(TransformedSigmaPoints.Column(i) - PredictedMean) * Matrix<double>.Build.DenseOfColumnVectors(TransformedSigmaPoints.Column(i) - PredictedMean).Transpose();
            }

            // Add the process noise to the covariance
            PredictedCovariance = PredictedCovariance + ProcessNoise;

            this.Covariance = PredictedCovariance;

            return;
        }

        /// <summary>
        /// Perform the UKF correction step
        /// </summary>
        /// <param name="Observation">Vector of length NumMeasurementVars representing the data received from the sensor</param>
        /// <param name="measurementModel">Function that contains the measurement model. 
        /// Inputs vector of length NumMeasurementVars and outputs vector of length NumStateVars.</param>
        public void Correct(Vector<double> Observation, Func<double[], double[]> measurementModel)
        {
            // Step 1: Assemble sigma points

            Matrix<double> SigmaPoints = CalculateSigmaPoints(this.CurrState, this.Covariance);

            // Step 2: Pass sigma points through measurement model

            List<double[]> TransformedSigmaPointsColumns = new List<double[]>() { };
            for (int i = 0; i < NumSigmaPoints; i++)
            {
                TransformedSigmaPointsColumns.Add(measurementModel(SigmaPoints.Column(i).ToArray()));
            }

            Matrix<double> TransformedSigmaPoints = Matrix<double>.Build.DenseOfColumnArrays(TransformedSigmaPointsColumns);

            // Step 3: Use weights and transformed sigma points to calculate the predicted measurement

            Vector<double> PredictedMeasurement = Vector<double>.Build.Dense(NumMeasurementVars);

            for (int i = 0; i < NumSigmaPoints; i++)
            {
                PredictedMeasurement = PredictedMeasurement + MeanWeights[i] * TransformedSigmaPoints.Column(i);
            }

            // Step 4: Calculate S matrix

            Matrix<double> S = Matrix<double>.Build.Dense(NumMeasurementVars, NumMeasurementVars);

            for(int i = 0; i < NumSigmaPoints; i++)
            {
                S = S + CovarianceWeights[i] * Matrix<double>.Build.DenseOfColumnVectors(TransformedSigmaPoints.Column(i) - PredictedMeasurement) * Matrix<double>.Build.DenseOfColumnVectors(TransformedSigmaPoints.Column(i) - PredictedMeasurement).Transpose();
            }

            S = S + MeasurementNoise;

            // Step 5: Calculate measurement covariance

            Matrix<double> MeasurementCovariance = Matrix<double>.Build.Dense(NumStateVars, NumMeasurementVars);

            for (int i = 0; i < NumSigmaPoints; i++)
            {
                MeasurementCovariance = MeasurementCovariance + CovarianceWeights[i] * Matrix<double>.Build.DenseOfColumnVectors(SigmaPoints.Column(i) - this.CurrState) * Matrix<double>.Build.DenseOfColumnVectors(TransformedSigmaPoints.Column(i) - PredictedMeasurement).Transpose();
            }

            // Step 6: Calculate Kalman gain

            Matrix<double> KalmanGain = MeasurementCovariance * S.Inverse();

            // Step 7: Calculate corrected mean

            Vector<double> CorrectedMean = this.CurrState + KalmanGain * (Observation - PredictedMeasurement);

            this.CurrState = CorrectedMean;

            // Step 8: Calculate corrected covariance

            Matrix<double> CorrectedCovariance = this.Covariance - KalmanGain * S * KalmanGain.Transpose();

            this.Covariance = CorrectedCovariance;

            return;
        }

        /// <summary>
        /// Calculates the sigma points used for sampling in the UKF
        /// </summary>
        /// <param name="Mean"></param>
        /// <param name="Covariance"></param>
        /// <returns></returns>
        private Matrix<double> CalculateSigmaPoints(Vector<double> Mean, Matrix<double> Covariance)
        {
            Matrix<double> SigmaPoints = Matrix<double>.Build.Dense(NumStateVars, NumSigmaPoints);

            Matrix<double> MatrixSquareRoot = Math.Sqrt(NumStateVars + this.Lambda) * Covariance.Cholesky().Factor;
            //MatrixSquareRoot = MatrixSquareRoot.Transpose();

            for (int i = 0; i < NumSigmaPoints; i++)
            {
                if (i == 0)
                {
                    SigmaPoints.SetSubMatrix(0, NumStateVars, i, 1, Matrix<double>.Build.DenseOfColumnVectors(Mean));
                }
                else if (i <= NumStateVars)
                {
                    SigmaPoints.SetSubMatrix(0, NumStateVars, i, 1, Matrix<double>.Build.DenseOfColumnVectors(Mean + MatrixSquareRoot.Column(i - 1))); 
                }
                else
                {
                    SigmaPoints.SetSubMatrix(0, NumStateVars, i, 1, Matrix<double>.Build.DenseOfColumnVectors(Mean - MatrixSquareRoot.Column(i - 1 - NumStateVars)));
                }
            }
            
            return SigmaPoints;
        }

    }
}
