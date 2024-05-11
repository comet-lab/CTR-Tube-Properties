# Motion Capture How-to

This document helps you get started with the NDI Polaris Vega motion capture system.

## Step 1

Complete the safety training required to gain card access to the Operating Room (OR) on the third floor of 50 Prescott Street. 
Chris Nycz used to be in charge of this training, but he has left WPI. 
Ask Prof. Fichera, Nick, or Wenpeng who you should contact to complete this training.

## Step 2

Reserve the device using [this link](https://wpi.ilab.agilent.com/account/login). Log in using your WPI credentials. 
You may need to ask Prof. Fichera to activate your account for you. 
The device is listed as "NDI Vega ST Optical Tracking System" in the iLab system. 
The device doesn't get used too frequently, so feel free to reserve it for the whole day.

## Step 3

Go to the third floor of 50 Prescott Street and get the motion capture device. 
The device is mounted on a tripod and looks like this:

![image](https://github.com/comet-lab/CTR-Tube-Properties/assets/62364581/c6a5890b-b0af-4ea6-9297-5dd5e915eeb2)

The device requires a specific Power-Over-Ethernet (POE) switch to operate. 
You can't use any generic switch as most switches lack sufficient power.
The switch that works for the device looks like this:

[INSERT IMAGE]

## Step 4

Download NDI Track, and associated apps.
The install file is too big to store in GitHub, but it is stored in the COMET Lab's MS Teams file structure.
Navigate to Lab Management -> Files -> Lab Equipment -> NDI Polaris Vega -> Software, then download the file called "Toolbox for Windows...":

![image](https://github.com/comet-lab/CTR-Tube-Properties/assets/62364581/858bffa8-6d52-4f78-b007-feaa80c1acdf)

## Step 5

Connect the motion capture device to your computer via the POE switch.
You will need 2 Ethernet wires to do this: one to connect the sensor to the POE switch, and one to connect the switch to your PC.
You should see a small green light on the front of the sensor, and the sensor should play a sound effect after several seconds.

## Step 6

Open the NDI Track app (the thing you downloaded in Step 4).
The app should automatically connect to the sensor. If it doesn't, try File->Connect To.
Enable reporting of markers by selecting the "Report Stray Markers" option in the "Track" menu in the top left.
You can test connectivity by holding a reflective sphere in the sensor's field of view.
You should see the marker appear on the screen.

# Tools and Strays

The motion capture sensor classifies objects as one of two types: _tools_ and _strays_. 
A _tool_ is defined as a set of 3 or more reflective spheres attached to each other as a rigid object.
A tool has a fixed geometry, such that the sensor can detect the tool regardless of orientation (as long as it can see at least 3 reflective spheres).
Tools are defined by SROM files (see the Tool Definitions folder). 
To load an SROM into the NDI track app, click the icon directly below the "File" tab:

![image](https://github.com/comet-lab/CTR-Tube-Properties/assets/62364581/30116840-5bec-4917-9e61-25f6b5738b6f)

A _stray_ is defined as any reflective object that is not part of a tool. 
Strays are also referred to as "passive" markers. 
Because a stray is represented by a single object, the sensor cannot report an orientation for strays; it can only track position.

We have used two tools in the COMET Lab: the "home frame" and the "needle pointer". 
The home frame is a 3D-printed contraption that attaches to the front of our CTR and represents the CTR's task space home (if you can't find it, ask Wenpeng).
The needle pointer is a manufacturer-provided (I think) object that allows you to point at an object/location of interest:

![image](https://github.com/comet-lab/CTR-Tube-Properties/assets/62364581/8c34f930-edf5-45fe-b5f3-efebf38dc987)

The tool is configured such that the sensor reports the tool's tip as its location.

You can create your own tools using the reflective spheres you will find in the COMET Lab and the OR.

# Controlling the Sensor

The APIs folder contains a C# SDK and a C++ SDK that you can use to control the device.
However (without giving myself too much credit), I recommend using the software infrastructure that I created (in the code_cs folder in the root directory of this repo).
The code you will find there is built upon the C# SDK and contains functionality to do basic motion capture tasks (find tools, find strays) as well as more advanced tasks (transformation matrix math).
The code also includes robot control, kinematics, encoder control, and Kalman filtering.
