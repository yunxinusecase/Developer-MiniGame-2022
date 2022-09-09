using System.Collections;
using System.Collections.Generic;
using Mini.Battle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Mini.Battle.UI
{
    public class UIGameRTCPanel : UIBasePanel
    {
        protected override string TAG
        {
            get { return "UIGameRTCPanel"; }
        }

        [Header("对端视频展示界面")]
        public HorizontalLayoutGroup mLayOutGroup;

        [Header("对端展示Item模板")]
        public GameObject mCellTemplate;
        public OwnTextureCellItem mOwnCellItem;

        private List<TextureCellItem> mFreeCells = new List<TextureCellItem>();
        private Dictionary<ulong, TextureCellItem> mDicItems = new Dictionary<ulong, TextureCellItem>();

        // UI 界面接受RTC相关的消息
        // uid = 0 own userid
        public void OnUserJoined(ulong uid)
        {
            if (!mDicItems.ContainsKey(uid))
            {
                TextureCellItem item = uid != 0 ? GetOrCreateItem() : mOwnCellItem;
                mDicItems[uid] = item;
                item.AddCell(uid);
                YXUnityLog.LogInfo(TAG, $"CreateItem new cell item uid: {uid.ToString()}");
            }
            else
            {
                YXUnityLog.LogInfo(TAG, "CreateItem has contains uid: " + uid.ToString());
                mDicItems[uid].AddCell(uid);
            }

            RectTransform rt = mLayOutGroup.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            rt.anchoredPosition = Vector2.zero;
            StartCoroutine(_LayOutRebuild());
        }

        public void OnUserLeft(ulong uid)
        {
            if (mDicItems.ContainsKey(uid))
            {
                TextureCellItem item = mDicItems[uid];
                item.RemoveCell(uid);
                if (uid != 0)
                {
                    mFreeCells.Add(item);
                }
                mDicItems.Remove(uid);
                YXUnityLog.LogInfo(TAG, "OnUserVideoStop remove cell uid: " + uid);
            }
            else
            {
                YXUnityLog.LogWarning(TAG, "OnUserVideoStop dic items not has uid: " + uid);
            }
        }

        public void OnTextureCallBack(ulong uid, Texture texture, int rotate)
        {
            if (mDicItems.ContainsKey(uid))
            {
                mDicItems[uid].UpdateTexture(uid, texture, rotate);
            }
        }

        private TextureCellItem GetOrCreateItem()
        {
            if (mFreeCells.Count > 0)
            {
                TextureCellItem cell = mFreeCells[0];
                mFreeCells.RemoveAt(0);
                return cell;
            }
            else
            {
                GameObject newone = GameObject.Instantiate(mCellTemplate, Vector3.zero, Quaternion.identity, mLayOutGroup.transform);
                TextureCellItem item = newone.GetComponent<TextureCellItem>();
                return item;
            }
        }

        IEnumerator _LayOutRebuild()
        {
            yield return null;
            RectTransform rt = mLayOutGroup.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            rt.anchoredPosition = Vector2.zero;
        }

        public void Release()
        {
            YXUnityLog.LogInfo(TAG, "Release()");
            foreach (var item in mDicItems)
            {
                if (item.Key != 0)
                    mFreeCells.Add(item.Value);
            }
            mDicItems.Clear();
            foreach (var item in mFreeCells)
            {
                item.RemoveCell(0);
            }
            mOwnCellItem.RemoveCell(0);
            Resources.UnloadUnusedAssets();
        }
    }
}
