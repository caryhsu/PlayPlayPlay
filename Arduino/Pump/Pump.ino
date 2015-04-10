const int buttonPin = 12;     // the number of the pushbutton pin
const int ledPin =  13;      // the number of the LED pin
int buttonState = 0;         // variable for reading the pushbutton status

void setup() {
	pinMode(ledPin, OUTPUT);
	pinMode(buttonPin, INPUT);
	Serial.begin(9600);
}

void loop() {
	int buttonState = digitalRead(buttonPin);
	if (buttonState == HIGH) {
		digitalWrite(ledPin, HIGH);
		Serial.print("1"); 
	}
	else {
		digitalWrite(ledPin, LOW);
	}
	delay(100);
}
