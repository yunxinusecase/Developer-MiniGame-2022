using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mini.Battle.Core
{
    public static class GameConst
    {
        // [Tooltip("You need firstly create you app and fetch your APP_KEY.View detail to https://doc.yunxin.163.com/nertc/docs/TA0ODQ2NjI?platform=unity")]
        public const string c_Appkey = "YOUR APP KEY";

        // [Tooltip("You need register an account for yourself firstly.And set password for your account.View detail to https://doc.yunxin.163.com/messaging/docs/jMwMTQxODk?platform=android")]
        public const string c_IMAccountName = "YOUR ACCOUNT NAME";
        public const string c_IMPassCode = "YOUR PASSCODE";

        // [Tooltip("You should call server-api to create chat room and retrieve the room ID.View detail to https://doc.yunxin.163.com/messaging/docs/jA0MzQxOTI?platform=server")]
        public const long c_IMRoomId = 0;
    }

    public static class GameHeaderTips
    {
        public static string Tips_Empty_Ip_Address = "请输入正确的IP地址";
        public static string Tip_Join_Server_Error = "加入游戏失败，请确定输入的IP地址或者Server不存在";
        public static string Tip_Start_Host_Error = "创建游戏失败，请重试";
        public static string Tip_Start_Game_Error = "网络异常，请重试";

        public static string Tips_OpenVideo = "开启视频";
        public static string Tips_CloseVideo = "关闭视频";

        public static string Tips_EnableAvatar = "开启虚拟人";
        public static string Tips_DisableAvatar = "关闭虚拟人";

        public static string Tips_OpenAudio = "开启音频";
        public static string Tips_CloseAudio = "关闭音频";
    }
}
