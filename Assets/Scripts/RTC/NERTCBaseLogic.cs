using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mini.Battle.Core;
using nertc;
using System;
using System.Runtime.InteropServices;

namespace Mini.Battle.RTC
{
    public class NERTCBaseLogic : MonoBehaviour
    {
        public class VideoFrameCallback : IVideoFrameTextureCallback
        {
            private Action<ulong, Texture2D, RtcVideoRotation> _callback;

            public VideoFrameCallback(Action<ulong, Texture2D, RtcVideoRotation> callback)
            {
                _callback = callback;
            }

            public void OnVideoFrameCallback(ulong uid, Texture2D texture, RtcVideoRotation rotation)
            {
                _callback?.Invoke(uid, texture, rotation);
            }
        }

        private string _defaultLogPath;
        private IAudioDeviceManager AudioDeviceManager;
        private IVideoDeviceManager VideoDeviceManager;
        private RtcVideoCanvas defaultCanvas;
        protected ulong mOwnUserId = 0;

        public void SwitchCamera()
        {
            int ret = Engine?.SwitchCamera() ?? (int)RtcErrorCode.kNERtcErrFatal;
            YXUnityLog.LogInfo(TAG, $"SwitchCamera() ret: {ret.ToString()}");
        }

        private int _mainThreadId;

        public bool InMainThread(int threadId)
        {
            return threadId == _mainThreadId;
        }

        public IRtcEngine Engine { get; set; }

        protected bool mEngineStatus = false;

        public bool EngineStatus
        {
            get { return mEngineStatus; }
        }

        protected virtual void RegisterRTCEvent()
        {
            if (Engine == null)
                return;

            Engine.OnJoinChannel = onJoinChannel;
            Engine.OnConnectionStateChange = onConnectionStateChange;
            Engine.OnError = onError;
            Engine.OnFirstVideoDataReceived = onFirstVideoDataReceived;
            Engine.OnLeaveChannel = onLeaveChannel;
            Engine.OnUserVideoStart = onUserVideoStart;
            Engine.OnUserVideoStop = onUserVideoStop;
            Engine.OnUserVideoMute = onUserVideoMute;
            Engine.OnUserLeft = onUserLeft;
            Engine.OnUserJoined = onUserJoined;
            Engine.OnUserSubStreamVideoStart = onUserSubStreamVideoStart;
            Engine.OnUserSubStreamVideoStop = onUserSubStreamVideoStop;
            Engine.OnAddLiveStreamTask = onAddLiveStreamTask;
            Engine.OnRemoveLiveStreamTask = onRemoveLiveStreamTask;
            Engine.OnUpdateLiveStreamTask = onUpdateLiveStreamTask;
            Engine.OnLiveStreamStateChanged = onLiveStreamStateChanged;

            Engine.OnAvatarStatus = onAvatarStatus;
            Engine.OnAvatarUserJoined = onAvatarUserJoined;
            Engine.OnAvatarUserLeft = onAvatarUserLeft;
            Engine.OnRecvSEIMessage = onReceiveSEIMessage;

            Engine.OnUserAudioStart = onUserAudioStart;
            Engine.OnUserAudioStop = onUserAudioStop;
            Engine.OnUserAudioMute = onUserAudioMute;
        }

        public virtual int DoCreateNERTCSDK(RtcEngineContext context)
        {
            RegisterRTCEvent();
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!InMainThread(System.Threading.Thread.CurrentThread.ManagedThreadId))
            {
                AndroidJNI.AttachCurrentThread();
            }
            AndroidJavaClass playerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = playerClass.GetStatic<AndroidJavaObject>("currentActivity");
            context.context = activity;
#endif

            context.logPath = string.IsNullOrEmpty(context.logPath) ? _defaultLogPath : context.logPath;
            int ret = Engine.Initialize(context);

#if UNITY_ANDROID && !UNITY_EDITOR
            if (!InMainThread(System.Threading.Thread.CurrentThread.ManagedThreadId))
            {
                AndroidJNI.DetachCurrentThread();
            }
#endif
            if (ret != 0)
            {
                YXUnityLog.LogError(TAG, $"DoCreateNERTCSDK Engine Fatal Error ret: {ret.ToString()}");
                mEngineStatus = false;
                return ret;
            }

            mEngineStatus = true;
            // SetVideoConfig(24, 360, 640, RtcVideoProfileType.kNERtcVideoProfileHD720P);
            AudioDeviceManager = Engine.AudioDeviceManager;
            VideoDeviceManager = Engine.VideoDeviceManager;

            SetupLocalVideoCanvas();

            return ret;
        }

        private void SetupLocalVideoCanvas()
        {
            defaultCanvas = new RtcVideoCanvas();
            defaultCanvas.callback = new VideoFrameCallback(onTexture2DVideoFrame);
            Engine.SetupLocalVideoCanvas(defaultCanvas);
        }

        // 游戏APP退出，执行ReleaseEngine操作
        public virtual void ReleaseEngine()
        {
            YXUnityLog.LogInfo(TAG, "ReleaseEngine() start");
            mOwnUserId = 0;
            mEngineStatus = false;
            Engine?.LeaveChannel();
            Engine?.Release(true);
            YXUnityLog.LogInfo(TAG, "ReleaseEngine() end");
        }

        // 退出房间
        public virtual void LeaveChannel()
        {
            YXUnityLog.LogInfo(TAG, "LeaveChannel() start");
            mOwnUserId = 0;
            Engine?.LeaveChannel();
            YXUnityLog.LogInfo(TAG, "LeaveChannel() start");
        }

        protected virtual void OnApplicationQuit()
        {
        }

        void Awake()
        {
            _mainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            OnAwake();
        }

        protected string TAG => "NERTCBaseLogic";

        private static int mPushBufferSize = 0;
        private static IntPtr mPushBufferPtr = IntPtr.Zero;

        private static IntPtr GetBufferIntPtr(int needsize)
        {
            if (mPushBufferPtr == IntPtr.Zero)
                mPushBufferPtr = Marshal.AllocHGlobal(needsize);
            else if (mPushBufferSize != needsize)
                mPushBufferPtr = Marshal.ReAllocHGlobal(mPushBufferPtr, (IntPtr)needsize);
            mPushBufferSize = needsize;
            return mPushBufferPtr;
        }

        protected virtual bool IsValidPush
        {
            get
            {
                return EngineStatus;
            }
        }

        public void PushToRtcEngineVideoFrame(byte[] bytes, int width, int height)
        {
            if (!IsValidPush)
                return;
            if (bytes == null || width * height == 0)
                return;
            RtcExternalVideoFrame vf = new RtcExternalVideoFrame();
            vf.timestamp = 0;
            vf.width = (uint)width;
            vf.height = (uint)height;
            vf.format = RtcVideoType.kNERtcVideoTypeARGB;
            vf.rotation = 0;

            IntPtr unmanagedPointer = GetBufferIntPtr(bytes.Length);
            Marshal.Copy(bytes, 0, unmanagedPointer, bytes.Length);
            vf.buffer = unmanagedPointer;
            int ret = Engine.PushExternalVideoFrame(vf);
            if (ret != 0)
            {
                // Debug.Log($"PushToRtcEngineVideoFrame error ret: {ret.ToString()}");
            }
            else
            {

            }
        }

        public virtual void SetVideoConfig(int fps, int width, int height, RtcVideoProfileType type)
        {
            if (!EngineStatus)
            {
                YXUnityLog.LogError(TAG, "SetVideoCaptureConfig Error Engine Not Init");
                return;
            }

            YXUnityLog.LogInfo(TAG, $"SetVideoCaptureConfig fps: {fps}, width: {width}, height: {height}, type: {type}");
            RtcVideoConfig vc = new RtcVideoConfig();
            vc.maxProfile = type;
            vc.framerate = (RtcVideoFramerateType)fps;
            vc.bitrate = 0;
            vc.degradationPreference = RtcDegradationPreference.kNERtcDegradationDefault;
            vc.width = (uint)width;
            vc.height = (uint)height;
            // 推送的画面是否做一个镜像操作
            // vc.mirrorMode = RtcVideoMirrorMode.kNERtcVideoMirrorModeEnabled;
            int ret = Engine.SetVideoConfig(vc);
            Debug.Log($"SetVideoConfig ret: {ret.ToString()}");
        }

        protected virtual void OnAwake()
        {
            _defaultLogPath = Application.persistentDataPath;
            Engine = IRtcEngine.GetInstance();
        }

        void Update()
        {
            OnUpdate(Time.deltaTime);
        }

        protected virtual void OnUpdate(float ftime)
        {
        }

        public virtual bool StartRTCLogic(string appkey, string cname, ulong uid)
        {
            // start to join channel
            YXUnityLog.LogInfo(TAG, $"StartRTCLogic cname:{cname},uid:{uid},appkey:{appkey}");
            int ret = 0;
            if (!mEngineStatus)
            {
                RtcEngineContext context = new RtcEngineContext();
                context.appKey = appkey;
                context.logPath = _defaultLogPath;
                // 100 MB
                context.logFileMaxSize = 100;
                context.logLevel = RtcLogLevel.kNERtcLogLevelInfo;
                ret = DoCreateNERTCSDK(context);
                if (ret != 0)
                {
                    YXUnityLog.LogError(TAG, $"StartRTCLogic Create NERTC SDK Fatal Error ret:{ret.ToString()}");
                    return false;
                }
            }

            if (!mEngineStatus)
            {
                YXUnityLog.LogError(TAG, "StartRTCLogic Engine Status Is False");
                return false;
            }

            ret = Engine.EnableLocalAudio(true);
            YXUnityLog.LogInfo(TAG, $"EnableClientVirtualHuman true ret: {ret}");

            OnBeforeJoin();

            int joinret = Engine.JoinChannel(string.Empty, cname, uid);
            YXUnityLog.LogInfo(TAG, $"JoinChannel ret: {joinret}");
            if (joinret != 0)
                return false;
            else
                return true;
        }

        protected virtual void OnBeforeJoin()
        {
        }

        // 订阅远端视频流+设置画布
        protected void SubRemoteVideoAndSetUpCanvas(ulong uid)
        {
            var userCanvas = new RtcVideoCanvas();
            userCanvas.callback = new VideoFrameCallback(onTexture2DVideoFrame);
            int ret = Engine.SetupRemoteVideoCanvas(uid, userCanvas);
            ret = Engine.SubscribeRemoteVideoStream(uid, RtcRemoteVideoStreamType.kNERtcRemoteVideoStreamTypeHigh, true);
        }

        // 取消订阅远端视频流
        protected void UnSubscribeRemoteVideoStream(ulong uid)
        {
            Engine.SetupRemoteVideoCanvas(uid, null);
            Engine.SubscribeRemoteVideoStream(uid, RtcRemoteVideoStreamType.kNERtcRemoteVideoStreamTypeHigh, false);
        }

        // 订阅远端辅流+设置画布
        protected void SubRemoteSubStreamVideoAndSetUpCanvas(ulong uid)
        {
            var userCanvas = new RtcVideoCanvas();
            userCanvas.callback = new VideoFrameCallback(OnTexture2DSubVideoFrame);
            int ret = Engine.SetupRemoteSubstreamVideoCanvas(uid, userCanvas);
            ret = Engine.SubscribeRemoteVideoSubstream(uid, true);
        }

        // 取消订阅远端辅流
        protected void UnSubRemoteSubStreamVideo(ulong uid)
        {
            Engine.SetupRemoteSubstreamVideoCanvas(uid, null);
            Engine.SubscribeRemoteVideoSubstream(uid, false);
        }

        // local canvas callback
        public virtual void onTexture2DVideoFrame(ulong uid, Texture2D texture, RtcVideoRotation rotation)
        {
            YXUnityLog.LogInfo(TAG, $"OnTexture2DVideoFrame uid: {uid}");
        }

        // sub remote canvas callback
        public virtual void OnTexture2DSubVideoFrame(ulong uid, Texture2D texture, RtcVideoRotation rotation)
        {
            YXUnityLog.LogInfo(TAG, $"OnTexture2DSubVideoFrame uid: {uid}");
        }

        protected virtual void onJoinChannel(ulong cid, ulong uid, RtcErrorCode result, ulong elapsed)
        {
            YXUnityLog.LogInfo(TAG, $"onJoinChannel cid: {cid}, uid: {uid}, code: {result.ToString()}, elapsed: {elapsed.ToString()}");
            if (result == RtcErrorCode.kNERtcNoError)
            {
                YXUnityLog.LogInfo(TAG, "onJoinChannel success");
            }
            else
            {
                YXUnityLog.LogError(TAG, "onJoinChannel fail");
            }
        }

        protected virtual void onConnectionStateChange(RtcConnectionStateType state, RtcReasonConnectionChangedType reason)
        {
            YXUnityLog.LogInfo(TAG, $"onConnectionStateChange state:{state}, reason: {reason}");
        }

        // Own LeaveChannel
        // 自己离开房间
        protected virtual void onLeaveChannel(RtcErrorCode result)
        {
            YXUnityLog.LogInfo(TAG, $"onLeaveChannel result: {result}");
        }

        protected virtual void onUserJoined(ulong uid, string userName)
        {
            YXUnityLog.LogInfo(TAG, $"onUserJoined uid:{uid}, userName: {userName}");
        }

        protected virtual void onUserLeft(ulong uid, RtcSessionLeaveReason reason)
        {
            YXUnityLog.LogInfo(TAG, $"onUserLeft uid:{uid}, reason: {reason}");
        }

        protected virtual void onAvatarUserJoined(ulong srcUid, ulong uid, string userName)
        {
            YXUnityLog.LogInfo(TAG, $"onAvatarUserJoin srcUid: {srcUid}, uid: {uid}");
        }

        protected virtual void onAvatarUserLeft(ulong srcUid, ulong uid, RtcSessionLeaveReason reason)
        {
            YXUnityLog.LogInfo(TAG, $"onAvatarUserLeft srcUid: {srcUid}, uid: {uid}, reason: {reason.ToString()}");
        }

        public virtual void onAvatarStatus(bool enable, RtcErrorCode errorCode)
        {
        }

        protected virtual void onUserSubStreamVideoStart(ulong uid, RtcVideoProfileType maxProfile)
        {
            YXUnityLog.LogInfo(TAG, $"onUserSubStreamVideoStart uid:{uid}, maxProfile: {maxProfile}");
        }

        protected virtual void onUserSubStreamVideoStop(ulong uid)
        {
            YXUnityLog.LogInfo(TAG, $"onUserSubStreamVideoStop uid: {uid}");
        }

        protected virtual void onUserVideoMute(ulong uid, bool mute)
        {
            YXUnityLog.LogInfo(TAG, $"onUserVideoMute uid:{uid}, mute: {mute}");
        }

        protected virtual void onUserVideoStart(ulong uid, RtcVideoProfileType maxProfile)
        {
            YXUnityLog.LogInfo(TAG, $"OnUserVideoStart uid:{uid}, profile: {maxProfile}");
        }

        protected virtual void onUserVideoStop(ulong uid)
        {
            YXUnityLog.LogInfo(TAG, $"onUserVideoStop uid:{uid}");
        }

        protected virtual void onFirstVideoDataReceived(ulong uid)
        {
            YXUnityLog.LogInfo(TAG, $"onFirstVideoDataReceived uid: {uid}");
        }

        protected virtual void onError(int errorCode, string msg)
        {
            YXUnityLog.LogError(TAG, $"errorcode: {errorCode.ToString()}, msg: {msg}");
        }

        public virtual void onAddLiveStreamTask(string taskId, string url, int errorCode)
        {
            YXUnityLog.LogInfo(TAG, $"OnAddLiveStreamTask taskId: {taskId}, url: {url}, errorCode: {errorCode}");
        }

        public virtual void onUpdateLiveStreamTask(string taskId, string url, int errorCode)
        {
            YXUnityLog.LogInfo(TAG, $"OnUpdateLiveStreamTask taskId: {taskId}, url: {url}, errorCode: {errorCode}");
        }

        public virtual void onLiveStreamStateChanged(string taskId, string url, RtcLiveStreamStateCode state)
        {
            YXUnityLog.LogInfo(TAG, $"onLiveStreamStateChanged taskId: {taskId}, url: {url}, statecode: {state}");
        }

        public virtual void onRemoveLiveStreamTask(string taskId, int errorCode)
        {
            YXUnityLog.LogInfo(TAG, $"onRemoveLiveStreamTask taskId: {taskId}, errorCode: {errorCode}");
        }

        protected virtual void onUserAudioStart(ulong uid)
        {
            YXUnityLog.LogInfo(TAG, $"onUserAudioStart uid: {uid}");
        }

        protected virtual void onUserAudioStop(ulong uid)
        {
            YXUnityLog.LogInfo(TAG, $"onUserAudioStop uid: {uid}");
        }

        protected virtual void onUserAudioMute(ulong uid, bool mute)
        {
        }

        // 虚拟人表情数据返回接收
        protected virtual void onReceiveSEIMessage(ulong uid, byte[] data, uint dataSize)
        {
        }

        public void StopOwnVideo()
        {
            int ret = Engine?.EnableLocalVideo(false) ?? (int)RtcErrorCode.kNERtcErrFatal;
            YXUnityLog.LogInfo(TAG, $"StopOwnVideo ret: {ret}");
        }

        public void StartOwnVideo()
        {
            int ret = Engine?.EnableLocalVideo(true) ?? (int)RtcErrorCode.kNERtcErrFatal;
            YXUnityLog.LogInfo(TAG, $"StartOwnVideo ret: {ret}");
        }

        public void StartOwnAudio()
        {
            int ret = Engine?.EnableLocalAudio(true) ?? (int)RtcErrorCode.kNERtcErrFatal;
            YXUnityLog.LogInfo(TAG, $"StartOwnAudio ret: {ret}");
        }

        public void StopOwnAudio()
        {
            int ret = Engine?.EnableLocalAudio(false) ?? (int)RtcErrorCode.kNERtcErrFatal;
            YXUnityLog.LogInfo(TAG, $"StopOwnAudio ret: {ret}");
        }

        // 打开虚拟人开关，在云端做虚拟人表情迁移，本地接受SEI表情数据
        public void EnableAnimoji()
        {
            int ret = Engine?.EnableAvatar(true) ?? (int)RtcErrorCode.kNERtcErrFatal;
            YXUnityLog.LogInfo(TAG, $"EnableAnimoji ret: {ret}");
        }

        // 打开虚拟人开关，关闭云端虚拟人表情迁移
        public void DisableAnimoji()
        {
            int ret = Engine?.EnableAvatar(false) ?? (int)RtcErrorCode.kNERtcErrFatal;
            YXUnityLog.LogInfo(TAG, $"EnableAnimoji ret: {ret}");
        }
    }
}

