using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NIMAudio
{ 
    public static class NIMAudio
    {
        /// <summary>
        /// 操作结果回调
        /// </summary>
        /// <param name="resCode">操作结果，一切正常200</param>
        /// <param name="filePath">播放文件绝对路径</param>
		/// <param name="user_data">用户自定义数据</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ResCodeIdCb(int resCode,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(NimUtility.Utf8StringMarshaler))]string filePath,
			IntPtr user_data);

		/// <summary>
		/// 操作结果回调
		/// </summary>
		/// <param name="resCode">操作结果，一切正常200</param>
		/// <param name="user_data>用户自定义数据</param>
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void NIMRESCodeCb(int resCode,IntPtr user_data);

		/// <summary>
		/// 获取采集设备回调
		/// </summary>
		/// <param name="rescode"></param>
		/// <param name="device_list"></param>
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void NIMEnumCaptureDevicesCb(int rescode,
		[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(NimUtility.Utf8StringMarshaler))]string device_list,
			IntPtr user_data);

		/// <summary>
		/// 操作回调
		/// </summary>
		/// <param name="resCode">操作结果，一切正常200</param>
		/// <param name="file_path">文件绝对路径</param>
		/// <param name="file_ext">文件扩展名</param>
		/// <param name="file_size">文件大小</param>
		/// <param name="audio_duration">语音时长</param>
		/// <param name="user_data">用户自定义数据</param>
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void NIMStopCaptureCb(int resCode,
			[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(NimUtility.Utf8StringMarshaler))]string file_path,
			[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(NimUtility.Utf8StringMarshaler))]string file_ext,
			int file_size,
			int audio_duration,
			IntPtr user_data);

        /// <summary>
        /// 音量值回调
        /// </summary>
        /// <param name="volume">音量值</param>
        /// <param name="json_extension">无效拓展参数</param>
        /// <param name="user_data">用户自定义数据</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void NIMCaptureVolumeCb(int volume,
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(NimUtility.Utf8StringMarshaler))]string json_extension,
            IntPtr user_data);
	}

    class AudioNativeMethods
    {
		//引用C中的方法
#region NIM Audio C SDK native methods
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
#else
		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_init_module", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool nim_audio_init_module(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(NimUtility.Utf8StringMarshaler))]string user_data_parent_path);

        [DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_uninit_module", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool nim_audio_uninit_module();

        [DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_play_audio", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool nim_audio_play_audio(
            [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(NimUtility.Utf8StringMarshaler))]string filePath, int format);

        [DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_stop_play_audio", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool nim_audio_stop_play_audio();

		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_start_capture", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_start_capture(
			int audio_format,
			int volume,
			int loudness,
			[MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(NimUtility.Utf8StringMarshaler))] string capture_device
			);


		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_stop_capture", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_stop_capture();

		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_cancel_audio", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_cancel_audio();



		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_reg_start_play_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool nim_audio_reg_start_play_cb(NIMAudio.ResCodeIdCb cb, IntPtr user_data);

        [DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_reg_stop_play_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool nim_audio_reg_stop_play_cb(NIMAudio.ResCodeIdCb cb,IntPtr user_data);

		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_reg_play_end_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_reg_play_end_cb(NIMAudio.ResCodeIdCb cb, IntPtr user_data);

		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_reg_start_capture_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_reg_start_capture_cb(NIMAudio.NIMRESCodeCb cb, IntPtr user_data);


		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_reg_stop_capture_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_reg_stop_capture_cb(NIMAudio.NIMStopCaptureCb cb, IntPtr user_data);

		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_reg_cancel_audio_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_reg_cancel_audio_cb(NIMAudio.NIMRESCodeCb cb, IntPtr user_data);

		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_reg_get_play_current_position_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_reg_get_play_current_position_cb(NIMAudio.NIMRESCodeCb cb, IntPtr user_data);

		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_reg_get_capture_time_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_reg_get_capture_time_cb(NIMAudio.NIMRESCodeCb cb, IntPtr user_data);

		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_reg_enum_capture_device_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_reg_enum_capture_device_cb(NIMAudio.NIMEnumCaptureDevicesCb cb,IntPtr ptr);

		/// <summary>
		/// 获取当前采集的时长
		/// </summary>
		/// <returns></returns>
		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_get_capture_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_get_capture_time();

		/// <summary>
		/// 获取播放进度
		/// </summary>
		/// <returns></returns>
		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_get_play_current_position", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_get_play_current_position();

		/// <summary>
		/// 获取播放文件的时长
		/// </summary>
		/// <returns></returns>
		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_get_play_time", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int nim_audio_get_play_time();

		/// <summary>
		/// 设置扬声器播放,
		/// </summary>
		/// <param name="speaker"></param>
		/// <param name="capture">capture 为ture,表示音频采集状态下设置扬声器，capture为false,表示音频播放状态下设置扬声器</param>
		/// <returns></returns>
		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_set_play_speaker", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern int nim_audio_set_play_speaker(bool speaker,IntPtr context);

		/// <summary>
		/// 获取扬声器状态
		/// </summary>
		/// <returns></returns>
		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_get_play_speaker", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_get_play_speaker(IntPtr context);

		/// <summary>
		/// 枚举采集设备
		/// </summary>
		/// <returns></returns>
		[DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_enum_capture_device", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		internal static extern bool nim_audio_enum_capture_device();


        [DllImport(NIM.NativeConfig.NIMAudioNativeDLL, EntryPoint = "nim_audio_reg_capture_volume_cb", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        internal static extern bool nim_audio_reg_capture_volume_cb(NIMAudio.NIMCaptureVolumeCb cb, IntPtr user_data);
#endif
        #endregion
    }
}
