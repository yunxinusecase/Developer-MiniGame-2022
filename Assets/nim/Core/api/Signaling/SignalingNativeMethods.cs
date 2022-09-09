﻿using NimUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NIM.Signaling.Native
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NimSignalingNotifyCbFunc(IntPtr notify_info, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NimSignalingNotifyListCbFunc(IntPtr info_list, int size, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NimSignalingChannelsSyncCbFunc(IntPtr info_list, int size, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NimSignalingMembersSyncCbFunc(IntPtr detailed_info, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void NimSignalingOptCbFunc(int code, IntPtr opt_res_param, IntPtr user_data);

    class SignalingNativeMethods
    {
        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_reg_online_notify_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nim_signaling_reg_online_notify_cb(NimSignalingNotifyCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_reg_mutil_client_sync_notify_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nim_signaling_reg_mutil_client_sync_notify_cb(NimSignalingNotifyCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_reg_offline_notify_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nim_signaling_reg_offline_notify_cb(NimSignalingNotifyListCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_reg_channels_sync_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nim_signaling_reg_channels_sync_cb(NimSignalingChannelsSyncCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_reg_offline_notify_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nim_signaling_reg_members_sync_cb(NimSignalingMembersSyncCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nim_signaling_create_channel(IntPtr param, NimSignalingOptCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_create_channel", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nim_signaling_create_channel2(ref NIMSignalingCreateParam param, NimSignalingOptCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_close", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nim_signaling_close(IntPtr param, NimSignalingOptCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_join", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nim_signaling_join(IntPtr param, NimSignalingOptCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_leave", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nim_signaling_leave(IntPtr param, NimSignalingOptCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_call", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nim_signaling_call(IntPtr param, NimSignalingOptCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_invite", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nim_signaling_invite(IntPtr param, NimSignalingOptCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_cancel_invite", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void nim_signaling_cancel_invite(IntPtr param, NimSignalingOptCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_reject", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]

        internal static extern void nim_signaling_reject(IntPtr param, NimSignalingOptCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_accept", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]

        internal static extern void nim_signaling_accept(IntPtr param, NimSignalingOptCbFunc cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMNativeDLL, EntryPoint = "nim_signaling_control", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]

        internal static extern void nim_signaling_control(IntPtr param, NimSignalingOptCbFunc cb, IntPtr user_data);
    }

    /// <summary>
    /// 频道属性
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct NIMSignalingChannelInfo_C
    {
        /// <summary>
        /// 通话类型,1:音频;2:视频;3:其他
        /// </summary>
        public NIMSignalingType channel_type_;
        /// <summary>
        /// 创建时传入的频道名
        /// </summary>
        public IntPtr channel_name_;
        /// <summary>
        /// 服务器生成的频道id
        /// </summary>
        public IntPtr channel_id_;
        ///<summary>
        ///创建时传入的扩展字段
        ///</summary>  
        public IntPtr channel_ext_;
        /// <summary>
        /// 创建时间点
        /// </summary>
        public long create_timestamp_;
        /// <summary>
        /// 失效时间点
        /// </summary>
        public long expire_timestamp_;
        /// <summary>
        /// 创建者的accid 
        /// </summary>
        public IntPtr creator_id_;
        /// <summary>
        /// 频道是否有效
        /// </summary>
        public bool invalid_;

    }

    /// <summary>
    /// 频道的详细信息，包含频道信息及成员列表
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential)]
    struct NIMSignalingChannelDetailedinfo_C
    {
        /// <summary>
        /// 频道信息
        /// </summary>
        public NIMSignalingChannelInfo_C channel_info_;
        /// <summary>
        /// 频道内成员信息数组
        /// </summary>
	    public System.IntPtr members_;
        /// <summary>
        /// 频道内成员信息数组大小
        /// </summary>
        public int member_size_;
    }

    /// <summary>
    /// 事件通知信息基类
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class NIMSignalingNotityInfo_C
    {
        /// <summary>
        /// 通知类型
        /// </summary>
        public NIMSignalingEventType event_type_;
        /// <summary>
        /// 频道信息 
        /// </summary>
        public NIMSignalingChannelInfo_C channel_info_;
        /// <summary>
        /// 操作者
        /// </summary>
        public IntPtr from_account_id_;
        /// <summary>
        /// 操作的扩展字段
        /// </summary>
        public IntPtr custom_info_;


    }

    public class SignalHelper
    {
        public static NIMSignalingChannelInfo NIMSignalingChannelInfoFromC(NIMSignalingChannelInfo_C info_c)
        {
            NIMSignalingChannelInfo info = new NIMSignalingChannelInfo();

            info.channel_ext_ = Convert.ToString(Utf8StringMarshaler.GetInstance("").MarshalNativeToManaged(info_c.channel_ext_));
            info.channel_id_ = Convert.ToString(Utf8StringMarshaler.GetInstance("").MarshalNativeToManaged(info_c.channel_id_));
            info.channel_name_ = Convert.ToString(Utf8StringMarshaler.GetInstance("").MarshalNativeToManaged(info_c.channel_name_));
            info.channel_type_ = info_c.channel_type_;
            info.create_timestamp_ = info_c.create_timestamp_;
            info.creator_id_ = Convert.ToString(Utf8StringMarshaler.GetInstance("").MarshalNativeToManaged(info_c.creator_id_));
            info.expire_timestamp_ = info_c.expire_timestamp_;
            info.invalid_ = info_c.invalid_;



            return info;
        }

        public static NIMSignalingNotityInfo NIMSignalingNotityInfoFromC(NIMSignalingNotityInfo_C info_c)
        {
            NIMSignalingNotityInfo info = new NIMSignalingNotityInfo();
            //info.event_type_ = info_c.event_type_;
            info.from_account_id_ = Convert.ToString(Utf8StringMarshaler.GetInstance("").MarshalNativeToManaged(info_c.from_account_id_));
            info.custom_info_ = Convert.ToString(Utf8StringMarshaler.GetInstance("").MarshalNativeToManaged(info_c.custom_info_));
            info.channel_info_ = NIMSignalingChannelInfoFromC(info_c.channel_info_);
            return info;
        }
    }
}
