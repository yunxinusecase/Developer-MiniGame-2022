using System.Collections;
using System.Collections.Generic;
using MetaVerse.FrameWork;
using UnityEngine;
using Mini.Battle.Core;
using UnityEngine.UI;

namespace Mini.Battle.UI
{
    public class UIGamePreparePanel : UIBasePanel
    {
        //
        // Host(Server + Client)
        // Server Only
        // Client Need Ip
        //

        protected override string TAG
        {
            get { return "UIGameReadyPanel"; }
        }

        public Button mBtnCreateHost;
        public Button mBtnJoinServer;
        public Button mBtnBack;
        public InputField mIpInputField;

        protected override void OnAwake()
        {
            mIpInputField.text = GameManager.Instance.NetWorkDebugAddress;
            base.OnAwake();
            mBtnCreateHost.onClick.AddListener(OnClickCreateHost);
            mBtnJoinServer.onClick.AddListener(OnClickJoinServer);
            mBtnBack.onClick.AddListener(OnClickBack);
        }

        protected override void OnStart()
        {
            base.OnStart();
        }

        private void OnClickCreateHost()
        {
            EventManager.TriggerEvent(GameEventDefine.Event_StartGameAsHost);
        }

        private void OnClickJoinServer()
        {
            string ipadd = mIpInputField.text;
            bool isvalidIp = string.IsNullOrEmpty(ipadd);
            if (isvalidIp)
            {
                System.Net.IPAddress ipAddress = null;
                isvalidIp = System.Net.IPAddress.TryParse(ipadd, out ipAddress);
            }

            if (isvalidIp)
            {
                YXUnityLog.LogError(TAG, "invalid ip server address");
                UIManager.Instance.ShowMessageBox(GameHeaderTips.Tips_Empty_Ip_Address, "È·¶¨", () => { });
            }
            else
            {
                EventManager.TriggerEvent(GameEventDefine.Event_JoinGameServer, ipadd);
            }
        }

        private void OnClickBack()
        {
            EventManager.TriggerEvent(GameEventDefine.Event_GoToNextUIPanel, UIPanelType.GamePrepare, UIPanelType.SelectHero);
        }
    }
}

