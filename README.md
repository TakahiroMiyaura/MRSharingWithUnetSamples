# MRSharingWithUnetSamples
This project is a sample code for SharingWithUNET in Mixed Reality Toolkit - Unity

## Development environment
* Visual Studio 2017
* Windows 10 Fall Creators Update(for immersive HMD)
* Unity 2017.2.1p2
* [Mixed Reality Toolkit - Unity 2017.2.1.1](https://github.com/Microsoft/MixedRealityToolkit-Unity/releases/tag/2017.2.1.1)

## 
* Open Unity.
  * Select Open.
  * Navigate to where you extracted the project files.
  * Click Select Folder.
* Import Mixed Reality Toolkit - Unity 2017.2.1.1
  * Select [Assets]-[Import Package]-[Custom Package].
  * Navigate to where you extracted the project files.
  * Click Select Folder.
* Check that Mixed Reality is enabled in Unity.
  * Select [Mixed Reality Toolkit]-[Configure]-[Apply Mixed Reality Project Settings]
  * Check that the following items are checked
    * Target Windowss Universal UWP
    * Enable XR
    * Build for Direct3D
    * Enable .NET scripting backend
  * Click Apply.
* Check UWP Capability Settings.
  * Check that the following items are checked
    * Internet Client Server
    * Private Network Client Server
* Open Scene.
  * Select [File]-[Open Scene].
  * Navigate to Assets\MRSharingWithUnetSamples\Scenes\MRSharingWithUnetSamples.unity
* Export To Visual Studio
  * Open the build menu (Control+Shift+B or File > Build Settings)
  * Click Add Open Scenes.
  * Check Unity C# Projects
  * Click Build.
  * In the file explorer window that appears, create a New Folder named App.
  * Single click the App folder.
  * Press Select Folder.
  * Wait for the build to complete
  * In the file explorer window that appears, navigate into the App folder.
  * Double-click MRSharingWithUnetSamples.sln to launch Visual Studio 
* Build From Visual Studio
  * Using the top toolbar change target to Release and x86.
  * Click the arrow next to Local Machine and select Device to deploy to HoloLens
  * Click the arrow next to Device and select Local Machine to deploy for the mixed reality headset.
  * Click Debug->Start Without Debugging or Control+F5 to start the application.
