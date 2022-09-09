using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mini.Battle.Core
{
    public class UIBasePanel : MonoBehaviour
    {
        protected virtual string TAG
        {
            get { return "UIBasePanel"; }
        }

        public bool IsShowing = false;

        void Awake()
        {
            OnAwake();
        }

        void Start()
        {
            OnStart();
        }

        void OnDestroy()
        {
            OnUIDestrory();
        }

        protected virtual void OnAwake()
        {
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnUIDestrory()
        {

        }

        public virtual void Show(UIBaseData data)
        {
            YXUnityLog.LogInfo(TAG, "Show");
            this.gameObject.SetActive(true);
            IsShowing = true;
            OnShow(data);
        }

        protected virtual void OnShow(UIBaseData data)
        {
        }

        public virtual void Hide()
        {
            YXUnityLog.LogInfo(TAG, "Hide");
            IsShowing = false;
            this.gameObject.SetActive(false);
            OnHide();
        }

        protected virtual void OnHide()
        {
        }

        public virtual void ShutDown()
        {

        }

        protected virtual void OnUpdate(float ftime)
        {
        }

        void Update()
        {
            float delta = Time.deltaTime;
            OnUpdate(delta);
        }
    }
}
