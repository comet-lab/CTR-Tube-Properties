# C# Infrastructure

This folder contains the C# codebase used to conduct the Kalman filter experiments. 
It contains the following classes (in the CAPISampleApplication folder):

## Robot

This class allows you to control the CTR via serial comms with the control board. 
It also allows you to calculate the forward kinematics of the CTR. 
The forward kinematics implementation is based on the piecewise constant curvature model presented in [Webster 2009](https://ieeexplore.ieee.org/document/4671090) and **does not** account for torsion.

This class also includes a method called Fkin_Modified() that calculates the forward kinematics of a CTR up to a certain tube.
This method is useful if you have placed markers on the tips of multiple tubes and need to calculate where the tip of each tube is expected to be.

## Tube

This is a helper class used for kinematics calculations. 
You need to create 1 or more Tube objects in order to create a Robot object.

## Encoders

This class allows you to communicate with the encoder board (Arduino Mega) via serial wire.
It also translates the encoder readings into usable measurements (in mm or deg).

This class contains instances of an internal class, EncoderChars.
This internal class holds the encoder-specific properties that relate encoder counts to mm/deg.
You need to find this data empirically.

## Mocap

This class allows you to interface with the motion capture sensor. 
It contains the default methods provided by NDI as part of the mocap's SDK as well as higher-level methods that I created to be more useful as part of the larger codebase.
You can find the tool(s) and stray(s) observed by the sensor and perform calculations to find the position/orientation of those objects.

## EKF

This class contains an implementation of the Extended Kalman Filter. 
It has a few quirks:
- The Predict() method, which handles the prediction step of the filter, requires you to pass as a parameter a method that takes in the state vector at time _t-1_ and outputs the state vector at time _t_.
  It also requires you to pass a list of methods, each of which input the state vector at time _t-1_ and output a single state variable at time _t_. The list of individual methods is required for the Jaobian estimation step.
- The Correct() method assumes the relation between the sensor output and the state vector is linear, i.e. the measurement vector is comprised of only some or all of the variables in the state vector.
  This assumption works if you are using the motion capture sensor to measure the position of a marker on the CTR and the marker's position is included in the state vector. If this assumption does not hold, you need to alter the Correct() method.

## UKF

This class contains an implementation of the Unscented Kalman Filter.
Because the UKF does not involve Jacobians, you do not need to pass a list of individual state methods.
This class also does not assume that the measurement function is linear.
For reference, I created this class using [this tutorial for reference](https://www.youtube.com/watch?v=DWDzmweTKsQ).
I followed the same general structure when creating this class as I did for the EKF, so the two types of filters can be interchanged easily.

## Program

This is the main class.
It contains example functions to show you how to interface with the robot, motion capture, and encoders.
It also includes some of this functions I used to run Kalman filter experiments, both in simulation and on a real CTR.

# How to Run the Code

1. Install Visual Studio for Windows.
2. Open CAPISample.sln in Visual Studio.
3. Ensure you have required dependencies:
    1. MathNet.Numerics*
    2. MathNet.Numerics.Data.Text
    3. Zeroconf
    4. Newtonsoft.Json
4. Build the project. You can either build through Visual Studio's UI (make sure the configuration is set to "Release") or you can build throught the command line: `dotnet build CAPISample.sln -c Release`.
5. Run the code. The executable can be found in `CAPISampleApplication\bin\Release\netcoreapp3.1`.

* I'd like to take this opportunity to sing the praises of the Math.NET Numerics library. It does all kinds of cool math things that would otherwise seem impossible to accomplish in C#. Check out the source code [here](https://github.com/mathnet/mathnet-numerics).

# Troubleshooting

The code is relatively well documented, but it's understandable if some parts are confusing. Feel free to contact Chris if something doesn't make sense or if you have issues running the code.

christopherdemaio1@gmail.com
