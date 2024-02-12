# A simple Windows service installer project DEMO

The installer will create a Windows User ("DemoService-User")
Give the user Log on as a Service
Run Windows service with the user that was created during installation
Start service will simply write a log file with the start time
Stop service will log service stop time
When Uninstall the uninstaller will remove the user from Log on as a service properties. (Remove rights SeServiceLogonRight)
Cleanup installation directory.

Test: 
Install then go to (Local Security Policy -> Local Policies -> User Rights Assignment -> Log on as a service) properties
the "DemoService-User" will be in the list.
Uninstall then go to (Local Security Policy -> Local Policies -> User Rights Assignment -> Log on as a service) properties
the "DemoService-User" will be removed from the list.
