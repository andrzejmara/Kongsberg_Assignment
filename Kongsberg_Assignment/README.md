## Description
This project is an assignment made for Kongsberg.

This C# application simulates sensor data transmission and reception based on configurations provided in JSON files. 

The program then creates a message pool to store messages.
It creates instance of SensorSimulator and Receiver for each sensor and receiver defined in the configuration and starts them. 
The sensors populate the message pool with specifically classified messages, and receivers read them from the pool.
The program logs what sensors send and what receiver receive. 
It uses ANSI codes for colouring received messages based on the quality of the message.
The sensors and receivers work asynchronously to simulate real-life behaviour.

It keeps the program running for 30 seconds for demonstration purposes, after which it stops all simulators.

## Usage

To use this project, follow these steps:

1. Build and run this project using Visual Studio using .NET 8
2. Enjoy the simulation.

You can modify sensorConfig.json and receiverConfig.json to add/remove the sensors and receivers that will be simulated.
If you want to run .exe, please refer to the system configuration section of this README.

## System configuration
Before running the application without visual studio, 
it's important to enable Virtual Terminal support in your Windows environment.
This allows the application to properly display coloured output.

You can do this by running this command in console :

reg add HKCU\Console /v VirtualTerminalLevel /t REG_DWORD /d 1

or by following this process : 
1. Double-click the `enable-virtual-terminal.bat` file to run it.
3. Follow any prompts that appear to confirm the action.

## Important insight
I decided to use clean C# without using external libraries to present my proficiency with this language,
and showcase my ability to design and implement complicated systems.
I didn't use js/typescript because it doesn't have built-in concurrent data structures,
(simulating similar behaviour would take too much time) and I didn't want to use any external libraries.

Pros:
I believe it is the most efficient way of creating this system based on the needs of this project.
As a rough estimate, the solution should be suitable for applications with tens to hundreds of sensors and receivers, 
but scalability beyond that may require further analysis and potentially redesigning certain components for improved performance and efficiency.

Cons:
The messages are in the pool but are not retained, so the receiver may miss messages if it's down, or configured to read messages only sporadically.
The user experience is not the best, reading the messages in console may be hard for a human and if the use-case is to notify a human if receiver receives an alarm,
I would implement an additional system with a proper GUI for it.

If I would be hired by your company (and hopefully I will) and tasked with creating this system on a production environment
I would suggest few improvements described in next section.

## Possible Improvement
Use interfaces and dependecy injection, create a test project to test the functionality.
Document the code in a systemized way (//summary and so on) for the sake of future developers.
Add logs, trycatches to catch potential errors and a GUI.
Add a stronger typing for sensor properties (Type could be an enum for example).
Messages should not be just plain text, or configured in a way that it will be less error prone for receivers to analyze them.
Improve the ClassifyMessage method, so the upper warning upper alarm, lower warning, lower alarm could be updated independently.

Consider using
1. MQTT Message Broker
	Pros:
	1. MQTT is an efficient protocol designed for simple devices like sensors
	2. It's also async, so the messages don't need to wait for a response from receivers.
	3. MQTT brokers can handle a lot of concurrent connections, so the solution would scale much better
	4. MQTT brokers can retain messages for subscribers who are not currently connected, ensuring message delivery even if receivers are temporarily offline.
	Cons:
	1. You need to deploy and maintain MQTT brokers
	2. Single point of failure
	3. May be too complex for a small operation as an interview assignment.
	
2. RabbitMQ
3. Apache Kafka
4. AMQP (with Azure Service Bus or RabbitMQ) - maybe too complex for such a system. 
	