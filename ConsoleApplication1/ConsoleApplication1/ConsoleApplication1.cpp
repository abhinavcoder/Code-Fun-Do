// ConsoleApplication1.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#define threshold 85

#define forward 0b00100100

#define left_zero 0b00101000 //defining motor controls

#define right_zero 0b00010100

#define stall 0b00111100

#define back 0b00011000

#define F_CPU 16000000UL

#define left 0b00101100

#define right 0b00110100

#define east 0

#define north 1 //defining directions

#define west 2

#define south 3

#include "LCD.h" //LCD display files

#include "LCD.c"

#include <string.h>

#include <avr/io.h> 

#include <util/delay.h>

#include <avr/interrupt.h>

volatile uint8_t overflow;

int time;

int counter; //global variables

int dist;

int dir;

int X = 0,Y = 0;

void motor_test() //to test motor

{

PORTC = forward;

_delay_ms(1000);

PORTC = right;

_delay_ms(1000);

PORTC = right_zero;

_delay_ms(1000);

PORTC = left;

_delay_ms(1000);

PORTC = left_zero;

_delay_ms(1000);

PORTC = back;

_delay_ms(1000);

}

uint8_t get_IR_sensor_value() //input from 6 LED LDR sensors

{

uint8_t i;

i = PINA;

i = i&(0b00111111); //removing other pins value

return i;

}

void follow_line(uint8_t i) //command to follow line on basis or sensor values
{

switch (i)
{

case 0b00001100:

case 0b00011110:

case 0b00111111:

PORTC = forward;

break;

case 0b00000110:

case 0b00001110:

case 0b00000111:

PORTC = left;

break;

case 0b00011000:

case 0b00011100:

case 0b00111000:

PORTC = right;

break;

case 0b00110000:

case 0b00100000:

PORTC = right_zero;

break;

case 0b00000011:

case 0b00000001:

PORTC = left_zero;

break;

default:

PORTC = forward;

break;

}

}

void initialize()

{

DDRC = 0b11111101; //to output and input selections

PORTC = (1<<PC1);

DDRA = 0b00000000;

DDRB = 0b11111111;

DDRD = 0b11111111;

ADMUX=(1<<REFS0)|(1<<ADLAR)|(1<<MUX1)|(1<<MUX2); //filling ADC registers

ADCSRA=(1<<ADEN)|(1<<ADPS2)|(1<<ADPS1)|(1<<ADPS0);

TCCR2=(1<<CS22)|(1<<CS21)|(1<<CS20); //filling timer counter 

registers

TIMSK|=(1<<TOIE0);

TCCR1A=(1<<WGM10)|(1<<COM1A1)|(1<<COM1B1);

TCCR1B=(1<<WGM12)|(1<<CS10)|(1<<CS11);

OCR1A = 180; //setting PWM to slow down 

motors

OCR1B = 180;

dir=0;

overflow = 0;

}

void next_node() //for travelling between 

nodes

{

uint8_t a;

while(1)

{

a = get_IR_sensor_value();

if (a!=0b00111111){

}

else{

}

}

}

int get_node_sensor_value() //for node sensor values

{

ADCSRA|=(1<<ADSC);

while((ADCSRA &(1<<ADSC))!=0);

int a = ADCH;

if(a>threshold){

}

else{

}

}

void get_to_node() //travelling upto

{ //nodes after detecting

while(1){

return 0;

return 1;

int b = get_node_sensor_value();

if(b==0){

}

else{

}

}

}

void turn_right() //turning on nodes

{

int c = get_node_sensor_value();

int d = get_IR_sensor_value();

PORTC = right_zero;

while(d){

}

_delay_ms(10);

while (c==0){

d=get_IR_sensor_value();

c=get_node_sensor_value();

}

while(get_IR_sensor_value()==0);

PORTC = stall;

}

void turn_left()

{

int c = get_node_sensor_value();

int d = get_IR_sensor_value();

PORTC = left_zero;

while(d){

}

_delay_ms(10);

while (c==0){

}

while(get_IR_sensor_value()==0);

PORTC = stall;

}

void turn_around()

{

turn_right();

turn_right();

}

void change_dir(uint8_t v) //changing directions

{

int a = v - dir;

switch(a){

d=get_IR_sensor_value();

c = get_node_sensor_value();

case 1:

case -3:

turn_right();

break;

case -1:

case 3:

turn_left();

break;

case 2:

case -2:

turn_around();

break;

default:

break;

}

dir = v;

}

ISR(TIMER0_OVF_vect) //inerrupt on counter overflow

{

overflow++;

}

void sonar() //initialize sonar pins and triggering sonar

{

overflow = 0;

PORTC|=(1<<PC0);

_delay_us(50);

PORTC&=~(1<<PC0);

while(!(PINC&(1<<PC1)));

TCNT2 = 0;

while ((PINC&(1<<PC1))&&TCNT2<245);

print_integer(overflow);

counter = overflow*256 + TCNT2;

time = counter*(1024/16);

clear_display();

print_integer(counter);

_delay_ms(200);

}

void coordinate_update() //updating coordinates on reaching node

{

switch(dir)

{

case 0:

Y++;

break;

case 1:

X++;

break;

case 2:

Y--;

break;

case 3:

X--;

break;

}

print_string("X=");

print_integer(X);

print_string(",Y=");

print_integer(Y);

}

int obstacle_detection() //for detecting obstacles

{

sonar();

if (counter>80)

{return 0;

}

else

{

}

}

int main(void) //main

{

//motor_test();

return 1;

initialize();

init_LCD();

_delay_ms(1000);

clear_display();

sei(); //enabling global interrupts

while(1)

{

next_node();

clear_display();

print_string("NEXT NODE 1");

get_to_node();

clear_display();

print_string("REACHED NODE 1");

next_node();

clear_display();

print_string("NEXT NODE 2");

get_to_node();

clear_display();

print_string("REACHED NODE 2");

change_dir(1);

if (obstacle_detection()){

}

next_node();

clear_display();

print_string("NEXT NODE 3");

get_to_node();

clear_display();

print_string("REACHED NODE 3");

if (obstacle_detection()){

}

}

return 0;

}
