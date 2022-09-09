using System;
using System.Collections.Generic;
using System.Text;
using MetaVerse.FrameWork;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using Mini.Battle.Core;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

namespace Mini.Battle.Core
{
    public class BattleNetWorkManager : NetworkManager
    {
        // Overrides the base singleton so we don't
        // have to cast to this type everywhere.
        public static new BattleNetWorkManager singleton { get; private set; }
        private string TAG = "BattleNetWorkManager";

        #region Unity Callbacks

        public override void OnValidate()
        {
            base.OnValidate();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Start()
        {
            singleton = this;
            base.Start();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// </summary>
        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        #region Start & Stop

        /// <summary>
        /// Set the frame rate for a headless server.
        /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
        /// </summary>
        public override void ConfigureHeadlessFrameRate()
        {
            base.ConfigureHeadlessFrameRate();
        }

        /// <summary>
        /// called when quitting the application by closing the window / pressing stop in the editor
        /// </summary>
        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// This causes the server to switch scenes and sets the networkSceneName.
        /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
        /// </summary>
        /// <param name="newSceneName"></param>
        public override void ServerChangeScene(string newSceneName)
        {
            base.ServerChangeScene(newSceneName);
        }

        /// <summary>
        /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
        public override void OnServerChangeScene(string newSceneName) { }

        /// <summary>
        /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
        /// </summary>
        /// <param name="sceneName">The name of the new scene.</param>
        public override void OnServerSceneChanged(string sceneName) { }

        /// <summary>
        /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
        /// <param name="sceneOperation">Scene operation that's about to happen</param>
        /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling) { }

        /// <summary>
        /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
        /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
        /// </summary>
        public override void OnClientSceneChanged()
        {
            base.OnClientSceneChanged();
        }

        #endregion

        #region Server System Callbacks

        /// <summary>
        /// Called on the server when a new client connects.
        /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerConnect(NetworkConnectionToClient conn) { }

        /// <summary>
        /// Called on the server when a client is ready.
        /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
        }

        /// <summary>
        /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
        /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // 有Player加入到游戏中
            YXUnityLog.LogInfo(TAG, $"OnServerAddPlayer conn: {conn.connectionId}, add: {conn.address}");
            base.OnServerAddPlayer(conn);
        }

        /// <summary>
        /// Called on the server when a client disconnects.
        /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            YXUnityLog.LogInfo(TAG, $"OnServerDisconnect conn: {conn.connectionId}, add: {conn.address}");
            base.OnServerDisconnect(conn);
        }

        /// <summary>
        /// Called on server when transport raises an exception.
        /// <para>NetworkConnection may be null.</para>
        /// </summary>
        /// <param name="conn">Connection of the client...may be null</param>
        /// <param name="exception">Exception thrown from the Transport.</param>
        public override void OnServerError(NetworkConnectionToClient conn, Exception exception)
        {
            YXUnityLog.LogError(TAG, $"OnServerError connid: {conn.connectionId}, exception: {exception.Message}, stack: {exception.StackTrace}");
        }

        #endregion

        #region Client System Callbacks

        /// <summary>
        /// Called on the client when connected to a server.
        /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
        /// </summary>
        public override void OnClientConnect()
        {
            // Local Client Enter the Game
            YXUnityLog.LogInfo(TAG, "OnClientContent");
            base.OnClientConnect();
            EventManager.TriggerEvent(GameEventDefine.Event_OnClientConnect);
        }

        /// <summary>
        /// Called on clients when disconnected from a server.
        /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
        /// </summary>
        public override void OnClientDisconnect()
        {
            YXUnityLog.LogInfo(TAG, "OnClientDisconnect");
            base.OnClientDisconnect();
            EventManager.TriggerEvent(GameEventDefine.Event_OnClientDisConnect);
        }

        /// <summary>
        /// Called on clients when a servers tells the client it is no longer ready.
        /// <para>This is commonly used when switching scenes.</para>
        /// </summary>
        public override void OnClientNotReady() { }

        /// <summary>
        /// Called on client when transport raises an exception.</summary>
        /// </summary>
        /// <param name="exception">Exception thrown from the Transport.</param>
        public override void OnClientError(Exception exception)
        {
            YXUnityLog.LogError(TAG, $"OnClientError exception: {exception.Message}, stack: {exception.StackTrace}");
            EventManager.TriggerEvent(GameEventDefine.Event_OnNetWorkError);
        }

        #endregion

        #region Start & Stop Callbacks

        // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
        // their functionality, users would need override all the versions. Instead these callbacks are invoked
        // from all versions, so users only need to implement this one case.

        /// <summary>
        /// This is invoked when a host is started.
        /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        public override void OnStartHost()
        {
            YXUnityLog.LogInfo(TAG, "OnStartHost");
        }

        /// <summary>
        /// This is invoked when a server is started - including when a host is started.
        /// <para>StartServer has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        public override void OnStartServer()
        {
            YXUnityLog.LogInfo(TAG, "OnStartServer");
            NetworkServer.RegisterHandler<CreateBattlePlayerMessage>(OnCreateCharacter);
        }

        /// <summary>
        /// This is invoked when the client is started.
        /// </summary>
        public override void OnStartClient()
        {
            YXUnityLog.LogInfo(TAG, "OnStartClient");
        }

        /// <summary>
        /// This is called when a host is stopped.
        /// </summary>
        public override void OnStopHost()
        {
            YXUnityLog.LogInfo(TAG, "OnStopHost");
        }

        /// <summary>
        /// This is called when a server is stopped - including when a host is stopped.
        /// </summary>
        public override void OnStopServer()
        {
            YXUnityLog.LogInfo(TAG, "OnStopServer");
        }

        /// <summary>
        /// This is called when a client is stopped.
        /// </summary>
        public override void OnStopClient()
        {
            YXUnityLog.LogInfo(TAG, "OnStopClient");
        }

        #endregion

        public void RegisterPrefab(bool register)
        {
            YXUnityLog.LogInfo(TAG, "Register NetClient Prefab: " + register);
            List<GameObject> prefabs = GameManager.Instance.GetNetWorkClientPrefabs();
            for (int i = 0; i < prefabs.Count; i++)
            {
                if (register)
                    NetworkClient.RegisterPrefab(prefabs[i]);
                else
                    NetworkClient.UnregisterPrefab(prefabs[i]);
            }
        }

        private void OnCreateCharacter(NetworkConnectionToClient conn, CreateBattlePlayerMessage message)
        {
            YXUnityLog.LogInfo(TAG, $"OnCreateCharacter conn: {conn.connectionId}, index: {message.playerTemplateIndex}, rtcId: {message.playerRTCId}");
            int index = message.playerTemplateIndex;
            GameObject newplayerLogic = GameManager.Instance.GetPlayerTemplate(index);
            GameObject targetplayer = GameObject.Instantiate(newplayerLogic, new Vector3(Random.Range(0.0f, 10.0f), 0f, 0f), Quaternion.identity);
            string playername = $"Player - [connId={conn.connectionId}]";
            targetplayer.name = playername;
            NetworkServer.AddPlayerForConnection(conn, targetplayer);
            NetPlayerController pc = targetplayer.GetComponent<NetPlayerController>();

            // player's params
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}:{1}", GameManager.Instance.GetRTCChannelName(), message.playerRTCId);
            pc.mStrRTCData = sb.ToString();
        }

        public void CreatePlayer()
        {
            string strDeviceUnique = SystemInfo.deviceUniqueIdentifier;
            int id = Mathf.Abs(strDeviceUnique.GetHashCode());
            string chname = GameManager.Instance.GetRTCChannelName();
            YXUnityLog.LogInfo(TAG, $"CreatePlayer: {id}, strDeviceUnique: {strDeviceUnique},chname: {chname},id: {id.ToString()}");
            CreateBattlePlayerMessage message = new CreateBattlePlayerMessage
            {
                playerName = GameManager.Instance.GetPlayerName,
                playerTemplateIndex = GameManager.Instance.SelectPlayerTemplateIndex,
                playerRTCId = id,
            };
            NetworkClient.Send(message);
        }
    }
}
