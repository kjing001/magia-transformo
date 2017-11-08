#include <Adafruit_NeoPixel.h>
#ifdef __AVR__
  #include <avr/power.h>
#endif

#define PIN 6
int NUM_LEDS = 40;
// Parameter 1 = number of pixels in strip
// Parameter 2 = Arduino pin number (most are valid)
// Parameter 3 = pixel type flags, add together as needed:
//   NEO_KHZ800  800 KHz bitstream (most NeoPixel products w/WS2812 LEDs)
//   NEO_KHZ400  400 KHz (classic 'v1' (not v2) FLORA pixels, WS2811 drivers)
//   NEO_GRB     Pixels are wired for GRB bitstream (most NeoPixel products)
//   NEO_RGB     Pixels are wired for RGB bitstream (v1 FLORA pixels, not v2)
//   NEO_RGBW    Pixels are wired for RGBW bitstream (NeoPixel RGBW products)
Adafruit_NeoPixel strip = Adafruit_NeoPixel(40, PIN, NEO_GRB + NEO_KHZ800);

// IMPORTANT: To reduce NeoPixel burnout risk, add 1000 uF capacitor across
// pixel power leads, add 300 - 500 Ohm resistor on first pixel's data input
// and minimize distance between Arduino and first pixel.  Avoid connecting
// on a live circuit...if you must, connect GND first.

// serial read
String s;   // for incoming serial data

char c;
float b = 1.0;

const int HIGH_STRIKE_LIKELIHOOD = 5;
const int LOW_STRIKE_LIKELIHOOD = 10;
int currentDataPoint = 0;
int chance = LOW_STRIKE_LIKELIHOOD;

int strikeTimes = 250;

// Simple moving average plot
int NUM_Y_VALUES = 17;

float yValues[] = {
  0,
  7,
  10,
  9,
  7.1,
  7.5,
  7.4,
  12,
  15,
  10,
  0,
  3,
  3.5,
  4,
  1,
  7,
  1
};

float simple_moving_average_previous = 0;
float random_moving_average_previous = 0;

float (*functionPtrs[10])(); //the array of function pointers
int NUM_FUNCTIONS = 2;

bool condition = true;

int t = 0; // lightning time

void setup() {

  strip.begin();
  strip.show(); // Initialize all pixels to 'off'

    // initializes the array of function pointers.
  functionPtrs[0] = simple_moving_average;
  functionPtrs[1] = random_moving_average;
  
  Serial.begin(9600);
  
  
}

void loop() {
  // Some example procedures showing how to display to the pixels:
  //colorWipe(strip.Color(255, 0, 0), 50); // Red
  // colorWipe(strip.Color(0, 255, 0), 50); // Green

//  if (random(chance) == 9) {
//    int led = random(NUM_LEDS);
//    for (int i = 0; i < 10; i++) {
//      // Use this line to keep the lightning focused in one LED.
//      // lightningStrike(led):
//      // Use this line if you want the lightning to spread out among multiple LEDs.
//      lightningStrike(random(NUM_LEDS));
//    }
//    // Once there's been one strike, I make it more likely that there will be a second.
//    chance = HIGH_STRIKE_LIKELIHOOD;
//  } else {
//    chance = LOW_STRIKE_LIKELIHOOD;
//  }
//  turnAllPixelsOff();
//  delay(10);

    condition = true;
    t = 0;
  
    if (Serial.available() > 0) {
        // read the incoming string:
        //s = " ";
        c = (char)Serial.read();
        //delay(100);
        if (c == 'Z')
        {
          colorWipe(strip.Color(0, 0, 0), 20); // No light
        }
        else if (c == 'f')
        {
          theaterChase(strip.Color(255*b, 0, 0), 50); // Red
        }
        else if (c == 'F')
        {
          colorWipe(strip.Color(255 * b, 0, 0), 10/b); // Red
        }
        else if (c == 'W')
        {
          colorWipe(strip.Color(0, 0, 255 * b), 10/b); // Blue
        }
        else if (c == 'K')
        {
          colorWipe(strip.Color(0, 0, 255), 60); // Knowledge
        }
        else if (c == 'w')
        {
          theaterChase(strip.Color(0, 0, 255 * b), 10/b); // Blue
        }
        else if (c == 'E')
        {
          colorWipe(strip.Color(165 * b, 42 * b, 42 * b), 10/b); // peru - earth color
        }
        else if (c == 'e')
        {
          theaterChase(strip.Color(165 * b, 42 * b, 42 * b), 10/b); // peru - earth color
        }
        else if (c == 'A')
        {
          colorWipe(strip.Color(240*b, 255*b, 255*b), 10/b); // azure - air color
        }           
        else if (c == 'a')
        {
          theaterChase(strip.Color(240*b, 255*b, 255*b), 10/b); // azure - air color
        }              
        else if (c == 'N')
        {
          colorWipe(strip.Color(255*b, 215*b, 0), 10/b); // gold - energy color
        }
        else if (c == 'n')
        {
          theaterChase(strip.Color(255*b, 215*b, 0), 10/b); // gold - energy color
        }
        else if (c == 'D')
        {
          colorWipe(strip.Color(128*b, 0, 128*b), 10/b); // purple - darkness color
        }
        else if (c == 'd')
        {
          theaterChase(strip.Color(128*b, 0, 128*b), 10/b); // purple - darkness color
        }

        // Rituals
        else if (c == 'g')  // left
        {
           colorWipe(strip.Color(255, 255, 255), 80); // Left
        }
        else if (c == 'h')  // right
        {
           colorWipe(strip.Color(255, 0, 255), 80); // Right
        }
        
        else if (c == 'X')  // Join us in our wild ride // By ancient Abrem-melin
        {
           //randomCauldron();
           rainbow(20);
        }

        else if (c == 'O')  // Dancing circles in the night // By slumbering Merlin
        {
           rainbowCycle(20);
        }
        // ritual
        else if (c == 'S') // Summon wisdom for our rite 
        {
          theaterChaseRainbow(20);
        }
        else if (c == 's')  // By the studious Starhawk
        {
          LightningEffectLoop(4, strikeTimes); 
        }
        
        else if (c == 'Y') // Spirits from the other side // We conjure power! // GoldenLightning
        {
          LightningEffectLoop(2, strikeTimes); 
        }

        else if (c == 'I') // We conjure wisdom!
        {
          LightningEffectLoop(3, strikeTimes); 
        }
        else if (c == 'L') // Lightning // Torch the air and light the skies
        {
          LightningEffectLoop(1, strikeTimes); 
        }
        else if (c == 'p') // darkness Lightning
        {
          LightningEffectLoop(5, strikeTimes); 
        }
        else if (c == 'R') // fire from the cauldron rise
        {
          LightningEffectLoop(0, strikeTimes); 
        }

        else if (c == 'j') // Spin1
        {
          colorWipe(strip.Color(0, 0, 0), 20); // No light
          theaterChase(strip.Color(255, 0, 0), 50); // Red          
        }
        else if (c == 'k') // Spin2
        {
          colorWipe(strip.Color(0, 0, 0), 20); // No light
          theaterChase(strip.Color(255, 255, 0), 50); // yellow          
        }
        else if (c == 'l') // Spin3
        {
          colorWipe(strip.Color(0, 0, 0), 20); // No light
          theaterChase(strip.Color(0, 255, 0), 50); // green
        }
        else if (c == 'c') // center
        {
          colorWipe(strip.Color(0, 0, 0), 20);
          rainbow(20);
          
        }
        else if (c == 'o') // GoldenCircle
        {
          colorWipe(strip.Color(0, 0, 0), 10);
          colorWipe(strip.Color(255, 200, 0), 50);
          colorWipe(strip.Color(0, 0, 0), 20);
        }
        else if (c == 'i') // HiddenCircle
        {
          colorWipe(strip.Color(0, 0, 0), 10);
          colorWipe(strip.Color(255, 0, 255), 50);
          colorWipe(strip.Color(0, 0, 0), 20);
        }
        else if (c == 'G') // golden societies
        {
            for(int i=0; i<3; i++){
              theaterChase(strip.Color(127, 0, 0), 50); // Red
              theaterChase(strip.Color(127, 127, 127), 50); // White
              theaterChase(strip.Color(255, 215, 0), 50); // gold              
              theaterChaseRainbow(50);
            }

        }
        else if (c == 'H') // hidden orders
        {
          for(int i=0; i<3; i++){
            theaterChase(strip.Color(0, 0, 255), 50); // Blue            
            theaterChase(strip.Color(255, 215, 0), 50); // Peru - energy color
            theaterChase(strip.Color(128, 0, 128), 50); // purple - darkness color
            theaterChaseRainbow(50);
          }  
        }
        else if (c == 'B')
        {
          b = 1.0;
        }
        
        else if (c == 'M')
        {
            b = 0.5;
        }

        else if (c == 'N')
        {
          b = 0;
        }

        else if (c == 'n')
        {
          strikeTimes = strikeTimes - 50;
          if(strikeTimes <= 0)
            strikeTimes = 50;
       
        }

        else if (c == 'b')
        {
          strikeTimes = strikeTimes + 50;
        }

        delay(100);

    }

 
//  colorWipe(strip.Color(0, 0, 255), 50); // Blue
//colorWipe(strip.Color(0, 0, 0, 255), 50); // White RGBW

  // Send a theater pixel chase in...
  // theaterChase(strip.Color(127, 127, 127), 50); // White
  // theaterChase(strip.Color(127, 0, 0), 50); // Red
  // theaterChase(strip.Color(0, 0, 127), 50); // Blue

  // rainbow(20);
  // rainbowCycle(20);
  // theaterChaseRainbow(50);
}

void randomCauldron(){
      rainbow(20);
      rainbowCycle(20);
      theaterChaseRainbow(50);    

      colorWipe(strip.Color(255*b, 0, 0), 50); // Red
      colorWipe(strip.Color(0, 255*b, 0), 50); // Green
      colorWipe(strip.Color(0, 0, 255*b), 50); // Blue
    //colorWipe(strip.Color(0, 0, 0, 255), 50); // White RGBW
      // Send a theater pixel chase in...
      theaterChase(strip.Color(127*b, 127*b, 127*b), 50); // White
      theaterChase(strip.Color(127*b, 0, 0), 50); // Red
      theaterChase(strip.Color(0, 0, 127*b), 50); // Blue
}

// Fill the dots one after the other with a color
void colorWipe(uint32_t c, uint8_t wait) {
  for(uint16_t i=0; i<strip.numPixels(); i++) {
    strip.setPixelColor(i, c);
    strip.show();
    delay(wait);
  }
}

void rainbow(uint8_t wait) {
  uint16_t i, j;

  for(j=0; j<256; j++) {
    for(i=0; i<strip.numPixels(); i++) {
      strip.setPixelColor(i, Wheel((i+j) & 255));
    }
    strip.show();
    delay(wait);
  }
}

// Slightly different, this makes the rainbow equally distributed throughout
void rainbowCycle(uint8_t wait) {
  uint16_t i, j;

  for(j=0; j<256*5; j++) { // 5 cycles of all colors on wheel
    for(i=0; i< strip.numPixels(); i++) {
      strip.setPixelColor(i, Wheel(((i * 256 / strip.numPixels()) + j) & 255));
    }
    strip.show();
    delay(wait);
  }
}

//Theatre-style crawling lights.
void theaterChase(uint32_t c, uint8_t wait) {
  for (int j=0; j<10; j++) {  //do 10 cycles of chasing
    for (int q=0; q < 3; q++) {
      for (uint16_t i=0; i < strip.numPixels(); i=i+3) {
        strip.setPixelColor(i+q, c);    //turn every third pixel on
      }
      strip.show();

      delay(wait);

      for (uint16_t i=0; i < strip.numPixels(); i=i+3) {
        strip.setPixelColor(i+q, 0);        //turn every third pixel off
      }
    }
  }
}

//Theatre-style crawling lights with rainbow effect
void theaterChaseRainbow(uint8_t wait) {
  for (int j=0; j < 256; j++) {     // cycle all 256 colors in the wheel
    for (int q=0; q < 3; q++) {
      for (uint16_t i=0; i < strip.numPixels(); i=i+3) {
        strip.setPixelColor(i+q, Wheel( (i+j) % 255));    //turn every third pixel on
      }
      strip.show();

      delay(wait);

      for (uint16_t i=0; i < strip.numPixels(); i=i+3) {
        strip.setPixelColor(i+q, 0);        //turn every third pixel off
      }
    }
  }
}

// Input a value 0 to 255 to get a color value.
// The colours are a transition r - g - b - back to r.
uint32_t Wheel(byte WheelPos) {
  WheelPos = 255 - WheelPos;
  if(WheelPos < 85) {
    return strip.Color(255 - WheelPos * 3, 0, WheelPos * 3);
  }
  if(WheelPos < 170) {
    WheelPos -= 85;
    return strip.Color(0, WheelPos * 3, 255 - WheelPos * 3);
  }
  WheelPos -= 170;
  return strip.Color(WheelPos * 3, 255 - WheelPos * 3, 0);
}

void LightningEffectLoop(int i, int st) {
    condition = true;
    t =0;
    
    while(condition){            
      LightningEffect(i);
      t = t + 1;
      if(t >= st){   //int strikeTimes = 200;
        condition = false;
      }             
    }         
}
void LightningEffect(int j){
  if (random(chance) == 9) {
    int led = random(NUM_LEDS);
    for (int i = 0; i < 10; i++) {
      // Use this line to keep the lightning focused in one LED.
      // lightningStrike(led):
      // Use this line if you want the lightning to spread out among multiple LEDs.
      
        Strike(random(NUM_LEDS), j);
      }
     
    // Once there's been one strike, I make it more likely that there will be a second.
    chance = HIGH_STRIKE_LIKELIHOOD;
  } else {
    chance = LOW_STRIKE_LIKELIHOOD;
  }
  turnAllPixelsOff();
  delay(10);
}


void turnAllPixelsOff() {
  for (int i = 0; i < NUM_LEDS; i++) {
    strip.setPixelColor(i, 0);
  }
  strip.show();
}

void Strike(int pixel, int i) {
  float brightness = callFunction(random(NUM_FUNCTIONS));
  float scaledWhite = abs(brightness*500);

  if (i == 0){
    strip.setPixelColor(pixel, strip.Color(scaledWhite, 0, 0));
  }
  else if (i==1){
    strip.setPixelColor(pixel, strip.Color(scaledWhite, scaledWhite, scaledWhite));
  }
  else if(i==2){
    strip.setPixelColor(pixel, strip.Color(scaledWhite, (int)(scaledWhite *215/255), 0));
  }
  else if(i==3){ // blue
     strip.setPixelColor(pixel, strip.Color(0, 0, scaledWhite));
  }
  else if (i == 4){ // brown
     strip.setPixelColor(pixel, strip.Color((int)(scaledWhite * 165/255), (int)(scaledWhite *42/255), (int)(scaledWhite *42/255)) );
  }
  else if (i == 5){ // purple
     strip.setPixelColor(pixel, strip.Color(scaledWhite, 0, scaledWhite) );
  }
  
  strip.show();
  delay(random(0, 7));
  currentDataPoint++;
  currentDataPoint = currentDataPoint%NUM_Y_VALUES;
}

float callFunction(int index) {
  return (*functionPtrs[index])(); //calls the function at the index of `index` in the array
}

// https://en.wikipedia.org/wiki/Moving_average#Simple_moving_average
float simple_moving_average() {
  uint32_t startingValue = currentDataPoint;
  uint32_t endingValue = (currentDataPoint+1)%NUM_Y_VALUES;
  float simple_moving_average_current = simple_moving_average_previous + 
                                  (yValues[startingValue])/NUM_Y_VALUES - 
                                  (yValues[endingValue])/NUM_Y_VALUES;

  simple_moving_average_previous = simple_moving_average_current;
  return simple_moving_average_current;
}


// Same as simple moving average, but with randomly-generated data points.
float random_moving_average() {
  float firstValue = random(1, 10);
  float secondValue = random(1, 10);
  float random_moving_average_current = random_moving_average_previous +
                                  firstValue/NUM_Y_VALUES -
                                  secondValue/NUM_Y_VALUES;
  random_moving_average_previous = random_moving_average_current;

  return random_moving_average_current;
}


