<img src="Sources/Stylophone/Assets/Square44x44Logo.targetsize-256.png" width="128">
  
Stylophone
===========

[**Music Player Daemon**](https://www.musicpd.org/) Client for UWP.
Based on [MpcNET](https://github.com/petrkr/MpcNET), the original .NET Client Library for MPD.  

<a href='//www.microsoft.com/store/apps/9NCB693428T8?cid=storebadge&ocid=badge'><img src='https://developer.microsoft.com/en-us/store/badges/images/English_get-it-from-MS.png' alt='English badge' width="142" height="52"/></a>

[Buy a sticker if you want!](https://ko-fi.com/s/9fcf421b6e)  

## Features

* Full playback control  
* Playlist management (Create, Add/Remove tracks, Delete)  
* Picture-in-picture mode  
* Live tile  
* Browse library by albums, or directly by folders  
* All data is pulled from your MPD Server only  
* Support for both albumart and readpicture commands for maximum compatibility with your cover art library

## Usage with a locally hosted MPD server

If your MPD server is locally hosted, you're probably running into the issue where UWP apps can't access `localhost`.  
(See https://stackoverflow.com/questions/33259763/uwp-enable-local-network-loopback/33263253#33263253 for a summary.)
There is a workaround you can use with checknetisolation which should work:  

```
checknetisolation loopbackexempt -a -n="13459Difegue.Stylophone_zd7bwy3j4yjfy"
```  

## Translation

You can easily contribute translations to Stylophone! To help translate, follow these instructions.

### Adding a new language (requires Visual Studio 2019)
- Create a new issue with the subject `[Translation] fr-CA` where you replace `fr-CA` with whatever language-region code you'll be translating into.
    - If an issue already exists, then don't do this step.
- Fork and clone this repo
- Open in VS 2019
- In the `Stylophone.Localization` project, find the `Strings` folder.
- Create a new file inside `Strings` that looks like this: `Resources.en-US.resx` but using the language you're translating into.
- Copy all the existing data from `Resources.en-US.resx` into your new `Resources.[language].resx`
- Translate the strings from english to your language
- Once done, then commit > push > create pull request!

### Improving an existing language (can be done with any text editor)
- Fork and clone this repo
- Open the `.resx` file (e.g. `Resources.en-US.resx`) you want to edit. Choose any text editor
- Translate
- Commit > push > create pull request!

## Screenshots

![Screen1](Screenshots/Screen1.jpg)
![Screen2](Screenshots/Screen2.jpg)
![Screen3](Screenshots/Screen3.jpg)
![Screen4](Screenshots/Screen4.jpg)
![Screen5](Screenshots/Screen5.jpg)
![Screen6](Screenshots/Screen6.jpg)

## Privacy Policy

Stylophone collects no data from your computer by default.  
The Windows Store version can send anonymized error reports related to crashes of the application back to me.  

If you enable Telemetry in the app's settings, the application will send detailed crash reports using App Center.  
Those reports can contain information about your hardware. (Motherboard type, etc)  
