#include <SPI.h>
/*PINS
   Arduino SPI pins
   MOSI = 11, MISO = 12, SCK = 13, CS = 10
   STM32 SPI pins
   MOSI = PA7, MISO = PA6, SCK = PA5, CS = PA4
*/

uint16_t command = 0b1111111111111111; //read command (0xFFF)

// float timer = 0; //timer for updating the LCD and sending the data through the serial port
// float rpmTimer = 0; //timer for estimating the RPM

class Motor
{
  public:
  
    byte pin;
    double startAngle = 0;
    double totalAngle = 0;
    int turnsCounter = 0;
    int quadrant = 0;

    Motor(byte pinNum)
    {
      pin = pinNum;    
    }

    // Reset the motor
    void init()
    {
      pinMode(pin, OUTPUT); //CS pin - output
      digitalWrite(pin, HIGH);

      turnsCounter = 0;
      quadrant = 1;
      startAngle = readRegister();
    }

    double getTotalAngle()
    {
      double degAngle = readRegister(); //read the position of the magnet

      double correctedAngle = correctAngle(degAngle); //normalize the previous reading

      totalAngle = checkQuadrant(correctedAngle); //check the direction of the rotation and calculate the final displacement

      return totalAngle;
    }

    // Get the raw reading from the encoder
    double readRegister()
    {
      uint16_t rawData = 0; //bits from the encoder (16 bits, but top 2 has to be discarded)
      double degAngle = 0; //Angle in degrees
      
      SPI.beginTransaction(SPISettings(3000000, MSBFIRST, SPI_MODE1));

      //--sending the command
      digitalWrite(pin, LOW);
      SPI.transfer16(command);
      digitalWrite(pin, HIGH);

      delay(10);

      //--receiving the reading
      digitalWrite(pin, LOW);
      rawData = SPI.transfer16(command);
      digitalWrite(pin, HIGH);

      SPI.endTransaction();

      rawData = rawData & 0b0011111111111111; //removing the top 2 bits (PAR and EF)

      degAngle = (double)rawData / (double)16384.0 * (double)360.0; //16384 = 2^14, 360 = 360 degrees

      return degAngle;
    }

    double correctAngle(float degAngle)
    {
      //recalculate angle
      double correctedAngle = degAngle - startAngle; //this tares the position

      if (correctedAngle < 0) //if the calculated angle is negative, we need to "normalize" it
      {
        correctedAngle = correctedAngle + (double)360.0; //correction for negative numbers (i.e. -15 becomes +345)
      }
      else
      {
        //do nothing
      }

      return correctedAngle;
    }

    double checkQuadrant(double correctedAngle)
    {
      /*
        //Quadrants:
        4  |  1
        ---|---
        3  |  2
      */

      int newQuadrant = 0;

      //Quadrant 1
      if (correctedAngle >= 0 && correctedAngle <= 90)
      {
        newQuadrant = 1;
      }

      //Quadrant 2
      if (correctedAngle > 90 && correctedAngle <= 180)
      {
        newQuadrant = 2;
      }

      //Quadrant 3
      if (correctedAngle > 180 && correctedAngle <= 270)
      {
        newQuadrant = 3;
      }

      //Quadrant 4
      if (correctedAngle > 270 && correctedAngle < 360)
      {
        newQuadrant = 4;
      }

      if (newQuadrant != quadrant) //if we changed quadrant
      {
        
        if (newQuadrant == 1 && quadrant == 4)
        {
          turnsCounter++; // 4 --> 1 transition: CW rotation
        }

        if (newQuadrant == 4 && quadrant == 1)
        {
          turnsCounter--; // 1 --> 4 transition: CCW rotation
        }
        //this could be done between every quadrants so one can count every 1/4th of transition

        quadrant = newQuadrant;  //update to the current quadrant

      }

      //after we have the corrected angle and the turns, we can calculate the total absolute position

      totalAngle = ((double)turnsCounter * (double)360) + correctedAngle; //number of turns (+/-) plus the actual angle within the 0-360 range
      
      return totalAngle;
    }

};


const byte CS_ROT1 = 31; //Chip select pin for manual switching
const byte CS_ROT2 = 32; //Chip select pin for manual switching
const byte CS_ROT3 = 35; //Chip select pin for manual switching
const byte CS_TRANS1 = 30; //Chip select pin for manual switching
const byte CS_TRANS2 = 33; //Chip select pin for manual switching
const byte CS_TRANS3 = 36; //Chip select pin for manual switching

Motor rot1 = Motor(CS_ROT1);
Motor rot2 = Motor(CS_ROT2);
Motor rot3 = Motor(CS_ROT3);

Motor trans1 = Motor(CS_TRANS1);
Motor trans2 = Motor(CS_TRANS2);
Motor trans3 = Motor(CS_TRANS3);

void setup()
{
  SPI.begin();
  Serial.begin(9600);
  Serial.println("AS5048A - Magnetic position encoder");
  
  // //------------------------------------------------------
  // digitalWrite(CS_ROT1, HIGH);
  // digitalWrite(CS_ROT2, HIGH);
  // digitalWrite(CS_ROT3, HIGH);
  //------------------------------------------------------

  // Initialize all the motors

  rot1.init();
  rot2.init();
  rot3.init();

  trans1.init();
  trans2.init();
  trans3.init();
}

void loop()
{
  // Get the latest encoder readings

  double r1Angle = rot1.getTotalAngle();
  double r2Angle = rot2.getTotalAngle();
  double r3Angle = rot3.getTotalAngle();

  double t1Angle = trans1.getTotalAngle();
  double t2Angle = trans2.getTotalAngle();
  double t3Angle = trans3.getTotalAngle();
  
  // If we have received a request over the serial wire...

  if (Serial.available() >= 1)
  {
    // ... read the request
    int readByte = Serial.read();
    char letter = readByte;

    // If the incoming message is "x"...
    if(letter == 'x')
    {
      // ... reset all the motors
      rot1.init();
      rot2.init();
      rot3.init();
      trans1.init();
      trans2.init();
      trans3.init();
      Serial.print((double)0.0);
    }

    // If the request is anything else...
    else
    {
      // ... assemble the encoder readings into one massive string
      String allEncoders = String(t1Angle,1) + "," + String(t2Angle,1) + "," + String(t3Angle,1) + "," + String(r1Angle,1) + "," + String(r2Angle,1) + "," + String(r3Angle,1);

      Serial.print(allEncoders);
    }

    Serial.println();
  }
}