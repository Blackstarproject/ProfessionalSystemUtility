# ProfessionalSystemUtility: A password is required to access Task Manager, Regedit, and Task Scheduler
High-Level Overview
The ProfessionalSystemUtility is a Windows Forms application designed to run in the background and act as a security gatekeeper for certain critical system programs. It operates from the system tray (the area next to the clock) and continuously monitors for the launch of "protected" applications like the Task Manager (taskmgr.exe), Registry Editor (regedit.exe), and Microsoft Management Console (mmc.exe).

When a user attempts to open one of these protected applications, this utility immediately terminates the process and prompts the user with a password dialog. If the correct password is provided, the utility relaunches the application for the user. If the password is incorrect, access is denied. The application can be shut down by right-clicking its icon in the system tray and selecting "Exit".

Architectural Design and Components
The application follows a modern design pattern using Dependency Injection and Separation of Concerns. The main components are defined by interfaces, which makes the code modular, maintainable, and easier to test.

Here is a breakdown of each file and its role:

1. Entry Point and Application Context
Program.cs: This is the application's starting point. It configures and "wires up" all the necessary components (services) using dependency injection. Instead of running a visible form directly, it runs AppContextForm, which manages the application's lifecycle from the system tray.

AppContextForm.cs: This is the core of the application, acting as a hidden master controller.

It creates the NotifyIcon that appears in the system tray, providing an "Exit" option.

It subscribes to the ProtectedProcessDetected event from the ProcessMonitorService.

When a protected process is detected, it orchestrates the response: it calls the SystemService to terminate the process and then displays the authentication window (Form1).

It uses BeginInvoke to safely handle UI updates (like showing the form) from a background thread, which is a crucial practice for stability.

2. Monitoring and System Interaction
ProcessMonitorService.cs: This service is responsible for the background monitoring.

It uses a System.Threading.Timer to periodically (every 1000 milliseconds) scan all running processes on the system.

It compares the list of running processes against a hardcoded list of protected process names: taskmgr, regedit, and mmc.

If a match is found, it raises the ProtectedProcessDetected event, passing the name of the detected process.

It includes a _isChecking flag to prevent the timer callback from running again if a previous scan is still in progress (a re-entrancy guard).

SystemService.cs: This class handles direct interactions with system processes.

TerminateProcess(string processName): Finds all processes with the given name and forcefully kills them using process.Kill().

RelaunchProcess(string processName): Starts a new process with the given name. It contains special logic for mmc, ensuring that it launches the Task Scheduler (taskschd.msc) instead of a blank console.

3. Authentication and Security
AuthenticationService.cs: This service handles password validation.

It does not store the password in plaintext. Instead, it retrieves a pre-computed password hash and a "salt" from the configuration file.

The ValidatePassword method takes the user's input, hashes it using the same salt, and compares the resulting hash to the stored hash. If they match, the password is correct.

PasswordHelper.cs: This is a static utility class that provides the cryptographic functionality for hashing passwords.

It uses the PBKDF2 algorithm, implemented in .NET as Rfc2898DeriveBytes, which is a modern, industry-standard method for securely hashing passwords.

It employs strong parameters: a SHA256 hash algorithm, a key size of 256 bits, and 10,000 iterations. The high number of iterations makes it computationally expensive for an attacker to guess passwords (brute-force attack).

ConfigurationProvider.cs: This provides an abstraction for reading configuration settings. The AppConfigConfigurationProvider implementation reads settings like the password hash and salt from the App.config file.

4. User Interface
Form1.cs & Form1.Designer.cs: These files define the password prompt window that the user interacts with.

It provides a text box for the password, an "OK" button, and a "Show Password" checkbox.

When the "OK" button is clicked, it calls the ExecuteProtectedActionAsync method.

This method disables the UI to prevent further clicks, shows an "Authenticating..." status, and calls the AuthenticationService on a background thread (Task.Run) to avoid freezing the UI.

Based on the authentication result, it either closes the form successfully (allowing the process to be relaunched) or displays an "Incorrect Password" error message.

It updates a ToolStripStatusLabel at the bottom of the window to provide real-time feedback to the user (e.g., "Authenticating...", "Authentication successful.", "Authentication failed.").

Workflow Example
Startup: The application starts, creates all the service instances, and runs the AppContextForm, which immediately hides itself and shows only a system tray icon. The ProcessMonitorService begins its timer-based scanning.

Detection: A user double-clicks Task Manager.

Interception: Within a second, the ProcessMonitorService timer fires, finds taskmgr in the list of running processes, and raises the ProtectedProcessDetected event.

Termination: The AppContextForm catches the event and calls _systemService.TerminateProcess("taskmgr"). The Task Manager window abruptly closes.

Authentication Prompt: The AppContextForm then creates a new instance of Form1, sets its ProcessToRelaunch property to "taskmgr", and displays the form as a dialog box.

User Interaction: The user enters their password into Form1 and clicks "OK".

Verification: Form1 calls _authenticationService.ValidatePassword with the input. The service hashes the input using the configured salt and compares it to the stored hash.

Outcome:

Success: ValidatePassword returns true. Form1 calls _systemService.RelaunchProcess("taskmgr"), which starts the Task Manager again. The password form then closes.

Failure: ValidatePassword returns false. Form1 shows an error MessageBox, clears the password field, and waits for the user to try again.

Return to Monitoring: After the authentication form is closed, the application returns to its idle state, monitoring processes from the system tray.
