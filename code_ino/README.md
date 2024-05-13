# Encoder Interfacing

## Hardware

Encoders: [DigiKey Part Num AS5048A-AB-1.0-ND](https://www.digikey.com/en/products/detail/ams-osram-usa-inc/AS5048A-TS-EK-AB/3188612). [Datasheet](https://www.mouser.com/datasheet/2/588/AS5048_DS000298_4_00-2324531.pdf)
Control Board: [Arduino Mega](https://store.arduino.cc/products/arduino-mega-2560-rev3)

## Wiring

The encoders use the SPI interface. Pin configuration is as follows:

MISO: 50

MOSI: 51

SCK: 52

Chip Select (CS) Pins can be any Digital I/O pins. Currently they are configured as follows:

Translation Motor 1: 30

Translation Motor 2: 33

Translation Motor 3: 36

Rotation Motor 1: 31

Rotation Motor 2: 32

Rotation Motor 3: 35

## Operation

Send any ASCII character from your PC to the Arduino over the serial wire other than "x" to receive the encoder readings. 
You will receive the encoder readings in the format "t1,t2,t3,r1,r2,r3" where "tn" is the encoder value of the nth translation motor and "rn" is the encoder value of the nth rotation motor.
The encoder values are given in degrees of rotation of the encoder, meaning you need to do the math to convert them into usable units.
The Encoders.cs class in the C# codebase in this repo can do this conversion for you.

Send the ASCII character "x" from your PC to the Arduino over the serial wire to reset all the encoders to 0.
