using System.Collections;
using System.Collections.Generic;
using nertc;
using UnityEngine;
using Mini.Battle.UI;
using Mini.Battle.Core;
using System.Text;
using MetaVerse.Animoji;
using System;

namespace Mini.Battle.RTC
{
    public class RTCMainLogic : NERTCBaseLogic
    {
        // 加入房间回调
        // Space Sound 3D音效效果
        private SpaceSoundController mSpaceSoundController;

        protected override void OnAwake()
        {
            base.OnAwake();
            mSpaceSoundController = this.gameObject.GetOrAddComponent<SpaceSoundController>();
            Debug.Assert(mSpaceSoundController != null);
        }

        protected override void OnUpdate(float ftime)
        {
            mSpaceSoundController?.Update3DSpaceSoundTransform();
        }

        protected override void OnBeforeJoin()
        {
            mSpaceSoundController?.Enable3DAudio();
        }

        public void Set3DSpaceSoundTarget(Transform target)
        {
            YXUnityLog.LogInfo(TAG, "Set3DSpaceSoundTarget target");
            if (mSpaceSoundController != null)
                mSpaceSoundController.mTargetTransform = target;
        }

        protected override void onJoinChannel(ulong cid, ulong uid, RtcErrorCode result, ulong elapsed)
        {
            YXUnityLog.LogInfo(TAG, $"onJoinChannel cid: {cid}, uid: {uid}, code: {result.ToString()}, elapsed: {elapsed.ToString()}");
            mOwnUserId = uid;
            if (result == RtcErrorCode.kNERtcNoError)
            {
                YXUnityLog.LogInfo(TAG, "onJoinChannel Success");
                // 加入房间成功
                // 打开摄像头
                StartOwnVideo();
            }
            else
            {
                YXUnityLog.LogError(TAG, "onJoinChannel Fail Please Retry");
            }

            Dispatcher.QueueOnMainThread(() =>
            {
                OnJoinChannelResultOnMainThread(result == RtcErrorCode.kNERtcNoError);
            });
        }

        private void OnJoinChannelResultOnMainThread(bool suc)
        {
            if (suc)
            {
                UIManager.Instance.ShowUI(UIPanelType.RTCAvatar, null);

                UIGameRTCPanel panel = UIManager.Instance.GetUIPanel<UIGameRTCPanel>(UIPanelType.RTCAvatar);
                panel.OnUserJoined(0);
            }
            else
            {
                UIManager.Instance.HideUI(UIPanelType.RTCAvatar);
            }
        }

        // 有用户加入到房间
        protected override void onUserJoined(ulong uid, string userName)
        {
            YXUnityLog.LogInfo(TAG, $"onUserJoined uid:{uid}, userName: {userName}");

            Dispatcher.QueueOnMainThread(() =>
            {
                OnUserJoinedMainThread(uid);
            });
        }

        protected override void onUserVideoStart(ulong uid, RtcVideoProfileType maxProfile)
        {
            YXUnityLog.LogInfo(TAG, $"onUserVideoStart uid:{uid}, profile: {maxProfile}");
            Dispatcher.QueueOnMainThread(() =>
            {
                OnUserVideoStartInMainThread(uid);
            });
        }

        protected override void onUserVideoStop(ulong uid)
        {
            YXUnityLog.LogInfo(TAG, $"onUserVideoStop uid:{uid}");
            Dispatcher.QueueOnMainThread(() =>
            {
                OnUserVideoStopInMainThread(uid);
            });
        }

        protected override void onUserLeft(ulong uid, RtcSessionLeaveReason reason)
        {
            Dispatcher.QueueOnMainThread(() =>
            {
                OnUserLeftInMainThread(uid, reason);
            });
        }

        // 代理虚拟人加入房间，虚拟人被开启
        protected override void onAvatarUserJoined(ulong srcUid, ulong uid, string userName)
        {
            Dispatcher.QueueOnMainThread(() =>
            {
                YXUnityLog.LogInfo(TAG, $"onAvatarUserJoined srcUid: {srcUid}, uid: {uid}, userName: {userName}");
            });
        }

        // 代理虚拟人离开房间，虚拟人被关闭
        protected override void onAvatarUserLeft(ulong srcUid, ulong uid, RtcSessionLeaveReason reason)
        {
            Dispatcher.QueueOnMainThread(() =>
            {
                YXUnityLog.LogInfo(TAG, $"onAvatarUserLeft srcUid: {srcUid}, uid: {uid}, reason: {reason.ToString()}");
            });
        }

        public override void onAvatarStatus(bool enable, RtcErrorCode errorCode)
        {
            YXUnityLog.LogInfo(TAG, $"onAvatarStatus enable: {enable}, code: {errorCode}");
        }

        // Own user leave channel
        protected override void onLeaveChannel(RtcErrorCode result)
        {
            YXUnityLog.LogInfo(TAG, $"onLeaveChannel result: {result.ToString()}");
            Dispatcher.QueueOnMainThread(() =>
            {
                OnLeaveChanelMainThread();
            });
        }

        // 当前用户离开音视频，清空相关UI操作
        private void OnLeaveChanelMainThread()
        {
            UIGameRTCPanel panel = UIManager.Instance.GetUIPanel<UIGameRTCPanel>(UIPanelType.RTCAvatar);
            panel.OnUserLeft(0);
        }

        public override void onTexture2DVideoFrame(ulong uid, Texture2D texture, RtcVideoRotation rotation)
        {
            UIGameRTCPanel panel = UIManager.Instance.GetUIPanel<UIGameRTCPanel>(UIPanelType.RTCAvatar);
            panel.OnTextureCallBack(uid, texture, (int)rotation);
        }

        protected override void onReceiveSEIMessage(ulong uid, byte[] data, uint dataSize)
        {
            //YXUnityLog.LogInfo(TAG, $"sei uid: {uid}, datasize: {dataSize}");
            if (dataSize != 0)
            {
                string str = Encoding.ASCII.GetString(data, 0, (int)dataSize);
                //Debug.Log("content:" + str);
                string[] strarr = str.Split(new char[] { ',', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                float[] bboxArrary = new float[4];
                float[] size = new float[2];
                float[] farr = new float[MetaVerse.Animoji.AnimojiConst.c_nativeExpressionSize];

                int index = 0;
                for (; index < strarr.Length; index++)
                {
                    float value = float.Parse(strarr[index]);
                    if (index >= 0 && index <= 1)
                        size[index] = value;
                    else if (index >= 2 && index <= 5)
                        bboxArrary[index - 2] = value;
                    else
                        farr[index - MetaVerse.Animoji.AnimojiConst.c_bboxSize - 2] = value;
                }
                Dispatcher.QueueOnMainThread(() =>
                {
                    ulong targetuid = uid == 0 ? mOwnUserId : uid;
                    NetPlayerController npc = GameManager.Instance.GetNetPlayerController(targetuid);
                    npc?.PushFaceData(farr, bboxArrary, size[0], size[1]);
                });
            }
            else
            {
                Dispatcher.QueueOnMainThread(() =>
                {
                    ulong targetuid = uid == 0 ? mOwnUserId : uid;
                    NetPlayerController npc = GameManager.Instance.GetNetPlayerController(uid);
                    npc.PushFaceData(null, null, 5, 5);
                });
                // BOOT.Instance.PushFaceData(null, null, 5, 5);
            }

            //Dispatcher.QueueOnMainThread(() =>
            //{
            //});
        }

        // MainThread Function

        private void OnUserJoinedMainThread(ulong uid)
        {
            UIGameRTCPanel panel = UIManager.Instance.GetUIPanel<UIGameRTCPanel>(UIPanelType.RTCAvatar);
            panel.OnUserJoined(uid);
        }

        private void OnUserLeftInMainThread(ulong uid, RtcSessionLeaveReason reason)
        {
            UIGameRTCPanel panel = UIManager.Instance.GetUIPanel<UIGameRTCPanel>(UIPanelType.RTCAvatar);
            panel.OnUserLeft(uid);
        }

        private void OnUserVideoStartInMainThread(ulong uid)
        {
            // 订阅视频流
            SubRemoteVideoAndSetUpCanvas(uid);
        }

        private void OnUserVideoStopInMainThread(ulong uid)
        {
            // 停止订阅视频流
            UnSubscribeRemoteVideoStream(uid);
        }

        protected override void OnApplicationQuit()
        {
            YXUnityLog.LogInfo(TAG, "OnApplicationQuit Try to Relese RTC Engine");
            ReleaseEngine();
        }

        public override void ReleaseEngine()
        {
            Set3DSpaceSoundTarget(null);
            base.ReleaseEngine();
        }

        public override void LeaveChannel()
        {
            YXUnityLog.LogInfo(TAG, "LeaveChannel() start");
            UIGameRTCPanel panel = UIManager.Instance.GetUIPanel<UIGameRTCPanel>(UIPanelType.RTCAvatar);
            panel.Release();
            Set3DSpaceSoundTarget(null);
            mOwnUserId = 0;
            Engine?.LeaveChannel();
            YXUnityLog.LogInfo(TAG, "LeaveChannel() start");
        }
    }
}
