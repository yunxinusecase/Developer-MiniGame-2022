/** @file NIMGlobalAPI.cs
  * @brief NIM SDK提供的一些全局接口
  * @copyright (c) 2015, NetEase Inc. All rights reserved
  * @author Harrison
  * @date 2015/12/8
  */

using System;
using System.Runtime.InteropServices;
using NimUtility;

namespace NIM
{
    
    public delegate void NimWriteLogDelegate(int level, string log);

    public delegate void NimNetworkDetectionDelegate(NetDetectionRes error, NetDetectResult result, IntPtr userData);

    public delegate void NimProxyDetectionDelegate(bool connection, NIMProxyDetectStep step);

    public delegate void DeleteCacheFileDelegate(ResponseCode code);

    public delegate void GetCacheFileInfoDelegate(CacheFileInfo info);

    public class GlobalAPI
    {
        /// <summary>
        ///     释放SDK内部分配的内存
        /// </summary>
        /// <param name="str">由SDK内部分配内存的字符串</param>
        public static void FreeStringBuffer(IntPtr str)
        {
            NIMGlobalNativeMethods.nim_global_free_str_buf(str);
        }

        /// <summary>
        ///     释放SDK内部分配的内存
        /// </summary>
        /// <param name="data">由SDK内部分配的内存</param>
        public static void FreeBuffer(IntPtr data)
        {
            NIMGlobalNativeMethods.nim_global_free_buf(data);
        }

        public static void SetSdkLogCallback(NimWriteLogDelegate cb)
        {
#if !UNITY_ANDROID
            IntPtr ptr = DelegateConverter.ConvertToIntPtr(cb);
            NIMGlobalNativeMethods.nim_global_reg_sdk_log_cb(null, NimSdkLogCb, ptr);
#endif
        }

        private static readonly NIMGlobalNativeMethods.nim_sdk_log_cb_func NimSdkLogCb = WriteSdkLog;

        [MonoPInvokeCallback(typeof(NIMGlobalNativeMethods.nim_sdk_log_cb_func))]
        private static void WriteSdkLog(int log_level, string log, IntPtr user_data)
        {
            DelegateConverter.Invoke<NimWriteLogDelegate>(user_data, log_level, log);
        }

    }
}