using System.Collections;
using System.Collections.Generic;
using MetaVerse.FrameWork;
using UnityEngine;
using Mini.Battle.Core;
using NIMAudio;
using UnityEngine.UI;

namespace Mini.Battle.UI
{
    public class UIGameIMPanel : UIBasePanel
    {
        public Button mBtnCloseRoom;
        public Button mBtnSendMessage;
        public InputField mInputFiledSendMessage;
        public GameObject BG;

        GameObject m_World_Go;
        Button m_NewMessage_Btn;
        Text m_NewMessage_Txt;
        public ScrollRect mScrollRect;
        public Transform m_ChatContent_Ts;

        protected override string TAG
        {
            get { return "UIGameIMPanel"; }
        }

        //显示的最大聊天数量
        int showMaxNum = 200;
        int newMessageNum = 0;
        private bool isInit = false;
        //所有的聊天（最大长度是展示的两倍）
        List<GameObject> allChatItemList = new List<GameObject>();
        //展示的聊天
        List<GameObject> chatItemShowList = new List<GameObject>();

        protected override void OnAwake()
        {
            mBtnCloseRoom.onClick.AddListener(OnCloseIMPanel);
            mBtnSendMessage.onClick.AddListener(OnClickSendMessage);

            GameEventDefine.Event_OnIMSendMessage += OnSendIMMessage;
            GameEventDefine.Event_OnIMRevMessage += OnRecvIMMessage;
        }

        protected override void OnShow(UIBaseData data)
        {
            base.OnShow(data);
            mInputFiledSendMessage.text = string.Empty;
            UIIMData imdata = data as UIIMData;
            if (imdata == null || imdata.clear)
            {
                allChatItemList.Clear();
                chatItemShowList.Clear();

                int count = m_ChatContent_Ts.childCount;
                List<GameObject> objs = new List<GameObject>();
                for (int i = 0; i < count; i++)
                {
                    objs.Add(m_ChatContent_Ts.GetChild(i).gameObject);
                }
                m_ChatContent_Ts.DetachChildren();
                for (int i = 0; i < count; i++)
                {
                    GameObject.Destroy(objs[i]);
                }
            }
        }

        private void OnCloseIMPanel()
        {
            EventManager.TriggerEvent(GameEventDefine.Event_CloseIMPanel);
        }

        private void OnClickSendMessage()
        {
            string msg = mInputFiledSendMessage.text;
            if (!string.IsNullOrEmpty(msg))
            {
                YXUnityLog.LogInfo(TAG, "send message: " + msg);
                GameManager.Instance.IMLogic.SendMessageToRoom(msg);
            }
            mInputFiledSendMessage.text = string.Empty;
        }

        // 接收到IM，SendMessage消息
        private void OnSendIMMessage(long roomid, NIMChatRoom.Message message)
        {
            // TODO
            YXUnityLog.LogInfo(TAG, "OnSendIMMessage");

            GameObject chatItem = null;
            if (chatItemShowList.Count <= showMaxNum)
            {
                chatItem = (GameObject)Instantiate(Resources.Load("WorldMyChatItem"));
            }
            else
            {
                bool isHave = false;
                for (int i = 0; i < allChatItemList.Count - showMaxNum; i++)
                {
                    allChatItemList[i].SetActive(false);
                }
                for (int i = 0; i < allChatItemList.Count - showMaxNum; i++)
                {
                    if (allChatItemList[i].name == "WorldMyChatItem")
                    {
                        chatItem = allChatItemList[i];
                        allChatItemList.RemoveAt(i);
                        isHave = true;
                        break;
                    }
                }

                if (!isHave)
                {
                    chatItem = (GameObject)Instantiate(Resources.Load("WorldMyChatItem"));
                }
                chatItemShowList.RemoveAt(0);
                chatItem.SetActive(false);
            }
            WorldMyChatItem myChatItem = chatItem.transform.GetComponent<WorldMyChatItem>();

            myChatItem.Init(message.MessageAttachment.ToString());
            chatItem.name = "WorldMyChatItem";
            chatItem.transform.SetParent(m_ChatContent_Ts);
            chatItem.transform.localScale = Vector3.one;
            chatItem.transform.localPosition = Vector3.zero;
            chatItem.transform.SetAsLastSibling();
            chatItem.SetActive(true);

            this.chatItemShowList.Add(chatItem);
            this.allChatItemList.Add(chatItem);

            if (this.isActiveAndEnabled)
                OnClickNewMessage();
        }

        private void OnClickNewMessage()
        {
            StartCoroutine(SetScrollView());
        }

        // 接收到IM，OnRecv消息
        private void OnRecvIMMessage(long roomid, NIMChatRoom.Message message)
        {
            // TODO
            YXUnityLog.LogInfo(TAG, "OnRecvIMMessage");
            GameObject chatItem = null;
            if (chatItemShowList.Count <= showMaxNum)
            {
                chatItem = (GameObject)Instantiate(Resources.Load("WorldOtherPeopleChatItem"));
            }
            else
            {
                bool isHave = false;
                for (int i = 0; i < allChatItemList.Count - showMaxNum; i++)
                {
                    allChatItemList[i].SetActive(false);
                }
                for (int i = 0; i < allChatItemList.Count - showMaxNum; i++)
                {
                    if (allChatItemList[i].name == "WorldOtherPeopleChatItem")
                    {
                        chatItem = allChatItemList[i];
                        allChatItemList.RemoveAt(i);
                        isHave = true;
                        break;
                    }
                }

                if (!isHave)
                {
                    chatItem = (GameObject)Instantiate(Resources.Load("WorldOtherPeopleChatItem"));
                }
                chatItemShowList.RemoveAt(0);
                chatItem.SetActive(false);
            }
            WorldOtherPeopleChatItem myChatItem = chatItem.transform.GetComponent<WorldOtherPeopleChatItem>();

            myChatItem.Init(message.MessageAttachment);
            chatItem.name = "WorldOtherPeopleChatItem";
            chatItem.transform.SetParent(m_ChatContent_Ts);
            chatItem.transform.localScale = Vector3.one;
            chatItem.transform.localPosition = Vector3.zero;
            chatItem.transform.SetAsLastSibling();
            chatItem.SetActive(true);
            this.chatItemShowList.Add(chatItem);
            this.allChatItemList.Add(chatItem);
            if (this.isActiveAndEnabled)
                OnClickNewMessage();
        }

        IEnumerator SetScrollView()
        {
            yield return null;
            mScrollRect.verticalNormalizedPosition = 0;
        }
    }
}