# HoloLens2-Unity-ResearchModeStreamer

Unity Plugin for accessing HoloLens2 Research Mode sensors and video camera, and streaming them to desktop. It builds upon the official [HoloLens2ForCV](https://github.com/microsoft/HoloLens2ForCV) and [HoloLensForCV](https://github.com/microsoft/HoloLensForCV) repos. 

Currently, only Depth AHAT and video camera streams are enabled, but an extension to other RM sensors should be straight forward and will probably be added in the future. The image, as well as ```rig2world``` transforms for AHAT and and ```pv2world```, ```fx``` and ```fy``` for video camera are transmitted for each frame.

## Using the Plugin
1. Open the [plugin solution](https://github.com/sysu17364029/Hololens2_detection/tree/main/HL2RmStreamUnityPlugin) in Visual Studio
2. Build the solution for ```Release, ARM64```.
3. In your Unity Project, create a folder ```Assets/Plugins/WSAPlayer/ARM64```.
4. Copy the  ```HL2RmStreamUnityPlugin.dll``` from ```HL2RmStreamUnityPlugin/ARM64/Release/HL2RmStreamUnityPlugin``` into the folder from step 3.
5. To call the ```StartStreaming``` function from the DLL, add this statement to one of your Unity scripts:
```
[DllImport("HL2RmStreamUnityPlugin", EntryPoint = "StartStreaming", CallingConvention = CallingConvention.StdCall)]
public static extern void StartStreaming();
``` 
6. You can call ```StartStreaming()``` from Unity. 
7. Before building the Unity Project, go to ```Build Settings -> Player Settings``` the following Capabilities are enabled: 
    *  InternetClient, InternetClientServer, PrivateNetworkClientServer, WebCam, SpatialPerception.
8. Build the Unity Project and open the solution in Visual Studio.
9. After building the Unity Project for the first time: Open the ```package.appxmanifest``` in the solution in a text editor to enable the research mode sensors:
    * add the rescap package to ```Package```:
      ```xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedcapabilities"```
    * add rescap to the Ignorable Namespaces:
      ```IgnorableNamespaces="... rescap"```
    * add rescap to ```Capabilities```:
      ```<rescap:Capability Name="perceptionSensorsExperimental" />```
    * save and close ```Package.appxmanifest```
10. Build solution for ```Release, ARM64``` and deploy to HoloLens2.

## Python Client
服务器作为检测结果传输的server [hololens2_detect.py](https://github.com/sysu17364029/Hololens2_detection/blob/main/hololens2_detect.py).

服务器作为检测结果传输的client [hdetect.py](https://github.com/sysu17364029/Hololens2_detection/blob/main/hdetect.py).

## Unity
接收检测结果并显示  [StartReceive.cs](https://github.com/sysu17364029/Hololens2_detection/blob/main/UnityHL2RmStreamer/Assets/Scripts/StartReceive.cs).
