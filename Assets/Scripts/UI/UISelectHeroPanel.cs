using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mini.Battle.Core;
using UnityEngine.UI;
using MetaVerse.FrameWork;

namespace Mini.Battle.UI
{
    public class UISelectHeroPanel : UIBasePanel
    {
        protected override string TAG
        {
            get { return "UISelectHeroPanel"; }
        }

        public Button mBtnNextPlayer;
        public Button mBtnPrePlayer;
        public Button mBtnConfirm;
        public Button mBtnBack;

        protected override void OnStart()
        {
            mBtnConfirm.onClick.AddListener(() =>
            {
                EventManager.TriggerEvent(GameEventDefine.Event_GoToNextUIPanel, UIPanelType.SelectHero, UIPanelType.GamePrepare);
            });

            mBtnNextPlayer.onClick.AddListener(() =>
            {
                EventManager.TriggerEvent(GameEventDefine.Event_OnSelectNextPlayer);
            });

            mBtnPrePlayer.onClick.AddListener(() =>
            {
                EventManager.TriggerEvent(GameEventDefine.Event_OnSelectPrePlayer);
            });

            mBtnBack.onClick.AddListener(() =>
            {
            });
        }

        protected override void OnShow(UIBaseData data)
        {
            base.OnShow(data);
            EventManager.TriggerEvent(GameEventDefine.Event_OnShowSelectCharacter, true);
        }

        protected override void OnHide()
        {
            base.OnHide();
            EventManager.TriggerEvent(GameEventDefine.Event_OnShowSelectCharacter, false);
        }
    }
}

