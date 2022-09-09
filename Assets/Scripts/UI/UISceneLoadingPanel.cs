using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mini.Battle.Core;
using System;

namespace Mini.Battle.UI
{
    public class UISceneLoadingPanel : UIBasePanel
    {
        protected override string TAG
        {
            get { return "UISceneLoadingPanel"; }
        }

        private bool mLoadingDone = false;
        private float mLoadingTime = 0.5f;
        private float mDefaultMiniLoadingTime = 1.0f;

        private Action mLoadingCallBack = null;

        protected override void OnAwake()
        {
        }

        public void SetLoadingDone()
        {
            mLoadingDone = true;
        }

        protected override void OnShow(UIBaseData data)
        {
            mLoadingCallBack = null;
            mLoadingDone = false;
            UISceneLoadingData loadingData = data as UISceneLoadingData;
            if (loadingData != null)
            {
                mLoadingTime = loadingData.minTime;
                mLoadingCallBack = loadingData.callBack;
                mLoadingDone = loadingData.done;
            }
            else
            {
                mLoadingTime = mDefaultMiniLoadingTime;
            }
            this.gameObject.SetActive(true);
        }

        protected override void OnUpdate(float ftime)
        {
            mLoadingTime -= ftime;
            if (mLoadingTime <= 0f && mLoadingDone)
            {
                YXUnityLog.LogInfo(TAG, "end loading");
                mLoadingCallBack?.Invoke();
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
            mLoadingCallBack = null;
            mLoadingTime = mDefaultMiniLoadingTime;
        }
    }
}
