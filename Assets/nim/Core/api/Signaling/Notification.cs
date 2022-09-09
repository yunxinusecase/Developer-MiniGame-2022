using NIM.Signaling.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NIM.Signaling
{
    /// <summary>
    /// 事件通知信息基类
    /// </summary>
    public class NIMSignalingNotityInfo
    {
        /// <summary>
        /// 通知类型
        /// </summary>
        public virtual NIMSignalingEventType event_type_ { get; private set; }
        /// <summary>
        /// 频道信息 
        /// </summary>
        public NIMSignalingChannelInfo channel_info_;
        /// <summary>
        /// 操作者
        /// </summary>
        public string from_account_id_;
        /// <summary>
        /// 操作的扩展字段
        /// </summary>
        public string custom_info_;
        /// <summary>
        /// 操作的时间戳
        /// </summary>
        public ulong msg_id_;

        void SetNotificationBaseInfo(IntPtr ptr)
        {
            var notification = NimUtility.Utilities.IntPtrToStructure<CNotification>(ptr);
            var notifyData = NimUtility.Utilities.IntPtrToStructure<CNotificationData>(notification.Data);
            custom_info_ = NimUtility.Utilities.MarshalUtf8String(notifyData.custom_info_);
            from_account_id_ = NimUtility.Utilities.MarshalUtf8String(notifyData.from_account_id_);
            msg_id_ = notifyData.msg_id_;
            var cChannel = NimUtility.Utilities.IntPtrToStructure<NIMSignalingChannelInfo_C>(notifyData.channel_info_);
            channel_info_ = SignalHelper.NIMSignalingChannelInfoFromC(cChannel);
        }

        public void SetNotification(IntPtr ptr)
        {
            SetNotificationBaseInfo(ptr);
            SetData(ptr);
        }

        protected virtual void SetData(IntPtr ptr)
        {
            
        }
    }

    /// <summary>
    /// 频道关闭事件通知信息，event_type_=kNIMSignalingEventTypeClose
    /// </summary>
    class NIMSignalingNotityInfoClose : NIMSignalingNotityInfo
    {
        public override NIMSignalingEventType event_type_ { get { return NIMSignalingEventType.kNIMSignalingEventTypeClose; } }
    }

    /// <summary>
    /// 加入频道事件通知信息，event_type_=kNIMSignalingEventTypeJoin
    /// </summary>
    class NIMSignalingNotityInfoJoin : NIMSignalingNotityInfo
    {
        /// <summary>
        ///  加入成员的信息，用于获得uid
        /// </summary>
        public NIMSignalingMemberInfo member_;

        public override NIMSignalingEventType event_type_ { get { return NIMSignalingEventType.kNIMSignalingEventTypeJoin; } }

        protected override void SetData(IntPtr ptr)
        {
            var notification = NimUtility.Utilities.IntPtrToStructure<CJoinNotification>(ptr);
            member_ = NimUtility.Utilities.IntPtrToStructure<NIMSignalingMemberInfo>(notification.Member);
        }
    }

    /// <summary>
    /// 邀请事件通知信息，event_type_=kNIMSignalingEventTypeInvite
    /// </summary>
    public class NIMSignalingNotityInfoInvite : NIMSignalingNotityInfo
    {

        public override NIMSignalingEventType event_type_ { get { return NIMSignalingEventType.kNIMSignalingEventTypeInvite; } }
        /// <summary>
        /// 被邀请者的账号
        /// </summary>
        public string to_account_id_;
        /// <summary>
        /// 邀请者邀请的请求id，用于被邀请者回传request_id_作对应的回应操作
        /// </summary>
        public string request_id_;
        /// <summary>
        ///  推送信息
        /// </summary>
        public NIMSignalingPushInfo push_info_;

        protected override void SetData(IntPtr ptr)
        {
            var notification = NimUtility.Utilities.IntPtrToStructure<CInviteNotification>(ptr);
            to_account_id_ = NimUtility.Utilities.MarshalUtf8String(notification.Account);
            request_id_ = NimUtility.Utilities.MarshalUtf8String(notification.RequestID);
            push_info_ = NimUtility.Utilities.IntPtrToStructure<NIMSignalingPushInfo>(notification.PushInfo);
        }
    }

    /// <summary>
    /// 退出频道事件通知信息，event_type_=kNIMSignalingEventTypeLeave
    /// </summary>
    public class NIMSignalingNotityInfoLeave : NIMSignalingNotityInfo
    {
        public override NIMSignalingEventType event_type_ { get { return NIMSignalingEventType.kNIMSignalingEventTypeLeave; } }
    }


    /// <summary>
    /// 控制事件通知信息，event_type_=kNIMSignalingEventTypeCtrl
    /// </summary>
    public class NIMSignalingNotityInfoControl : NIMSignalingNotityInfo
    {
        public override NIMSignalingEventType event_type_ { get { return NIMSignalingEventType.kNIMSignalingEventTypeCtrl; } }
    }

    public class NIMSignalingNotityProcessInvite: NIMSignalingNotityInfo
    {
        /// <summary>
        /// 邀请者的账号
        /// </summary>
        public string to_account_id_;
        /// <summary>
        /// 邀请者邀请的请求id
        /// </summary>
        public string request_id_;

        protected override void SetData(IntPtr ptr)
        {
            var notification = NimUtility.Utilities.IntPtrToStructure<CInviteProcessingNotification>(ptr);
            to_account_id_ = NimUtility.Utilities.MarshalUtf8String(notification.Account);
            request_id_ = NimUtility.Utilities.MarshalUtf8String(notification.RequestID);
        }
    }

    /// <summary>
    /// 取消邀请事件通知信息，event_type_=kNIMSignalingEventTypeCancelInvite
    /// </summary>
    public class NIMSignalingNotityInfoCancelInvite : NIMSignalingNotityProcessInvite
    {
        public override NIMSignalingEventType event_type_ { get { return NIMSignalingEventType.kNIMSignalingEventTypeCancelInvite; } }
    }

    /// <summary>
    /// 拒绝邀请事件通知信息，event_type_=kNIMSignalingEventTypeReject
    /// </summary>
    public class NIMSignalingNotityInfoReject : NIMSignalingNotityProcessInvite
    {
        public override NIMSignalingEventType event_type_ { get { return NIMSignalingEventType.kNIMSignalingEventTypeReject; } }
    }

    /// <summary>
    /// 接收邀请事件通知信息，event_type_=kNIMSignalingEventTypeAccept
    /// </summary>
    public class NIMSignalingNotityInfoAccept : NIMSignalingNotityProcessInvite
    {
        public override NIMSignalingEventType event_type_ { get { return NIMSignalingEventType.kNIMSignalingEventTypeAccept; } }
    }


    public static class Notification
    {
        public static NIMSignalingNotityInfo Create(IntPtr data)
        {
            if (data == IntPtr.Zero) return null;
            NIMSignalingNotityInfo info;
            var notification = NimUtility.Utilities.IntPtrToStructure<CNotification>(data);
            var notifyData = NimUtility.Utilities.IntPtrToStructure<CNotificationData>(notification.Data);
            switch(notifyData.event_type_)
            {
                case NIMSignalingEventType.kNIMSignalingEventTypeAccept:
                    info = new NIMSignalingNotityInfoAccept();
                    break;
                case NIMSignalingEventType.kNIMSignalingEventTypeCancelInvite:
                    info = new NIMSignalingNotityInfoCancelInvite();
                    break;
                case NIMSignalingEventType.kNIMSignalingEventTypeClose:
                    info = new NIMSignalingNotityInfoClose();
                    break;
                case NIMSignalingEventType.kNIMSignalingEventTypeCtrl:
                    info = new NIMSignalingNotityInfoControl();
                    break;
                case NIMSignalingEventType.kNIMSignalingEventTypeInvite:
                    info = new NIMSignalingNotityInfoInvite();
                    break;
                case NIMSignalingEventType.kNIMSignalingEventTypeJoin:
                    info = new NIMSignalingNotityInfoJoin();
                    break;
                case NIMSignalingEventType.kNIMSignalingEventTypeLeave:
                    info = new NIMSignalingNotityInfoLeave();
                    break;
                case NIMSignalingEventType.kNIMSignalingEventTypeReject:
                    info = new NIMSignalingNotityInfoReject();
                    break;
                default:
                    info = null;
                    break;
            }
            if(info != null)
            {
                info.SetNotification(data);
            }
            return info;
        }
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct CNotification
    {
        public IntPtr Data;
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    struct CJoinNotification
    {
        public IntPtr Data;
        public IntPtr Member; //NIMSignalingMemberInfo
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    struct CInviteProcessingNotification
    {
        public IntPtr Data;
        public IntPtr Account;
        public IntPtr RequestID;
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    struct CInviteNotification
    {
        public IntPtr Data;
        public IntPtr Account;
        public IntPtr RequestID;
        public IntPtr PushInfo; //
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
    struct CNotificationData
    {
        /// <summary>
        /// 通知类型
        /// </summary>
        public NIMSignalingEventType event_type_;
        /// <summary>
        /// 频道信息 
        /// </summary>
        public IntPtr channel_info_;
        /// <summary>
        /// 操作者
        /// </summary>
        public IntPtr from_account_id_;
        /// <summary>
        /// 操作的扩展字段
        /// </summary>
        public IntPtr custom_info_;

        public ulong msg_id_;
    }
}
