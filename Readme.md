# PSKReporter DX Cluster Service

## Overview

This is a local DX cluster server that receives
[PSK Reporter](https://www.pskreporter.info/)
spots from the
[MQTT feed](http://samburns.uk/)
using the
[MQTT protocol](https://www.hivemq.com/blog/mqtt-essentials-wrap-up/),
and serves them in the classical DX cluster format understood by many
loggers, cluster monitors and other Ham programs.

You can either run the server as a console application by double-clicking on the exe file,
or install as a
[Windows service](https://stackify.com/what-are-windows-services/). In the latter case, the
server may be configured to start on system startup, even before the user logs in, and
run silently while Windows is running.

The server performs two tasks, it serves the clients with the spots and archives the spots
in a local folder.  Both functions may be enabled or disabled in the settings file as described below.

## Settings

The settings file, `Settings.json`, is created after the first run of the server and looks like this:

```json
{
  "TelnetPort": 7309,
  "TelnetServerEnabled": true,
  "Mqtt":
  {
    "ArchiveSpots": true,
    "ArchiveFolder": "C:\\PskrDxClusterService\\SpotArchive",
    "Host": "mqtt.pskreporter.info",
    "Port": 1883,
    "Topics": [
        "pskr/filter/v2/6m/+/+/+/+/+/1/+", 
        "pskr/filter/v2/6m/+/+/+/+/+/+/1", 
        "pskr/filter/v2/6m/+/+/+/+/+/291/+", 
        "pskr/filter/v2/6m/+/+/+/+/+/+/291"]
  }
}
```

Edit this file in a text editor to change the settings.

## Connecting to the DX Cluster

Use this address in your Ham software to connect to the DX cluster server:

`localhost:7309`

If some other program is already listening on the TCP port 7309, choose another port number and enter it in the settings file.

To test the DX cluster server, you can connect to it using the
[telnet.exe](https://social.technet.microsoft.com/wiki/contents/articles/38433.windows-10-enabling-telnet-client.aspx)
program that comes with Windows, and enter this command:

`open localhost 7309`

Enter your callsign when prompted, and watch the spot feed. Type "`BYE`" to disconnect from the server.

## Installing the Windows Service

To install the program as a Windows service, right-click on the `InstallService.bat` file that comes with the program, and click on `Run as Administrator` in the popup menu.

Use the `UninstallService.bat` file to uninstall the windows service.

Once the service is installed, press `Ctrl-Esc` and type `Services` to open the Services window, select your service and click on the Start button. To enable auto-start of the service, double-click on it and select `Automatic (Delayed Start)` in the Startup Type box.
