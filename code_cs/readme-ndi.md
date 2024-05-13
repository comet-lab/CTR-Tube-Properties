# NDI C# CAPI Sample

The Combined API (CAPI) sample application demonstrates how to use the Combined API for Polaris and Aurora
systems in a C# application to communicate with an NDI measurement system. The source code is intended as a
reference to help developers become familiar with NDI's API.

Please see [license.txt](license.txt) for the terms associated with the provision of this sample code.

By default, CAPI Sample will connect to a device, initialize it and issue a series of tracking
API commands that illustrate the basic principles of communicating with the measurement system.
The CAPISampleApplication does not issue every possible API command, but focuses on the most common
and basic tasks including:

   - Connecting to an ethernet or serial port device (via serial-to-USB converter)
   - Initializing the system
   - Loading and initializing passive and active tools (if an SCU is connected)
   - Getting/setting user parameters stored in the device firmware
   - Sending BX or BX2 to retrieve tracking data and printing it to the terminal

There is a second sample application CAPISampleStreaming, that outlines a sample implementation of
the STREAM command for devices that support API revision G.003 or newer.

The source code for CAPI Sample is provided so it can be analyzed and modified as desired.
Developers may want to:

   - Explore simulating alerts and how the alert data is transmitted to the application
   - Investigate using dummy tools to return 3D data
   - Refer to the API guide for their system to add commands that CAPI Sample didn't implement
   - Completely gut and rewrite CAPI Sample as a starting point for their project

### Platform Support

The C# CAPI Sample has only been tested on Windows. Thought it is expected to work on other platforms. For details on 
building for specific platforms, please see the Building section below.

- Windows: Tested
- Linux: Untested
- Mac: Untested

### Runtime Dependencies

- .NET Core 3.1 Runtime - https://dotnet.microsoft.com/download/dotnet-core/thank-you/runtime-3.1.9-windows-x64-installer

## Usage

You can find a precompiled version of the CAPI Sample in the bin directory for each subproject.

The `CAPISampleApplication` will try to connect to the first available serial device on your computer, if it cannot
connect to a serial device, then it will try to find an NDI device on your network using Zeroconf. If the application
is unable to find either type of device, it will output the help as shown below. You can also skip the device
discovery by specifying the hostname or serial device name as a parameter.

```
2020-10-20T19:28:15.649Z [CAPISample] C# CAPI Sample v1.0.0.0
2020-10-20T19:28:15.651Z [CAPISample] Finding serial NDI devices.
2020-10-20T19:28:15.656Z [CAPISample] Could not find a serial device.
2020-10-20T19:28:15.656Z [CAPISample] Finding an NDI device on the network.
2020-10-20T19:28:18.314Z [CAPISample] Could not find an NDI device on the network.
2020-10-20T19:28:18.314Z [CAPISample] Could not automatically detect an NDI device, please manually specify one.
2020-10-20T19:28:18.314Z [CAPISample]
2020-10-20T19:28:18.314Z [CAPISample] usage: TestNDICapiCSharp.exe [<hostname>]
2020-10-20T19:28:18.314Z [CAPISample] where:
2020-10-20T19:28:18.314Z [CAPISample]      <hostname>      (optional) The measurement device's hostname, IP address, or serial port.
2020-10-20T19:28:18.314Z [CAPISample] example hostnames:
2020-10-20T19:28:18.314Z [CAPISample]      Connecting to device by IP address: 169.254.8.50
2020-10-20T19:28:18.314Z [CAPISample]      Connecting to device by hostname: P9-B0103.local
2020-10-20T19:28:18.314Z [CAPISample]      Connecting to serial port varies by operating system:
2020-10-20T19:28:18.314Z [CAPISample]              COM10 (Windows), /dev/ttyUSB0 (Linux), /dev/cu.usbserial-001014FA (Mac)
```

### SROM Files

For NDI Polaris optical position sensors, the `CAPISampleApplication` will try to load the reference rigid body
file `sroms/8700339.rom`. The `sroms` directory can be found at the root of this package, copy the directory
next to the executable you wish to run. You will also need to have the physical rigid body tool inside the
position sensor's tracking volume.

## Build Setup

The CAPI Sample was developed using .Net Core 3.1.9.

### Tools

- .NET Core 3.1.9 - https://dotnet.microsoft.com/download/dotnet-core/3.1

### Dependencies

The CAPI Sample Application uses the [Zeroconf](https://www.nuget.org/packages/Zeroconf/) Nuget package to demonstrate network discovery of NDI devices.

You can restore the dependencies using `dotnet restore CAPISample.sln`. The `dotnet` build process will do this automatically for you when you first build.

If you want, you may remove the Zeroconf dependency and it's corresponding code in the `CAPISampleApplication` project, but you will need to manually specify a device IP address to connect.

## Building

### Console Build

1. Install .NET Core 3.1.9
2. Add the .NET Core install directory to your path if not already present
3. From the root of this package run: `dotnet build CAPISample.sln -c Release` to build for your current platform.
  - To build for other platforms, see the [--runtime](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build#examples) parameter for `dotnet build`.
  - Ex. `dotnet build CAPISample.sln -c Release -r ubuntu.18.04-x64`
4. Navigate to `./CAPISampleApplication/bin/Release/netcoreapp3.1/` to find the built sample application.

