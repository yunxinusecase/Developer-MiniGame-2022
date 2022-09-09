using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mini.Battle.Core;

namespace Mini.Battle.UI
{
    public class OwnTextureCellItem : TextureCellItem
    {
        public Button mBtnVideoToggle;
        public Text mTxtVideo;
        public Button mBtnAudioToggle;
        public Text mTxtAudio;
        public Button mBtnAvatarToggle;
        public Text mTxtAvatar;

        private bool mVideoStatus = true;
        private bool mAvatarStatus = false;
        private bool mAudioStatus = true;

        private string TAG = "OwnTextureCellItem";

        protected override void Awake()
        {
            mBtnVideoToggle.onClick.AddListener(OnClickVideo);
            mBtnAudioToggle.onClick.AddListener(OnClickAudio);
            mBtnAvatarToggle.onClick.AddListener(OnClickAvatar);
            ResetStatus();
        }

        private void ResetStatus()
        {
            mVideoStatus = true;
            mAvatarStatus = false;
            mAudioStatus = true;
            RefreshText(mVideoStatus, mTxtVideo, GameHeaderTips.Tips_CloseVideo, GameHeaderTips.Tips_OpenVideo);
            RefreshText(mAvatarStatus, mTxtAvatar, GameHeaderTips.Tips_DisableAvatar, GameHeaderTips.Tips_EnableAvatar);
            RefreshText(mAudioStatus, mTxtAudio, GameHeaderTips.Tips_CloseAudio, GameHeaderTips.Tips_OpenAudio);
        }

        public override void AddCell(ulong uid)
        {
            base.AddCell(uid);
            ResetStatus();
        }

        private void RefreshText(bool hason, Text input, string onText, string disableText)
        {
            input.text = hason ? onText : disableText;
        }

        private void OnClickVideo()
        {
            YXUnityLog.LogInfo(TAG, "OnClickVideo()");
            if (mVideoStatus)
            {
                GameManager.Instance.RTCLogic.StopOwnVideo();
            }
            else
            {
                GameManager.Instance.RTCLogic.StartOwnVideo();
            }
            mVideoStatus = !mVideoStatus;
            RefreshText(mVideoStatus, mTxtVideo, GameHeaderTips.Tips_CloseVideo, GameHeaderTips.Tips_OpenVideo);
        }

        private void OnClickAudio()
        {
            YXUnityLog.LogInfo(TAG, "OnClickAudio()");
            if (mAudioStatus)
            {
                GameManager.Instance.RTCLogic.StopOwnAudio();
            }
            else
            {
                GameManager.Instance.RTCLogic.StartOwnAudio();
            }
            mAudioStatus = !mAudioStatus;
            RefreshText(mAudioStatus, mTxtAudio, GameHeaderTips.Tips_CloseAudio, GameHeaderTips.Tips_OpenAudio);
        }

        private void OnClickAvatar()
        {
            YXUnityLog.LogInfo(TAG, "OnClickAvatar");
            if (mAvatarStatus)
                GameManager.Instance.RTCLogic.DisableAnimoji();
            else
                GameManager.Instance.RTCLogic.EnableAnimoji();
            mAvatarStatus = !mAvatarStatus;
            RefreshText(mAvatarStatus, mTxtAvatar, GameHeaderTips.Tips_DisableAvatar, GameHeaderTips.Tips_EnableAvatar);
        }
    }
}
