# Unity Demo for Game Development Competition 2022

_Other Languages: [简体中文](README.md)_

The demo app allows you to create the multiplayer online MiniGame connected in the same LAN obased on Instant Messaging, Audio & Video Call, Avatar Facial Expression, and spatial sound. You can use capabilities of specified products in the demo app.

## Project folder and core files  
Assets/
----Scenes/  
----Mirror/ Third-party network library Mirror
----Scripts/  
--------NetWork/ Network script  
--------RTC/ Audio and video call  
--------UI/ UI logic  
--------Animoji/ Digital avatar emojis  
--------IM/ IM messaging  
--------MiniGameBooter.cs Game start script  
--------GameConst.cs Configurations for audio and video calls and IM accounts  
--------GameEventDefine.cs Defines events in the games
--------GameManager.cs Game core class that Controls the game logic
--------UIManager.cs UI controller

## Run the demo project

### Development environment requirements

Before starting the demo project, make sure your development environment meets the following requirements:
- Unity 2020 or later
- Windows
- [Download RTCSDK (v4.5.907)]()
- [Download IMSDK (v2.4.1)](https://doc.yunxin.163.com/all/sdk-download?platform=unity)

### Prerequisites

- [Create a project and get `App Key`](https://doc.yunxin.163.com/nertc/docs/DE3NDM0NTI?platform=unity) 
- You have contacted CommsEase technical support, activated required services and signed up your IM account.

### Run the demo project

***Note:***  
* Unity Demo for Game Development Competition is used for demo purpose. You can use basic messaging, RTC streaming, spatial sound, and avatar facial expression transfer and refer to the source code for your own purpose.
* If you want to use the source code in the production environment, make sure that the application is thoroughly tested for compatibility and other issues before the application is officially released.

1. Clone the source code from the repository to a local directory of your project.
2. Open the project using Unity. Unity 2020.3.30f1 is used for the demo project.
3. Move the com.netease.game.rtc-4.5.907 file in the SDK package to the Packages directory.
4. Open the Package Manager of Unity Editor. Click the + icon in the top-left corner, click Add Package from tarball..., and select the com.netease.game.rtc-4.5.907 file in the Packages directory.
5. Double click UNITY_IM_SDK_2.4.1.unitypackage. （**Demo has already integrated IMSDK. You do not repeat the import operation**）
6. Open the scene file, Scenes/BattleScene.unity
7. Open Assets/Scripts/GameConst.cs and specify the AppKey of your project and IM login credentials
8. Click Run to try the demo app by clicking RUN.

## UI and features
### Select character UI
Demo offers four characters (created using ReadyPlayMe). You can create characters on https://readyplayer.me/.
![Create Join Game](/imgs/01.png)
### Create or join a game

*Make sure two machines are connected in the same lAN.*
1. Server + Client: The current machine serves as Host in Server + Client mode. Other players can join the game based on the IP address of the Host.
2. Join Server: Join a game based on the specified IP address.
3. ServerIP: Join a Host room based on the specified IP address.

![Create Join Game](/imgs/02.png)


### Game main UI
![Create Join Game](/imgs/03.png)

### IM chat UI
1. Click the mailbox button in the upper left corner to open the IM interface  
2. Enter text and start chatting

### Start a digital avatar and view the facial expression effects
1. Open your digital avatar by clicking Start Avatar
2. View the real avatar for your 3D digital character captured by the current camera.  
![Create Join Game](/imgs/04.png)

### Enable spatial sound effects
1. At least two participants have joined the room  
2. Two characters can walk and talk with each other in the game. The sound intensity and sound of the other character will vary with changing distances  


## Contact us

- [CommsEase Documentation](https://doc.yunxin.163.com/messaging/docs/home-page?platform=unity)
- [API Reference](https://doc.yunxin.163.com/all/api-refer)
- [Knowledge base](https://faq.yunxin.163.com/kb/main/#/)
- [Submit ticket](https://app.yunxin.163.com/index#/issue/submit)
