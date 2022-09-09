using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Mini.Battle.UI
{
    public class TextureCellItem : MonoBehaviour
    {
        public Image mBackGround;
        public RawImage mCellTexture;
        public Text mUid;

        protected virtual void Awake()
        {
        }

        public virtual void AddCell(ulong uid)
        {
            mUid.text = uid.ToString();
            mCellTexture.texture = null;
            this.gameObject.SetActive(true);
        }

        public virtual void RemoveCell(ulong uid)
        {
            mUid.text = string.Empty;
            mCellTexture.texture = null;
            this.gameObject.SetActive(false);
        }

        public void UpdateTexture(ulong uid, Texture texture, int rotateangle)
        {
            // TODO LayOut
            if (texture != null)
            {
                mCellTexture.texture = texture;
                mCellTexture.rectTransform.localScale = new Vector3(1f, 1f, 1f);
                if (Application.platform == RuntimePlatform.Android)
                {
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                }
                else
                {
                }
                mCellTexture.rectTransform.localRotation = Quaternion.Euler(0, 0, -rotateangle);
            }
        }
    }
}

