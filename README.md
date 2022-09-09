# 2022开发者大赛，音视频相关Unity Demo
_其他语言: [英文](README_EN.md)_

基于云信IM，RTC，虚拟人表情，空间音效能力，创建的局域网多人在线MiniGame，用户可以使用该Demo体验相关能力给相关产品赋能虚拟社交能力。

## 文件工程+核心文件介绍  
 Assets/       
----Scenes/  
----Mirror/ 第三方网络库Mirror  
----Scripts/  
--------NetWork/ 网络相关脚本  
--------RTC/ 音视频通话  
--------UI/ 界面逻辑  
--------Animoji/ 虚拟人表情  
--------IM/ IM消息  
--------MiniGameBooter.cs 游戏启动脚本  
--------GameConst.cs 定义音视频通话，IM相关账号配置  
--------GameEventDefine.cs 定义游戏中使用的事件    
--------GameManager.cs 游戏核心类，控制整个游戏流程    
--------UIManager.cs UI界面控制器    

## 跑通示例项目

### 开发环境要求

在开始运行示例项目之前，请确保开发环境满足以下要求：  
- Unity 2020及以上版本  
- Windows开发平台  
- [下载RTCSDK开发包(v4.5.907)]()
- [下载IMSDK开发包(v2.4.1)](https://doc.yunxin.163.com/all/sdk-download?platform=unity)

### 前提条件
- [已创建应用并获取`App Key`](https://doc.yunxin.163.com/nertc/docs/DE3NDM0NTI?platform=unity) 
- 已联系网易云信工作人员开通相关能力，并注册自己的IM 账号

### 运行示例项目

注意：  
* 开发者大赛Unity Demo仅供开发者接入参考，体验相关IM，RTC，3D空间音效，虚拟人表情迁移能力，请结合实际业务需求做修改参考。  
* 若您计划将源码用于生产环境，请确保应用正式上线前已经过全面测试，以免因兼容性等问题造成损失。

1. 克隆项目源码仓库到本地工程
2. 使用Unity打开工程，当前使用Unity版本为2020.3.30f1
3. 把下载到的 SDK 文件com.netease.game.rtc-4.5.907放到Packages目录。
4. 打开Unity Editor的Package Manager，单击左上角“+”图标，单击"Add Package from tarball..."，选中Packages目录下的com.netease.game.rtc-4.5.907文件，即可完成导入。
5. 点击运行UNITY_IM_SDK_2.4.1.unitypackage，import IMSDK开发包内容。（**Demo已经集成，无需重复Import**）
6. 打开场景文件Scenes/BattleScene.unity
7. 打开Assets/Scripts/GameConst.cs 文件填写APPKEY和IM登录信息
8. 点击运行即可体验Demo

## 相关界面和功能介绍
### 选择虚拟人角色界面
Demo提供了四个角色可控选择（角色使用的ReadyPlayMe平台创建）可以通过https://readyplayer.me/ 进行创建角色
![Create Join Game](/imgs/01.png)
### 创建或者加入游戏

*确保两个机器联网并且在同一个局域网络下面*
1. Server+Client ：当前机器作为Host（Server+Client）模式，其他玩家可以通过当前机器的ip地址加入到游戏中，当前机器作为Host  
2. Join Server ： 根据指定的ip地址加入一个游戏
3. ServerIP：根据填写的ip地址尝试加入一个Host房间

![Create Join Game](/imgs/02.png)


### 游戏主界面
![Create Join Game](/imgs/03.png)

### IM聊天界面 
1. 点击左上角的邮箱按钮打开IM界面  
2. 输入文字开始聊天  

### 如何打开虚拟人，查看角色的表情驱动效果
1. 点击打开虚拟人按钮，启动虚拟人  
2. 查看自己的3D虚拟人角色被当前摄像头采集的真实人脸驱动  
![Create Join Game](/imgs/04.png)

### 如何体验3D空间音效
1. 至少两个用户加入到房间  
2. 控制两个角色在场景中漫游于并说话，可以体验到随着距离变化，对方说话的声音强度和音色会发生变化  

## 联系我们

- [网易云信文档中心](https://doc.yunxin.163.com/nertc/docs/home-page?platform=unity)
- [API参考](https://doc.yunxin.163.com/all/api-refer)
- [知识库](https://faq.yunxin.163.com/kb/main/#/)
- [提交工单](https://app.yunxin.163.com/index#/issue/submit)

