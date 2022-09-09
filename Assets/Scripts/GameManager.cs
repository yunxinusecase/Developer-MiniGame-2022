using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Cinemachine;
using UnityEngine;
using MetaVerse.FrameWork;
using Mini.Battle.IM;
using Mini.Battle.RTC;
using Mini.Battle.UI;
using Mirror;
using StarterAssets;
using UnityEngine.InputSystem;

namespace Mini.Battle.Core
{
    public class GameManager : SingletonMonoBehaviour<GameManager>
    {
        public Camera mGameMainCamera;
        public CinemachineVirtualCamera mVirtualCamera;
        private GameStatus mCurGameStatus = GameStatus.SplashLoading;
        private string TAG = "GameManager";
        private BattleNetWorkManager mNetWorkManager;
        private RTCMainLogic mRTCMainLogic;
        private IMLogic mIMLogic;

        public RTCMainLogic RTCLogic
        {
            get { return mRTCMainLogic; }
        }

        public IMLogic IMLogic
        {
            get { return mIMLogic; }
        }

        [Header("Player Input Config")]
        public InputActionAsset mInputAssets;
        public StarterAssetsInputs mPlayerInputs;
        public PlayerInput mPlayerInput;

        void Start()
        {
            mNetWorkManager = this.GetComponent<BattleNetWorkManager>();
            mRTCMainLogic = this.GetComponent<RTCMainLogic>();
            mIMLogic = this.GetComponent<IMLogic>();

            GameEventDefine.Event_GoToNextUIPanel += OnGoToNextUIPanel;
            GameEventDefine.Event_StartGameAsHost += OnTryStartGameAsHost;
            GameEventDefine.Event_JoinGameServer += OnTryJoinServerMatch;
            GameEventDefine.Event_SetMainCameraFollow += OnSetMainCameraLookAt;
            GameEventDefine.Event_OnClientConnect += OnClientConnect;
            GameEventDefine.Event_OnClientDisConnect += OnClientDisConnect;
            GameEventDefine.Event_OnNetWorkError += OnClientError;
            GameEventDefine.Event_SetInputAssets += OnSetInputAssets;
            GameEventDefine.Event_OnNetPlayerCreated += OnNetPlayerCreated;
            GameEventDefine.Event_OnNetPlayerDestroy += OnNetPlayerDestroy;
            GameEventDefine.Event_OpenIMPanel += OnOpenIMPanel;
            GameEventDefine.Event_CloseIMPanel += OnCloseIMPanel;
        }

        private void OnGoToNextUIPanel(UIPanelType curtype, UIPanelType targettype)
        {
            if (curtype == UIPanelType.SelectHero && targettype == UIPanelType.GamePrepare)
            {
                // 从选择角色到游戏准备界面
                UIManager.Instance.HideUI(UIPanelType.SelectHero);
                UIManager.Instance.ShowUI(UIPanelType.GamePrepare, null);

                mIMLogic.InitSDK();
            }
            else if (curtype == UIPanelType.GamePrepare || targettype == UIPanelType.SelectHero)
            {
                // 从游戏准备界面到角色选择界面
                UIManager.Instance.HideUI(UIPanelType.GamePrepare);
                UIManager.Instance.ShowUI(UIPanelType.SelectHero, null);
            }
        }

        // Create Match（Server+Client Type）
        // 创建游戏，当前设备作为Server+Client（Host）
        private void OnTryStartGameAsHost()
        {
            if (!NetworkClient.active)
            {
                RefreshRTCChannelName();
                UIManager.Instance.ShowLoading(true);
                YXUnityLog.LogInfo(TAG, "OnTryStartGameAsHost");
                mNetWorkManager.RegisterPrefab(true);
                mNetWorkManager.StartHost();
            }
        }

        // Join Match with server Ip
        // 加入指定Server游戏，当前设备作为Client
        private void OnTryJoinServerMatch(string address)
        {
            if (!NetworkClient.active)
            {
                UIManager.Instance.ShowLoading(true);
                mNetWorkManager.RegisterPrefab(true);
                YXUnityLog.LogInfo(TAG, $"OnTryJoinServerMatch address: {address}");
                mNetWorkManager.networkAddress = address;
                mNetWorkManager.StartClient();
            }
        }

        public void BootGame()
        {
            UIManager.Instance.ShowUI(UIPanelType.SelectHero, null);
        }

        // 加载游戏场景，进入到游戏
        private void EnterGameLoading(GameStatus tostatus)
        {
            UISceneLoadingData data = new UISceneLoadingData();
            data.minTime = 1.5f;
            if (tostatus == GameStatus.GameMain)
                data.callBack = EnterGame;
            else if (tostatus == GameStatus.CharacterSelect)
            {
                data.callBack = ExitGameToSelectCharacter;
                data.done = true;
            }
            UIManager.Instance.ShowUI(UIPanelType.GameSceneLoading, data);
        }

        private void EnterGame()
        {
            YXUnityLog.LogInfo(TAG, "EnterGame");
            UIManager.Instance.HideUI(UIPanelType.GameSceneLoading);
            UIManager.Instance.HideUI(UIPanelType.GamePrepare);
            UIManager.Instance.HideUI(UIPanelType.IM);
            UIManager.Instance.ShowUI(UIPanelType.GameMain, null);
        }

        public void ExitGame()
        {
            YXUnityLog.LogInfo(TAG, "ExitGame");
            mNetWorkManager.RegisterPrefab(false);
            if (NetworkServer.active)
            {
                mNetWorkManager.StopHost();
            }
            else
            {
                mNetWorkManager.StopClient();
            }
            EnterGameLoading(GameStatus.CharacterSelect);
        }

        private void ExitGameToSelectCharacter()
        {
            UIManager.Instance.ShowUI(UIPanelType.GamePrepare, null);
            UIManager.Instance.HideUI(UIPanelType.GameSceneLoading);
            UIManager.Instance.HideUI(UIPanelType.GameMain);
            UIManager.Instance.HideUI(UIPanelType.RTCAvatar);
            UIManager.Instance.HideUI(UIPanelType.IM);
            hasOpenIM = false;
        }

        // 加入游戏成功
        private void OnClientConnect()
        {
            UIManager.Instance.ShowLoading(false);
            EnterGameLoading(GameStatus.GameMain);
            mNetWorkManager.CreatePlayer();

            // 
            UISceneLoadingPanel panel = UIManager.Instance.GetUIPanel<UISceneLoadingPanel>(UIPanelType.GameSceneLoading);
            panel.SetLoadingDone();
        }

        private void OnClientDisConnect()
        {
            UIManager.Instance.ShowLoading(false);
            ExitGame();
        }

        private void OnClientError()
        {
            UIManager.Instance.ShowLoading(false);
            UIManager.Instance.ShowMessageBox(GameHeaderTips.Tip_Start_Game_Error, "确定", () =>
            {
            });
        }

        private void OnSetMainCameraLookAt(Transform target)
        {
            mVirtualCamera.m_Follow = target;
        }

        private void OnSetInputAssets(StarterAssetsInputs inputs, PlayerInput playerInput)
        {
            mPlayerInputs = inputs;
            mPlayerInput = playerInput;
        }

        // Get Player Information
        public int SelectPlayerTemplateIndex
        {
            get { return PlayerManager.Instance.GetCurPlayerIndex; }
        }

        public GameObject GetPlayerTemplate(int index)
        {
            return PlayerManager.Instance.GetPlayerTemplate(index);
        }

        public List<GameObject> GetNetWorkClientPrefabs()
        {
            return PlayerManager.Instance.mGBSelectPlayerList;
        }

        public string NetWorkDebugAddress
        {
            get { return mNetWorkManager.networkAddress; }
        }

        public string GetPlayerName
        {
            get { return "My-Player"; }
        }

        // RTCUserID
        private Dictionary<ulong, NetPlayerController> mCurNetPlayers = new Dictionary<ulong, NetPlayerController>();
        private void OnNetPlayerCreated(ulong id, NetPlayerController player)
        {
            if (player == null)
            {
                YXUnityLog.LogError(TAG, "OnNetPlayerCreated error");
                return;
            }

            if (mCurNetPlayers.ContainsKey(id))
            {
                YXUnityLog.LogError(TAG, $"OnNetPlayerCreated has contains id: {id}");
                return;
            }
            mCurNetPlayers.Add(id, player);
            // 当本地角色加入到游戏，需要执行Join RTC操作
            YXUnityLog.LogInfo(TAG, $"OnNetPlayerCreated: localPlayer: {player.isLocalPlayer}, rtcId: {player.mRTCUserId}, netId: {player.netId}, isServer: {player.isServer}, serverActive: {NetworkServer.active}");
            if (player.isLocalPlayer)
            {
                mRTCMainLogic.StartRTCLogic(GameConst.c_Appkey, player.mRTCChannelId, (ulong)player.mRTCUserId);
                mRTCMainLogic.Set3DSpaceSoundTarget(player.transform);
                mIMLogic.LoginToRoomId();
            }
        }

        private void OnNetPlayerDestroy(ulong id, NetPlayerController player)
        {
            if (player == null)
            {
                YXUnityLog.LogError(TAG, "OnNetPlayerDestroy error");
                return;
            }
            if (!mCurNetPlayers.ContainsKey(id))
            {
                YXUnityLog.LogError(TAG, $"OnNetPlayerDestroy not contains id: {id}");
                return;
            }
            mCurNetPlayers.Remove(id);
            // 当本地角色加入到游戏，需要执行Leave RTC操作
            // 不执行ReleaseEngine操作
            YXUnityLog.LogInfo(TAG, $"OnNetPlayerDestroy: localPlayer: {player.isLocalPlayer}, rtcId: {player.mRTCUserId}, netId: {player.netId}, isServer: {player.isServer}, serverActive: {NetworkServer.active}");
            if (player.isLocalPlayer)
            {
                mIMLogic.ExitRoom();
                mRTCMainLogic.LeaveChannel();
            }
        }

        public bool EnablePlayerInput
        {
            get
            {
                UIGameIMPanel panel = UIManager.Instance.GetUIPanel<UIGameIMPanel>(UIPanelType.IM);
                if (panel != null && panel.IsShowing)
                    return false;
                else
                    return true;
            }
        }

        // TODO
        private bool hasOpenIM = false;
        private void OnOpenIMPanel()
        {
            UIGameIMPanel panel = UIManager.Instance.GetUIPanel<UIGameIMPanel>(UIPanelType.IM);
            UIIMData data = new UIIMData();
            data.clear = !hasOpenIM;
            UIManager.Instance.ShowUI(UIPanelType.IM, data);
            hasOpenIM = true;
        }

        private void OnCloseIMPanel()
        {
            UIManager.Instance.HideUI(UIPanelType.IM);
        }

        private string mRTCChannelName = string.Empty;

        public void RefreshRTCChannelName()
        {
            string strDeviceUnique = SystemInfo.deviceUniqueIdentifier;
            mRTCChannelName = $"mini_{strDeviceUnique}_{DateTime.Now.ToFileTimeUtc().ToString()}";
        }

        public string GetRTCChannelName()
        {
            return mRTCChannelName;
        }

        public NetPlayerController GetNetPlayerController(ulong userid)
        {
            if (mCurNetPlayers.ContainsKey(userid))
                return mCurNetPlayers[userid];
            else
                return null;
        }
    }
}