using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Mini.Battle.Core
{
    //
    // NetWork Message Define
    //

    // 创建Player
    public struct CreateBattlePlayerMessage : NetworkMessage
    {
        public int playerTemplateIndex;
        public string playerName;
        public int playerRTCId;
        public string channelName;
    }
}
