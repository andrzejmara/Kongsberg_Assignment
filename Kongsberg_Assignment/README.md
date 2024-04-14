## Usage

To use this project, follow these steps:

1. Build and run this project using Visual Studio using .NET 8
1. 

## System configuration
Before running the application without visual studio, 
it's important to enable Virtual Terminal support in your Windows environment.
This allows the application to properly display coloured output.

You can do this by running this command in console :

reg add HKCU\Console /v VirtualTerminalLevel /t REG_DWORD /d 1

or by following this process : 
1. Double-click the `enable-virtual-terminal.bat` file to run it.
3. Follow any prompts that appear to confirm the action.
