using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MetaVerse.FrameWork;
using Mini.Battle.UI;
using System;
using UnityEngine.InputSystem;

namespace Mini.Battle.Core
{
    public enum UIPanelType
    {
        None = -1,
        // 选择角色界面
        SelectHero,
        // 开启游戏界面
        GamePrepare,
        // 过场Loading界面
        GameSceneLoading,
        // 游戏主界面
        GameMain,
        // IM界面
        IM,
        // RTC通话界面
        RTCAvatar,
    }

    // 打开界面，传递给UI界面的数据
    public class UIBaseData
    {
    }

    // 打开IM界面逻辑
    public class UIIMData : UIBaseData
    {
        public bool clear = true;
    }

    // 打开Loading界面数据
    public class UISceneLoadingData : UIBaseData
    {
        public float minTime = 1.5f;
        public Action callBack = null;
        public bool done = false;
    }

    public class UIManager : SingletonMonoBehaviour<UIManager>
    {
        private Dictionary<UIPanelType, UIBasePanel> mDictUI = new Dictionary<UIPanelType, UIBasePanel>();
        // TODO
        private Dictionary<UIPanelType, string> mDictUIResPath = new Dictionary<UIPanelType, string>
        {
            {UIPanelType.RTCAvatar, "UI/UI-RTC-Panel"},
            {UIPanelType.IM, "UI/UI-IM-Panel"},
            {UIPanelType.SelectHero, "UI/UI-SelectHero-Panel"},
            {UIPanelType.GameMain, "UI/UI-GameMain-Panel"},
            {UIPanelType.GamePrepare, "UI/UI-GamePrepare-Panel"},
            {UIPanelType.GameSceneLoading, "UI/UI-SceneLoading-Panel"},
        };

        private string TAG = "UIManager";
        public RectTransform mUIRoot;
        public Canvas mUIRootCanvas;
        public UILoading mSimpleLoading;
        public UIMessageBox mMessageBox;

        private GameObject mUIRootGameObject;

        public RectTransform mNormalDepthRoot;
        public RectTransform mMiddleDepthRoot;
        public RectTransform mTopDepthRoot;

        void Start()
        {
            YXUnityLog.LogInfo(TAG, "Start");
            mUIRootGameObject = mUIRoot.gameObject;

            // 调试，不从本地加载UI界面
            //UISelectHeroPanel selectPanel = GameObject.FindObjectOfType<UISelectHeroPanel>(true);
            //mDictUI.Add(UIPanelType.SelectHero, selectPanel);
            //UIGameMainPanel mainPanel = GameObject.FindObjectOfType<UIGameMainPanel>(true);
            //mDictUI.Add(UIPanelType.GameMain, mainPanel);
            //UIGamePreparePanel preparePanel = GameObject.FindObjectOfType<UIGamePreparePanel>(true);
            //mDictUI.Add(UIPanelType.GamePrepare, preparePanel);
            //UISceneLoadingPanel loadingPanel = GameObject.FindObjectOfType<UISceneLoadingPanel>(true);
            //mDictUI.Add(UIPanelType.GameSceneLoading, loadingPanel);
            //UIGameRTCPanel rtcpanel = GameObject.FindObjectOfType<UIGameRTCPanel>(true);
            //mDictUI.Add(UIPanelType.RTCAvatar, rtcpanel);
            //UIGameIMPanel impanel = GameObject.FindObjectOfType<UIGameIMPanel>(true);
            //impanel.RegisterEvent();
            //mDictUI.Add(UIPanelType.IM, impanel);
        }

        public void ShowUI(UIPanelType type, UIBaseData data)
        {
            YXUnityLog.LogInfo(TAG, $"ShowUI type: {type.ToString()}");
            UIBasePanel panel = LoadPanel(type);
            if (panel != null)
            {
                if (!mDictUI.ContainsKey(type))
                    mDictUI.Add(type, panel);
                panel.Show(data);
            }
            else
            {
                YXUnityLog.LogError(TAG, $"Can't Load UI Prefab");
            }
        }

        public void HideUI(UIPanelType type)
        {
            YXUnityLog.LogInfo(TAG, $"HideUI type: {type.ToString()}");
            UIBasePanel panel = GetUIPanel<UIBasePanel>(type);
            if (panel != null)
            {
                panel.Hide();
            }
            else
            {
                YXUnityLog.LogInfo(TAG, $"Skip HideUI Type is Not Loaded type: {type.ToString()}");
            }
        }

        public T GetUIPanel<T>(UIPanelType type) where T : UIBasePanel
        {
            if (mDictUI.ContainsKey(type))
            {
                return mDictUI[type] as T;
            }
            else
            {
                return default(T);
            }
        }

        private UIBasePanel LoadPanel(UIPanelType type)
        {
            if (mDictUI.ContainsKey(type))
                return mDictUI[type];

            string path = mDictUIResPath[type];
            YXUnityLog.LogInfo(TAG, $"LoadPanel type: {type}, res path: {path}");
            UnityEngine.Object tempalte = Resources.Load(path);
            Debug.Assert(tempalte != null);

            RectTransform root = mUIRoot;
            bool tolast = false;
            if (
                type == UIPanelType.RTCAvatar
                || type == UIPanelType.GameMain
                || type == UIPanelType.GamePrepare
                || type == UIPanelType.IM
                || type == UIPanelType.SelectHero)
            {
                root = mNormalDepthRoot;
            }
            else if (type == UIPanelType.GameSceneLoading)
            {
                root = mMiddleDepthRoot;
            }

            if (type == UIPanelType.IM)
                tolast = true;

            GameObject ui = GameObject.Instantiate(tempalte, root) as GameObject;
            if (tolast)
                ui.transform.SetAsLastSibling();
            UIBasePanel panel = ui.GetComponent<UIBasePanel>();
            return panel;
        }

        // Utils
        public void ShowMessageBox(string content, string firstText, Action first, string secondText = "", Action second = null)
        {
            mMessageBox.ShowMessageBox(content, firstText, first, secondText, second);
        }

        public void ShowLoading(bool show)
        {
            YXUnityLog.LogInfo(TAG, $"ShowLoading: {show}");
            mSimpleLoading.ShowLoading(show);
        }
    }
}

