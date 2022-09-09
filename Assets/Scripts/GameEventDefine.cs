using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using StarterAssets;
using UnityEngine.InputSystem;

namespace Mini.Battle.Core
{
    public static class GameEventDefine
    {
        public static Action Event_OnSelectNextPlayer;
        public static Action Event_OnSelectPrePlayer;

        public static Action<bool> Event_OnShowSelectCharacter;

        // game status 
        public static Action<UIPanelType, UIPanelType> Event_GoToNextUIPanel;

        // start game with host
        public static Action Event_StartGameAsHost;
        // start game join server
        public static Action<string> Event_JoinGameServer;
        // when local player created to set up camera
        public static Action<Transform> Event_SetMainCameraFollow;
        // set input
        public static Action<StarterAssetsInputs, PlayerInput> Event_SetInputAssets;

        // client status
        public static Action Event_OnClientConnect;
        public static Action Event_OnClientDisConnect;
        public static Action Event_OnNetWorkError;

        // net player created
        public static Action<ulong, NetPlayerController> Event_OnNetPlayerCreated;
        // net player leave
        public static Action<ulong, NetPlayerController> Event_OnNetPlayerDestroy;
        // net player ready to join rtc channel

        // IM Message 
        public static Action<long, NIMChatRoom.Message> Event_OnIMSendMessage;
        public static Action<long, NIMChatRoom.Message> Event_OnIMRevMessage;
        public static Action Event_OpenIMPanel;
        public static Action Event_CloseIMPanel;
    }
}