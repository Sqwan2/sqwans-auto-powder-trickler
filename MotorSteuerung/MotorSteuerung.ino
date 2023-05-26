#include <TMCStepper.h>         // TMCstepper - https://github.com/teemuatlut/TMCStepper
#include <SoftwareSerial.h>     // Software serial for the UART to TMC2209 - https://www.arduino.cc/en/Reference/softwareSerial
#include <Streaming.h>          // For serial debugging output - https://www.arduino.cc/reference/en/libraries/streaming/

#define EN_PIN           2      // Enable - PURPLE
#define DIR_PIN          3      // Direction - WHITE
#define STEP_PIN         4      // Step - ORANGE
#define SW_SCK           5      // Software Slave Clock (SCK) - BLUE
#define SW_TX            7      // SoftwareSerial receive pin - BROWN
#define SW_RX            6      // SoftwareSerial transmit pin - YELLOW
#define DRIVER_ADDRESS   0b00   // TMC2209 Driver address according to MS1 and MS2
#define R_SENSE 0.11f           // SilentStepStick series use 0.11 ...and so does my fysetc TMC2209 (?)

SoftwareSerial SoftSerial(SW_RX, SW_TX);                          // Be sure to connect RX to TX and TX to RX between both devices

TMC2209Stepper TMCdriver(&SoftSerial, R_SENSE, DRIVER_ADDRESS);   // Create TMC driver

int accel;
long maxSpeed;
int speedChangeDelay;
bool dir = false;

//== Setup ===============================================================================

void setup() {

  Serial.begin(115200);               // initialize hardware serial for debugging
  SoftSerial.begin(11520);           // initialize software serial for UART motor control
  TMCdriver.beginSerial(11520);      // Initialize UART  
  
  pinMode(EN_PIN, OUTPUT);           // Set pinmodes
  pinMode(STEP_PIN, OUTPUT);
  pinMode(DIR_PIN, OUTPUT);
  digitalWrite(EN_PIN, LOW);         // Enable TMC2209 board  

  TMCdriver.begin();                                                                                                                                                                                                                                                                                                                            // UART: Init SW UART (if selected) with default 115200 baudrate
  TMCdriver.toff(5);                 // Enables driver in software
  TMCdriver.rms_current(500);        // Set motor RMS current
  TMCdriver.microsteps(400);         // Set microsteps

  TMCdriver.en_spreadCycle(false);
  TMCdriver.pwm_autoscale(true);     // Needed for stealthChop

  TMCdriver.VACTUAL(0);
  
}
//== Loop =================================================================================
void loop() {
  while (Serial.available() == 0) {}     //wait for data available
  long incByte = 0;
  incByte = Serial.read();
  long delayMs = incByte*1000;

  TMCdriver.VACTUAL(delayMs);
}

//TMCdriver.VACTUAL(10000);                                     // Set motor speed
//Serial << TMCdriver.VACTUAL() << endl;
/**
#include <Arduino.h>

void setup() {
  // put your setup code here, to run once:

}

void loop() {
  // put your main code here, to run repeatedly:
  Serial.begin(115200);
  while (Serial.available() == 0) {}     //wait for data available
  String teststr = Serial.readString();  //read until timeout
  teststr.trim(); 
  Serial.println(teststr);
  
  delay(500);
}
*/
