using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MetaVerse.FrameWork;
using Mini.Battle.Core;


namespace Mini.Battle.UI
{
    public class UILoading : MonoBehaviour
    {
        public void ShowLoading(bool show)
        {
            this.gameObject.SetActive(show);
        }
    }
}
