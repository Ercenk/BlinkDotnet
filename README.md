# BlinkDotnet
A .NET Core SDK implementation for Blink Security cameras based on the MattTW's unofficial protocol [documentation](https://github.com/MattTW/BlinkMonitorProtocol) (thanks MattTW!)

I implemented this simple library with minimal set of operations to support my custom security camera management project running on Azure. The library implements the following:

- Login
- Get Events
- Get networks
- Get all videos
- Download a video

Just instantiate a camera with your user name and password and call the methods...
```C#
  this.blinkCam = new BlinkCam(userName, password);
```


