using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NIMChatRoom;
using NIM;
using NimUtility;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using NIM.Signaling;
using static NIM.ClientAPI;
using Mini.Battle.Core;
using MetaVerse.FrameWork;
using NIM.Plugin;
using nertc;
using Newtonsoft.Json.Linq;

namespace Mini.Battle.IM
{
    public class AccountInfo : NimJsonObject<AccountInfo>
    {
        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        /// <value>The account.</value>
        [JsonProperty("account")]
        public string Account { get; set; }


        /// <summary>
        /// Gets or sets the passcode.
        /// </summary>
        /// <value>The passcode.</value>
        [JsonProperty("passcode")]
        public string Passcode { get; set; }
    }

    static class DataCenter
    {
        public static void RegisterCallback()
        {
            Channels = new List<NIMSignalingChannelDetailedinfo>();
            SignalingOfflineNotifications = new List<NIMSignalingNotityInfo>();
            NIM.Signaling.NIMSignalingAPI.RegMutilClientSyncNotifyCb(SignalingMultiClientSync);
            NIM.Signaling.NIMSignalingAPI.RegOfflineNotifyCb(SignalingOfflineNotify);
            NIM.Signaling.NIMSignalingAPI.RegChannelsSyncCb(SignalingChannelSync);
        }

        public static List<NIMSignalingChannelDetailedinfo> Channels { get; set; }

        public static List<NIMSignalingNotityInfo> SignalingOfflineNotifications { get; set; }

        public static NIMSignalingNotityInfo SignalingMultiClientNotification { get; set; }

        private static void SignalingChannelSync(List<NIMSignalingChannelDetailedinfo> channels)
        {
            if (channels != null && channels.Count > 0)
                Channels.AddRange(channels);
        }

        private static void SignalingOfflineNotify(List<NIMSignalingNotityInfo> notifys)
        {
            if (notifys != null && notifys.Count > 0)
                SignalingOfflineNotifications.AddRange(notifys);
        }

        private static void SignalingMultiClientSync(NIMSignalingNotityInfo notify_info)
        {
            SignalingMultiClientNotification = notify_info;
        }
    }

    public class IMLogic : SingletonMonoBehaviour<IMLogic>
    {
        private string TAG = "IMLogic";
        private string mAppDataDir;
        private AccountInfo mAccountInfo;

        KickoutResultHandler kickoutResultHandler;
        LogoutResultDelegate logoutResultDelegate;
        LoginResultDelegate loginResultDelegate;
        DndConfigureDelegate dndConfigureDelegate;
        MultiSpotLoginNotifyResultHandler multiSpotLoginNotifyResultHandler;
        KickOtherClientResultHandler kickOtherClientResultHandler;

        void Awake()
        {
            //all callbacks isn't called on the main thread.
            //If you want to update UI elements,you must schedule a task to the main thread.
            NIM.ClientAPI.RegDisconnectedCb(OnDisconnected);
            NIM.ClientAPI.RegAutoReloginCb(OnAutoRelogin);
            NIM.ClientAPI.RegMultiSpotLoginNotifyCb(OnMultiLogin);
            NIM.ClientAPI.RegKickoutCb(OnKickedoutCallback);

            ChatRoomApi.LoginHandler += OnLoginRoomHandler;
            ChatRoomApi.ExitHandler += OnExitRoomHandler;
            ChatRoomApi.LinkStateChanged += OnLinkStateRoomChanged;
            ChatRoomApi.ReceiveMessageHandler += OnReceiveMessageRoomHandler;
            ChatRoomApi.ReceiveNotificationHandler += OnReceiveNotificationRoomHandler;
            ChatRoomApi.SendMessageHandler += OnSendMessageRoomHandler;
            mIMClientInitSuc = false;
            mChatRoomInitSuc = false;
            mChatRoomJoinSuc = false;


            mAppDataDir = UnityEngine.Application.persistentDataPath;
            mAccountInfo = new AccountInfo();
            mAccountInfo.Account = GameConst.c_IMAccountName;
            mAccountInfo.Passcode = GameConst.c_IMPassCode;

            // Debug RoomID
            mDebugRoomInfo.RoomId = GameConst.c_IMRoomId;
            mDebugRoomInfo.Valid = 1;
            mDebugConfig.AppKey = GameConst.c_Appkey;
        }

        private void OnDisconnected()
        {
            YXUnityLog.LogInfo(TAG, "OnDisconnected");
        }

        private void OnAutoRelogin(NIMLoginResult result)
        {
            YXUnityLog.LogInfo(TAG, $"OnAutoRelogin result  {result.Serialize()}");
        }

        private void OnMultiLogin(NIMMultiSpotLoginNotifyResult result)
        {
            YXUnityLog.LogInfo(TAG, $"OnMultiLogin result : {result.Serialize()}");
            //result.OtherClients
            if (result.NotifyType == NIMMultiSpotNotifyType.kNIMMultiSpotNotifyTypeImIn)
            {
                //Your account had been logged in the other equipment.
            }
            else if (result.NotifyType == NIMMultiSpotNotifyType.kNIMMultiSpotNotifyTypeImOut)
            {
                //Your account had been logout from the other equipment.
            }
        }

        private void OnKickedoutCallback(NIMKickoutResult result)
        {
            YXUnityLog.LogInfo(TAG, ($"OnKickedoutCallback  ClientType - {result.ClientType}, KickReason - {result.KickReason}"));
        }

        private void OnLinkStateRoomChanged(long roomId, NIMChatRoomLinkCondition state)
        {
            YXUnityLog.LogInfo(TAG, $"OnLinkStateChanged roomId: {roomId}, state: {state.ToString()}");
            Dispatcher.QueueOnMainThread(() =>
            {
                // messageDeal.Reload(state);
            });
        }

        // 退出ChatRoom房间回调
        void OnExitRoomHandler(long roomId, NIM.ResponseCode errorCode, NIMChatRoomExitReason reason)
        {
            YXUnityLog.LogInfo(TAG, $"OnExitHandler roomid: {roomId}, errorCode: {errorCode}, reason: {reason}");
            mChatRoomJoinSuc = false;
        }

        // 加入ChatRoom房间回调
        void OnLoginRoomHandler(NIMChatRoomLoginStep loginStep, NIM.ResponseCode errorCode, ChatRoomInfo roomInfo, MemberInfo memberInfo)
        {
            Dispatcher.QueueOnMainThread(() =>
            {
                if (errorCode == ResponseCode.kNIMResSuccess && loginStep == NIMChatRoomLoginStep.kNIMChatRoomLoginStepRoomAuthOver)
                {
                    mChatRoomJoinSuc = true;
                    YXUnityLog.LogInfo(TAG, $"OnLoginHandler logicSuccess");
                }
                else
                {
                    YXUnityLog.LogInfo(TAG, $"OnLoginHandler logic fail errorCode: {errorCode}, loginStep: {loginStep}");
                    mChatRoomJoinSuc = false;
                }
            });
        }

        // IM接收到发送消息回调
        private void OnSendMessageRoomHandler(long roomId, ResponseCode code, Message message)
        {
            Dispatcher.QueueOnMainThread(() =>
            {
                if (message.MessageType == NIMChatRoomMsgType.kNIMChatRoomMsgTypeText)
                {
                    YXUnityLog.LogInfo(TAG, $"OnSendMessageHandler roomId: {roomId}, message: {message.MessageAttachment}");
                    EventManager.TriggerEvent<long, Message>(GameEventDefine.Event_OnIMSendMessage, roomId, message);
                }
            });
        }

        // IM接收到远端消息回调
        private void OnReceiveMessageRoomHandler(long roomId, Message message)
        {
            Dispatcher.QueueOnMainThread(() =>
            {
                if (message.MessageType == NIMChatRoomMsgType.kNIMChatRoomMsgTypeText)
                {
                    YXUnityLog.LogInfo(TAG, $"OnReceiveMessageHandler roomId: {roomId}, message: {message.MessageAttachment}");
                    EventManager.TriggerEvent<long, Message>(GameEventDefine.Event_OnIMRevMessage, roomId, message);
                }
            });
        }

        private void OnReceiveNotificationRoomHandler(long roomId, NIMChatRoom.Notification notification)
        {
            Dispatcher.QueueOnMainThread(() =>
            {
                YXUnityLog.LogInfo(TAG, $"OnReceiveNotificationHandler notification: {notification.ToString()}");
            });
        }

        // IM接口：初始化
        public void InitSDK()
        {
            if (!mIMClientInitSuc)
            {
                //create a NIMConfig object if need
                var config = new NimUtility.NimConfig();
                config.AppKey = GameConst.c_Appkey;
                config.CommonSetting = new NimUtility.SdkCommonSetting
                {
                    MaxLoginRetry = 3,//relogin times
                    PredownloadAttachmentThumbnail = true, //auto download the thumbnails for the image message
                    PreloadImageQuality = 100, // the quality of the image thumbnail
                    PreloadImageResize = "100x100", //the size of the image thumbnail
                    SyncSessionAck = true,
                    CustomTimeout = 15,// login timeout,seconds
                };

                bool ret = ClientAPI.Init(mAppDataDir, string.Empty, config);
                YXUnityLog.LogInfo(TAG, $"InitAndLoginIM init: {ret.ToString()}, inited: {NIM.ClientAPI.SdkInitialized}");
                if (NIM.ClientAPI.SdkInitialized)
                {
                    mIMClientInitSuc = true;
                    DataCenter.RegisterCallback();
                    ChatRoomApi.Init(mDebugConfig);
                    mChatRoomInitSuc = true;
                    // 使用账号密码登陆Client APP
                    NIM.ClientAPI.Login(GameConst.c_Appkey, mAccountInfo.Account, mAccountInfo.Passcode, HandleLoginClientResult);
                }
                else
                {
                    mIMClientInitSuc = false;
                }
            }
            else
            {
                if (!mChatRoomInitSuc)
                {
                    ChatRoomApi.Init(mDebugConfig);
                    mChatRoomInitSuc = true;
                }
            }
        }

        // IM接口：登陆RoomId操作
        public void LoginToRoomId()
        {
            if (mChatRoomInitSuc && mIMClientInitSuc)
            {
                if (!mChatRoomJoinSuc)
                {
                    YXUnityLog.LogInfo(TAG, $"Try Join Room Id: {mDebugRoomInfo.RoomId}");
                    ChatRoom.RequestLoginInfo(System.Convert.ToInt64(mDebugRoomInfo.RoomId), OnRequestRoomIdTokenCallback);
                }
                else
                {
                    YXUnityLog.LogInfo(TAG, $"Has Joined Room Id: {mDebugRoomInfo.RoomId} success");
                }
            }
            else
            {
                YXUnityLog.LogInfo(TAG, $"IM Not Valid clientInit: {mIMClientInitSuc}, roomInit: {mChatRoomInitSuc}");
            }
        }

        // IM接口：发送消息
        public void SendMessageToRoom(string txtMessage)
        {
            if (mChatRoomJoinSuc)
            {
                YXUnityLog.LogInfo(TAG, $"Try SendMessageToRoom txtMessage: {txtMessage}");
                mSendMessage.MessageAttachment = txtMessage;
                ChatRoomApi.SendMessage(System.Convert.ToInt64(mDebugRoomInfo.RoomId), mSendMessage);
            }
            else
            {
                YXUnityLog.LogInfo(TAG, "SendMessageToRoom Fail Client And Room Is Not Valid...");
            }
        }

        private void HandleLoginClientResult(NIM.NIMLoginResult result)
        {
            var text = string.Format("{0}:{1}", result.LoginStep, result.Code);
            if (result.LoginStep == NIM.NIMLoginStep.kNIMLoginStepLogin)
            {
                if (result.Code == NIM.ResponseCode.kNIMResSuccess)
                {
                    LoginToRoomId();
                }
            }
            YXUnityLog.LogInfo(TAG, $"HandleLoginResult: {result.Code.ToString()}, step: {result.LoginStep.ToString()}");
        }

        private Message mSendMessage = new Message();
        private NIMChatRoomConfig mDebugConfig = new NIMChatRoomConfig();
        private ChatRoomInfo mDebugRoomInfo = new ChatRoomInfo();
        private bool mIMClientInitSuc = false;
        private bool mChatRoomInitSuc = false;
        private bool mChatRoomJoinSuc = false;

        // 当前聊天室+房间是否有效


        private void OnRequestRoomIdTokenCallback(ResponseCode code, string result)
        {
            Dispatcher.QueueOnMainThread(() =>
            {
                mChatRoomJoinSuc = false;
                if (code != ResponseCode.kNIMResSuccess)
                {
                    YXUnityLog.LogError(TAG, $"OnRequestTokenCallback code: {code}, result: {result}");
                }
                else
                {
                    string token = result;
                    if (mChatRoomJoinSuc == false && !string.IsNullOrEmpty(token))
                    {
                        ChatRoomApi.Login(mDebugRoomInfo.RoomId, token);
                        mChatRoomJoinSuc = true;
                    }
                }
                YXUnityLog.LogInfo(TAG, $"OnRequestTokenCallback code: {code}, joinRoomSuc: {mChatRoomJoinSuc}");
            });
        }

        public void ReleaseIM()
        {
            YXUnityLog.LogInfo(TAG, "ReleaseIM");
            ChatRoomApi.Cleanup();
            ClientAPI.Cleanup();
        }

        // IM接口，退出房间
        public void ExitRoom()
        {
            // 退出房间
            mChatRoomJoinSuc = false;
            YXUnityLog.LogInfo(TAG, $"ExitRoom debug roomId: {mDebugRoomInfo.RoomId}");
            ChatRoomApi.Exit(mDebugRoomInfo.RoomId);
        }

        private void OnApplicationQuit()
        {
            ReleaseIM();
        }
    }
}