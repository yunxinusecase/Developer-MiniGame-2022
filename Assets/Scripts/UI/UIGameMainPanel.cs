using System.Collections;
using System.Collections.Generic;
using MetaVerse.FrameWork;
using UnityEngine;
using Mini.Battle.Core;
using StarterAssets;
using UnityEngine.UI;

namespace Mini.Battle.UI
{
    public class UIGameMainPanel : UIBasePanel
    {
        protected override string TAG
        {
            get { return "UIGameMainPanel"; }
        }

        public GameObject mMobileInputController;
        public Button mBackButton;
        public Button mBtnOpenIM;

        protected override void OnAwake()
        {
            base.OnAwake();
            mMobileInputController = Transform.FindObjectOfType<UICanvasControllerInput>(true).gameObject;
        }

        protected override void OnStart()
        {
            base.OnStart();
            mBackButton.onClick.AddListener(() =>
            {
                YXUnityLog.LogInfo(TAG, "click back button");
                GameManager.Instance.ExitGame();
            });

            mBtnOpenIM.onClick.AddListener(OnClickOpenIM);
        }

        protected override void OnShow(UIBaseData data)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            mMobileInputController.SetActive(false);
#else
            mMobileInputController.SetActive(true);

#endif
            UICanvasControllerInput input = mMobileInputController.GetComponent<UICanvasControllerInput>();
            input.SetPlayerInput(GameManager.Instance.mPlayerInputs);

#if ENABLE_INPUT_SYSTEM && (UNITY_IOS || UNITY_ANDROID)
            MobileDisableAutoSwitchControls script = mMobileInputController.GetComponent<MobileDisableAutoSwitchControls>();
            script?.SetPlayerInput(GameManager.Instance.mPlayerInput);
#endif
        }

        protected override void OnHide()
        {
            mMobileInputController.SetActive(false);
        }

        private void OnClickOpenIM()
        {
            YXUnityLog.LogInfo(TAG, "OnClickOpenIM");
            EventManager.TriggerEvent(GameEventDefine.Event_OpenIMPanel);
        }
    }
}
