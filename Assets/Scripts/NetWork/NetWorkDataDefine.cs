using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace Mirror
{
    public struct GamePlayerRTCIMData
    {
        // 房间名称，有服务器创建
        public string mRtcChannelId { get; set; }

        // 当前角色的userid，用来加入RTC房间使用
        public int mRTCUserId { get; set; }

        public GamePlayerRTCIMData(string channelId, int userid)
        {
            this.mRtcChannelId = channelId;
            this.mRTCUserId = userid;
        }
    }

    public static class CustomReadWriteFunctions
    {
        public static void WriteGamePlayerRTCIMData(this NetworkWriter writer, GamePlayerRTCIMData value)
        {
            writer.WriteString(value.mRtcChannelId);
            writer.WriteInt(value.mRTCUserId);
            Debug.Log("write here test");
        }

        public static GamePlayerRTCIMData ReadGamePlayerRTCIMData(this NetworkReader reader)
        {
            Debug.Log("write here test 111");
            return new GamePlayerRTCIMData(reader.ReadString(), reader.ReadInt());
        }
    }
}
