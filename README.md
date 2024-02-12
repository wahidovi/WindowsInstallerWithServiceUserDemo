# A simple Windows service installer project DEMO

The installer will create a Windows User ("DemoService-User") <br>
Give the user Log on as a Service <br>
Run Windows service with the user that was created during installation <br>
Start service will simply write a log file with the start time <br>
Stop service will log service stop time <br>
When Uninstall the uninstaller will remove the user from Log on as a service properties. (Remove rights SeServiceLogonRight) <br>
Cleanup installation directory. <br>

Test: <br>
Install then go to (Local Security Policy -> Local Policies -> User Rights Assignment -> Log on as a service) properties <br>
the "DemoService-User" will be in the list. <br>
Uninstall then go to (Local Security Policy -> Local Policies -> User Rights Assignment -> Log on as a service) properties <br>
the "DemoService-User" will be removed from the list. <br>
