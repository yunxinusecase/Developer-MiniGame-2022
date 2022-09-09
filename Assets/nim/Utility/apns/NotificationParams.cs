#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;
using Newtonsoft.Json;

namespace NIM.Notification
{

    public class NotificationSetting
    {
        public bool Sound = true;
        public bool Vibrate = true;
        public bool Light = true;
        public NotificationIcon SmallIcon = NotificationIcon.Message;
        public Color SmallIconColor = new Color(0, 0.6f, 1);
        public string LargeIcon = "app_icon";
    }

    public class NotificationParams : NimUtility.NimJsonObject<NotificationParams>
    {
        public NotificationParams()
        {
            this.Sound = true;
            this.Vibrate = true;
            this.Light = true;
            this.SmallIcon = NotificationIcon.Message;
        }
        /// <summary>
        /// 通知id
        /// </summary>
        /// 
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonIgnore]
        public TimeSpan Delay { get; set; }

        /// <summary>
        /// 延时发送(毫秒)
        /// </summary>
        [JsonProperty("delay")]
        public long delayMilliseconds
        {
            get
            {
                if (Delay == null)
                    return 0;
                return (long)Delay.TotalMilliseconds;
            }
        }

        /// <summary>
        /// 消息标题
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>

        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// ticker内容
        /// </summary>
        [JsonProperty("ticker")]
        public string Ticker { get; set; }

        /// <summary>
        /// 是否提示音
        /// </summary>

        [JsonProperty("sound")]
        public bool Sound { get; set; }

        /// <summary>
        /// 是否震动
        /// </summary>
        [JsonProperty("vibrate")]
        public bool Vibrate { get; set; }

        /// <summary>
        /// 呼吸灯是否打开
        /// </summary>
        [JsonProperty("light")]
        public bool Light { get; set; }
        
        [JsonIgnore]
        public NotificationIcon SmallIcon { get; set; }

        /// <summary>
        /// 小图标（必填，文件必须存在)
        /// </summary>
        [JsonProperty("small_icon")]
        public string smallIcon
        {
            get
            {
               return "anp_" + SmallIcon.ToString().ToLower();
            }
        }

        [JsonIgnore]
        public Color SmallIconColor { get; set; }

        /// <summary>
        /// 小图标颜色
        /// </summary>
        [JsonProperty("small_icon_color")]
        public int colorSmallIcon
        {
            get
            {
                if (SmallIconColor == null)
                    return 0;
                var color = (Color32)SmallIconColor;
                return color.r * 65536 + color.g * 256 + color.b;
            }
        }
        /// <summary>
        /// 大图标，使用 ""作为简单. "app_icon" 默认为app图标. 使用自定义值必须先将图片放到 "nim_sdk.aar/res/"（方法：修改.aar文件为zip，将图片放入指定目录，然后再改回aar后缀).
        /// </summary>
        [JsonProperty("large_icon")]
        public string LargeIcon;

        /// <summary>
        /// 扩展数据,点击跳转后回调给指定的MainActivity
        /// </summary>
        [JsonProperty("extra_data")]
        public string extraData;
    }
}
#endif