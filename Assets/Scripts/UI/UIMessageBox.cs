using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Mini.Battle.Core;

namespace Mini.Battle.UI
{
    public class UIMessageBox : MonoBehaviour
    {
        public Button mButton001_1;
        public Text mTxtBtn001_1;
        public Button mButton001_2;
        public Text mTxtBtn001_2;
        public Button mButton002;
        public Text mTxtBtn002;

        public Button mButtonClose;
        public Text mContent;

        private Action m001_1Action;
        private Action m001_2Action;
        private Action m002_Action;

        public void ShowMessageBox(
            string content,
            string firstText,
            Action first,
            string secondText = "",
            Action second = null)
        {
            bool twobutton = false;
            if (first != null && second != null)
            {
                m001_1Action = first;
                m001_2Action = second;
                twobutton = true;
                mTxtBtn001_1.text = firstText;
                mTxtBtn001_2.text = secondText;
            }
            else
            {
                m002_Action = first;
                mTxtBtn002.text = firstText;
            }
            mContent.text = content;
            mButton001_1.gameObject.SetActive(twobutton);
            mButton001_2.gameObject.SetActive(twobutton);
            mButton002.gameObject.SetActive(!twobutton);

            this.gameObject.SetActive(true);
        }

        public void HideMessageBox()
        {
            m001_1Action = null;
            m001_2Action = null;
            m002_Action = null;
            this.gameObject.SetActive(false);
        }

        public void OnClickButton(Button obj)
        {
            if (obj == mButton001_1)
                m001_1Action?.Invoke();
            if (obj == mButton001_2)
                m001_2Action?.Invoke();
            if (obj == mButton002)
                m002_Action?.Invoke();
            HideMessageBox();
        }
    }
}

