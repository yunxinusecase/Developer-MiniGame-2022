#if UNITY_ANDROID && !UNITY_EDITOR

using System;
using UnityEngine;

using System.Linq;
using NIM;
using NIM.Team;
using NIM.Session;
using NIM.User;


namespace NIM.Notification
{
    
    /// <summary>
    /// 通知提醒检测结果回调
    /// </summary>
    /// <param name="notifyEnabled"></param>
    public delegate void NotifyEnabledDelegate(bool notifyEnabled);
    /// <summary>
    /// 获取自定义的推送文案的委托
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public delegate string CustomPushContentDelegate(NIMReceivedMessage message);

    /// <summary>
    /// 获取自定义的推送标题的委托
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public delegate string CustomPushTitleDelegate(NIMReceivedMessage message);

    public static class NotificationManager
    {
        private static AndroidJavaClass _androidClass = null;   
        private static string FullClassName = "com.netease.nimlib.Controller";
        private static string MainActivityClassName = "com.netease.nimlib.MainActivity";//"com.unity3d.player.UnityPlayerActivity";
     
        static NotificationManager()
        {
            TalkAPI.SetSendLocalPushNotificationCallback(OnSendLocalPushNotificationCallback);
            _androidClass = new AndroidJavaClass(FullClassName);
        }
        /// <summary>
        /// 当前用户uid
        /// </summary>
        public static string  CurrentUser { get;set; }

        /// <summary>
        /// 设置本地push通知标识
        /// </summary>
        /// <param name="needPush">true:需要打开本地推送 ,否则为 false，默认值为false</param>
        private static bool _needLocalPushNotification = false;
        public static void  SetLocalPushNotificationFlag(bool needPush)
        {
            _needLocalPushNotification = needPush;
        }

        private static NotificationSetting _pushNotificationSetting = new NotificationSetting();
        /// <summary>
        /// global setting for push service
        /// </summary>
        /// <param name="setting"></param>
        public static void SetPushNotificationSetting(NotificationSetting setting)
        {
            _pushNotificationSetting = setting;
        }

        /// <summary>
        /// 为intent跳转设置 MainActivity,若设置不正确，会导致点击通知消息无法跳转到app. default to "com.unity3d.player.UnityPlayerActivity"
        /// 在该跳转页面获取点击状态栏回调获得数据 "Notification.CallbackData",如果使用或继承"com.netease.nimlib.MainActivity"内置的页面,"Notification.CallbackData"
        /// 将通过SetBind设置的对象和函数通知
        /// </summary>
        public static void SetMainActivity(string className)
        {
            if (!string.IsNullOrEmpty(className))
                MainActivityClassName = className;
        }


        /// <summary>
        /// 设置游戏对象和回调函数名，以接收点击跳转后收到的回调数据
        /// </summary>
        /// <param name="gameObjectName"></param>
        /// <param name="callbackName">函数形式为"public void func(string json)"</param>
        public static void SetBind(string gameObjectName, string callbackName)
        {
            _androidClass.CallStatic("setBind", gameObjectName, callbackName);
        }

        /// <summary>
        /// 获取自定义推送文案的委托回调
        /// </summary>
        static CustomPushContentDelegate _customPushContentDelegate = null;
        public static void SetCustomPushContentDelegate(CustomPushContentDelegate callback)
        {
            _customPushContentDelegate = callback;
        }

        /// <summary>
        /// 获取自定义推送标题的委托回调
        /// </summary>
        static CustomPushTitleDelegate _customPushTitleDelegate = null;
        public static void SetCustomPushTitleDelegate(CustomPushTitleDelegate callback)
        {
            _customPushTitleDelegate = callback;
        }


        //收到消息的回调处理本地推送
        private static void OnSendLocalPushNotificationCallback(NIMReceivedMessage receivedMsg)
        {
            if (_needLocalPushNotification)
            {
                CheckNotifyEnabled(receivedMsg,(notify)=>
                {
                    if (notify)
                    {
                        int badgeCount = SessionAPI.GetUnreadBadgeCount();
                        UnityEngine.AndroidJNI.AttachCurrentThread();
                        
                         if (IsBackground())
                        {
                            SendLocalPushNotify(receivedMsg);                            
                        }
                        SetBadgeCount(badgeCount);
                        UnityEngine.AndroidJNI.DetachCurrentThread();
                        
                    }

                });

                   
            }
        }

    //判断该会话消息是否需要通知
    private static void CheckNotifyEnabled(NIMReceivedMessage message, NotifyEnabledDelegate action)
    {
        var content = message.MessageContent;
        if (content.NeedPush && message.Feature == NIMMessageFeature.kNIMMessageFeatureDefault)
        {
            
            if (content.SessionType == NIMSessionType.kNIMSessionTypeP2P)
            {
                if (content.SenderID.Equals(CurrentUser))
                {
                    action(false);
                }
                else
                {
                    UserAPI.GetRelationshipList((code,list)=>{
                        bool muted = false;
                        if (code == ResponseCode.kNIMResSuccess && list != null)
                        {
                            foreach(var obj in list)
                            {
                                if (obj.AccountId.Equals(content.SenderID) && obj.IsMuted)
                                {
                                    muted = true;
                                }
                            }
                        }

                        action(!muted);

                    });

                }
                
            }
            else if (content.SessionType == NIMSessionType.kNIMSessionTypeTeam)
            {
                var memberInfo = TeamAPI.QuerySingleMemberInfo(content.ReceiverID, NotificationManager.CurrentUser);
                bool notify = memberInfo == null || memberInfo.NotifyNewMessage;
                action(notify);
            }
        }
       else
        {
            action(false);
        }
    }
        //获取通知文本内容
        private static string GetPushNotifyText(NIMReceivedMessage receivedMsg)
        {
            string text = null;
            string prefix = "";
            NIMIMMessage msg = receivedMsg.MessageContent;
            if (msg.NeedPushNick)
            {
                prefix = string.IsNullOrEmpty(msg.SenderNickname)?msg.SenderID:msg.SenderNickname;
            }
            switch(msg.MessageType)
            {
                case NIMMessageType.kNIMMessageTypeText:
                    NIMTextMessage message = (NIMTextMessage)msg;
                    text = message.TextContent;                
                    break;
                case NIMMessageType.kNIMMessageTypeAudio:
                    text = "[语音]";
                    break;
                case NIMMessageType.kNIMMessageTypeImage:
                    text = "[图片]";
                    break;
                case NIMMessageType.kNIMMessageTypeVideo:
                    text = "[视频]";
                    break;
                case NIMMessageType.kNIMMessageTypeLocation:
                    text = "[位置]";
                    break;
                case NIMMessageType.kNIMMessageTypeFile:
                    text = "[文件]";
                    break;
                case NIMMessageType.kNIMMessageTypeCustom:
                    text = "[自定义消息]";
                    break;
                case NIMMessageType.kNIMMessageTypeRobot:
                    text = "[机器人消息]";
                    break;
                case NIMMessageType.kNIMMessageTypeTips:
                    text = "[提示消息]";
                    break;
                case NIMMessageType.kNIMMessageTypeNotification:
                    text = "你有一个系统消息";
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(msg.PushContent))
                text = msg.PushContent;

            string notifyText = null;
            if (!string.IsNullOrEmpty(text))
            {
                notifyText = string.IsNullOrEmpty(prefix) ? text : prefix + " : " + text;
            }

            return notifyText;
        }
        //生成随机通知id
        private static int generateRandom()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            int iSeed = BitConverter.ToInt32(buffer, 0);
            System.Random random = new System.Random(iSeed);
            return random.Next(0, int.MaxValue);
        }
        //发送消息通知
        private static  int SendLocalPushNotify(NIMReceivedMessage msg)
        {
            if (msg == null || msg.MessageContent == null)
            {
                return -1;
            }
            string nofity = GetPushNotifyText(msg);
            //获取自定义推送文案
            if (_customPushContentDelegate != null)
            {
                string customNotify = _customPushContentDelegate(msg);
                if (customNotify != null)
                {
                    nofity = customNotify;
                }
            }

            string title = "";
            //获取自定义推送标题
            if (_customPushTitleDelegate != null)
            {
                string customTitle = _customPushTitleDelegate(msg);
                if (customTitle != null)
                {
                    title = customTitle;
                }
            }

            if (string.IsNullOrEmpty(nofity))
                return -1;

            NotificationParams notificationParams = new NotificationParams
            {
                Id = generateRandom(),
                Delay = new TimeSpan(0, 0, 0, 0, 10),
                Title = title,
                Message = nofity,
                Ticker = nofity,
                Sound = _pushNotificationSetting.Sound,
                Vibrate = _pushNotificationSetting.Vibrate,
                Light = _pushNotificationSetting.Light,
                SmallIcon = _pushNotificationSetting.SmallIcon,
                SmallIconColor = _pushNotificationSetting.SmallIconColor,
                LargeIcon = _pushNotificationSetting.LargeIcon,
                extraData = msg.MessageContent.Serialize(),
            };
          
            return NotificationManager.SendNotification(notificationParams);
        }

        /// <summary>
        /// 发送简单通知消息.
        /// </summary>
        /// <param name="smallIcon">List of build-in small icons: notification_icon_bell (default), notification_icon_clock, notification_icon_heart, notification_icon_message, notification_icon_nut, notification_icon_star, notification_icon_warning.</param>
        public static int Send(TimeSpan delay, string title, string message, Color smallIconColor, NotificationIcon smallIcon = 0)
        {
            return SendNotification(new NotificationParams
            {
                Id = UnityEngine.Random.Range(0, int.MaxValue),
                Delay = delay,
                Title = title,
                Message = message,
                Ticker = message,
                Sound = true,
                Vibrate = true,
                Light = true,
                SmallIcon = smallIcon,
                SmallIconColor = smallIconColor,
                LargeIcon = ""
            });
        }

        /// <summary>
        /// 发送普通通知消息.
        /// </summary>
        /// <param name="smallIcon">List of build-in small icons: notification_icon_bell (default), notification_icon_clock, notification_icon_heart, notification_icon_message, notification_icon_nut, notification_icon_star, notification_icon_warning.</param>
        public static int SendWithAppIcon(TimeSpan delay, string title, string message, Color smallIconColor, NotificationIcon smallIcon = 0)
        {
            return SendNotification(new NotificationParams
            {
                Id = UnityEngine.Random.Range(0, int.MaxValue),
                Delay = delay,
                Title = title,
                Message = message,
                Ticker = message,
                Sound = true,
                Vibrate = true,
                Light = true,
                SmallIcon = smallIcon,
                SmallIconColor = smallIconColor,
                LargeIcon = "app_icon"
            });
        }

        /// <summary>
        /// 发送自定义通知.
        /// </summary>
        public static int SendNotification(NotificationParams notificationParams)
        {
            if (notificationParams == null){
                return -1;
            }
            // _androidClass.CallStatic("SetNotification", p.Id, delay, p.Title, p.Message, p.Ticker,
            //     p.Sound ? 1 : 0, p.Vibrate ? 1 : 0, p.Light ? 1 : 0, p.LargeIcon, GetSmallIconName(p.SmallIcon), ColorToInt(p.SmallIconColor),p.extraData, MainActivityClassName);

            string notification = notificationParams.Serialize();


            _androidClass.CallStatic("SendNotification", notification, MainActivityClassName);
            return notificationParams.Id;
        }

        /// <summary>
        /// 设置app角标，小于或等于0时 将角标计数清空
        /// </summary>
        /// <param name="badgeCount"></param>
        public static void SetBadgeCount(int badgeCount)
        {
            _androidClass.CallStatic("setBadgeCount",badgeCount);
        }
        
        /// <summary>
        /// 判断app是否在后台
        /// </summary>
        public static bool IsBackground()
        {
            return _androidClass.CallStatic<bool>("isBackground");
        }


        /// <summary>
        /// 取消通知.
        /// </summary>
        public static void Cancel(int id)
        {
            _androidClass.CallStatic("CancelScheduledNotification", id);
        }

        /// <summary>
        /// 取消所有通知消息
        /// </summary>
        public static void CancelAll()
        {
            _androidClass.CallStatic("CancelAllNotifications");
        }
        
        private static int ColorToInt(Color color)
        {
            var smallIconColor = (Color32) color;
            
            return smallIconColor.r * 65536 + smallIconColor.g * 256 + smallIconColor.b;
        }
        //获取在资源包中的smallIcon名称
        private static string GetSmallIconName(NotificationIcon icon)
        {
            return "anp_" + icon.ToString().ToLower();
        }
    }
}


#endif
