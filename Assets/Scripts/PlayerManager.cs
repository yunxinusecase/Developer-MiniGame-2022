using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MetaVerse.FrameWork;

namespace Mini.Battle.Core
{
    //
    // game player manager
    //
    public class PlayerManager : SingletonMonoBehaviour<PlayerManager>
    {
        public List<GameObject> mGBSelectPlayerList;
        private int mTotalSelectPlayerCount;
        public int mCurSelectPlayerIndex = 0;
        public Transform mTrsSelectRoot;
        public GameObject mRoot;

        private string TAG = "PlayerManager";

        // 当前角色索引
        public int GetCurPlayerIndex
        {
            get { return mCurSelectPlayerIndex; }
        }

        public GameObject GetPlayerTemplate(int index)
        {
            YXUnityLog.LogInfo(TAG, $"GetPlayerTemplate index: {index}");
            if (index < 0 || index >= mTotalSelectPlayerCount)
            {
                return null;
            }
            else
            {
                return mGBSelectPlayerList[index];
            }
        }

        private void Start()
        {
            mTotalSelectPlayerCount = mGBSelectPlayerList.Count;
            SetPlayerIndex(mCurSelectPlayerIndex);

            GameEventDefine.Event_OnSelectPrePlayer += OnSwitchPlayerPre;
            GameEventDefine.Event_OnSelectNextPlayer += OnSwitchPlayerNext;
            GameEventDefine.Event_OnShowSelectCharacter += OnShowChacterSelectScene;

            YXUnityLog.LogInfo(TAG, $"total player template count: {mTotalSelectPlayerCount}");
        }

        private void OnSwitchPlayerNext()
        {
            mCurSelectPlayerIndex++;
            if (mCurSelectPlayerIndex >= mTotalSelectPlayerCount)
                mCurSelectPlayerIndex = 0;
            SetPlayerIndex(mCurSelectPlayerIndex);
        }

        private void OnSwitchPlayerPre()
        {
            mCurSelectPlayerIndex--;
            if (mCurSelectPlayerIndex < 0)
                mCurSelectPlayerIndex = mTotalSelectPlayerCount - 1;
            SetPlayerIndex(mCurSelectPlayerIndex);
        }

        private void SetPlayerIndex(int index)
        {
            YXUnityLog.LogInfo(TAG, $"SetPlayerIndex index: {index}");
            float xvalue = index * 5;
            mTrsSelectRoot.transform.localPosition = new Vector3(xvalue, 0f, 0f);
        }

        public void OnShowChacterSelectScene(bool show)
        {
            mRoot.SetActive(show);
        }
    }
}