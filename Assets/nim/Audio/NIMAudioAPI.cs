/** @file NIMAudioAPI.cs
  * @brief NIM 提供的语音录制和播放接口
  * @copyright (c) 2015, NetEase Inc. All rights reserved
  * @author Harrison
  * @date 2015/12/8
  */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using NIM;

namespace NIMAudio
{

	public class AudioAPI
    {
        private static bool _initialized = false;

		private static readonly NIMAudio.ResCodeIdCb OnStartPlayCallback = StartPlayCallback;
		private static readonly NIMAudio.ResCodeIdCb OnStopPlayCallback = StopPlayCallback;
		private static readonly NIMAudio.ResCodeIdCb OnPlayEndCallback = PlayEndCallback;
		private static readonly NIMAudio.NIMRESCodeCb OnAudioErrorCallback = AudioErrorCallback;

		private static readonly NIMAudio.NIMRESCodeCb OnStartCaptureCallback = StartCaptureCallback;
		private static readonly NIMAudio.NIMStopCaptureCb OnStopCaptureCallback = StopCaptureCallback;
		private static readonly NIMAudio.NIMRESCodeCb OnCancelCaptureCallback = CancelCaptureCallback;
		private static readonly NIMAudio.NIMRESCodeCb OnGetCaptureTimeCallback = GetCaptureTimeCallback;
		private static readonly NIMAudio.NIMRESCodeCb OnGetPlayCurrentPosition = GetPlayCurrentPositionCallback;
		private static readonly NIMAudio.NIMEnumCaptureDevicesCb OnGetCaptureDevices = GetCaptureDevicesCallback;
        private static readonly NIMAudio.NIMCaptureVolumeCb OnCaptureVolume = AudioVolumeCallback;

		/// <summary>
		/// NIM SDK 初始化语音模块
		/// </summary>
		/// <param name="userDataParentPath">缓存目录</param>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool InitModule(string userDataParentPath)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			return _initialized = AudioNativeMethods.nim_audio_init_module(userDataParentPath);
#endif
        }

        /// <summary>
        /// NIM SDK 卸载语音模块（只有在主程序关闭时才有必要调用此接口）
        /// </summary>
        /// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
        public static bool UninitModule()
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			_initialized = false;
            return AudioNativeMethods.nim_audio_uninit_module();
#endif
        }

		/// <summary>
		/// NIM SDK 播放,通过回调获取开始播放状态。android平台需在主线程调用
		/// </summary>
		/// <param name="filePath">播放文件绝对路径</param>
		/// <param name="audioFormat">播放音频格式，AAC : 0， AMR : 1</param>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool PlayAudio(string filePath, NIMAudioType audioFormat)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			NimUtility.Log.Info("nim_audio api _initialized->" + _initialized.ToString());
			if (!_initialized)
				return false;
            return AudioNativeMethods.nim_audio_play_audio(filePath,(int) audioFormat);
#endif
			//             if (!_initialized)
			//                 throw new Exception("nim audio moudle uninitialized!");

		}

		/// <summary>
		/// NIM SDK 停止播放,通过回调获取停止播放状态
		/// </summary>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool StopPlayAudio()
		{
#if  UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			if (!_initialized)
				return false;
			return AudioNativeMethods.nim_audio_stop_play_audio();
#endif
		}

		/// <summary>
		/// NIM SDK 注册播放开始事件回调
		/// </summary>
		/// <param name="cb">播放开始事件的回调函数</param>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool RegStartPlayCb(PlayCallbackDelegate cb)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			if (!_initialized)
				return false;
			var ptr = NimUtility.DelegateConverter.ConvertToIntPtr(cb);
            return AudioNativeMethods.nim_audio_reg_start_play_cb(OnStartPlayCallback,ptr);
#endif
        }

        /// <summary>
        /// NIM SDK 注册停止播放事件回调
        /// </summary>
        /// <param name="cb">播放结束事件的回调函数</param>
        /// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
        public static bool RegStopPlayCb(PlayCallbackDelegate cb)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			if (!_initialized)
				return false;
			var ptr = NimUtility.DelegateConverter.ConvertToIntPtr(cb);
			return AudioNativeMethods.nim_audio_reg_stop_play_cb(OnStopPlayCallback,ptr);
#endif
        }

		/// <summary>
		/// NIM SDK 注册播放结束事件回调
		/// </summary>
		/// <param name="cb">播放结束事件的回调函数</param>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool RegPlayEndCb(PlayCallbackDelegate cb)
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			if (!_initialized)
				return false;
			var ptr = NimUtility.DelegateConverter.ConvertToIntPtr(cb);
			return AudioNativeMethods.nim_audio_reg_play_end_cb(OnPlayEndCallback,ptr);
#endif
		}

		public static bool RegGetCaptureDevices(GetCaptureDevicesCallbackDelegate cb)
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			if (!_initialized)
				return false;
			var ptr = NimUtility.DelegateConverter.ConvertToIntPtr(cb);
			return AudioNativeMethods.nim_audio_reg_enum_capture_device_cb(GetCaptureDevicesCallback,ptr);
#endif
		}


		/// <summary>
		/// 录制语音 android平台需在主线程调用
		/// </summary>
		/// <param name="audio_format">音频格式，AAC : 0， AMR : 1</param>
		/// <param name="volume">音量(0 - 500, 默认100)增益值为(volume/100),值为0时,sdk底层已默认值处理</param>
		/// <param name="loudness">默认0   pc有效</param>
		/// <param name="capture_device">capture_device 录音设备 pc有效</param>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool StartCapture(int audio_format,int volume,int loudness,string capture_device)
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			if (!_initialized)
				return false;
			return AudioNativeMethods.nim_audio_start_capture(audio_format, volume,loudness,capture_device);
#endif
		}

		/// <summary>
		/// 停止录制语音
		/// </summary>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool StopCapture()
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			if (!_initialized)
				return false;
			return AudioNativeMethods.nim_audio_stop_capture();
#endif
		}

		/// <summary>
		/// 取消录制并删除临时文件
		/// </summary>
		/// <param name="filepath"></param>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool CancelCapture()
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			if (!_initialized)
				return false;
			return AudioNativeMethods.nim_audio_cancel_audio();
#endif
		}

		/// <summary>
		/// 注册录制语音开始回调
		/// </summary>
		/// <param name="cb">回调函数</param>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool RegStartCaptureCb(StatusCallbackDelegate cb)
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			if (!_initialized)
				return false;
			var ptr = NimUtility.DelegateConverter.ConvertToIntPtr(cb);
			return AudioNativeMethods.nim_audio_reg_start_capture_cb(OnStartCaptureCallback, ptr);
#endif
		}

		/// <summary>
		/// 注册录制语音停止回调
		/// </summary>
		/// <param name="cb">回调函数</param>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool RegStopCaptureCb(StopCaptureCallbackDelegate cb)
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			if (!_initialized)
				return false;
			var ptr = NimUtility.DelegateConverter.ConvertToIntPtr(cb);
			return AudioNativeMethods.nim_audio_reg_stop_capture_cb(OnStopCaptureCallback, ptr);
#endif
		}

		/// <summary>
		/// 注册录制语音取消回调
		/// </summary>
		/// <param name="cb">回调函数</param>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool RegCancelCaptureCb(StatusCallbackDelegate cb)
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			if (!_initialized)
				return false;
			var ptr = NimUtility.DelegateConverter.ConvertToIntPtr(cb);
			return AudioNativeMethods.nim_audio_reg_cancel_audio_cb(OnCancelCaptureCallback, ptr);
#endif
		}

		/// <summary>
		/// 注册获取当前录制时间的回调
		/// </summary>
		/// <param name="cb"></param>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool RegGetCaptureTimeCb(StatusCallbackDelegate cb)
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			if (!_initialized)
				return false;
			var ptr = NimUtility.DelegateConverter.ConvertToIntPtr(cb);
			return AudioNativeMethods.nim_audio_reg_get_capture_time_cb(OnGetCaptureTimeCallback, ptr);
#endif
		}

		/// <summary>
		/// 注册获取当前播放时间的回调
		/// </summary>
		/// <param name="cb"></param>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool RegGetPlayCurrentPositionCb(StatusCallbackDelegate cb)
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			if (!_initialized)
				return false;
			var ptr = NimUtility.DelegateConverter.ConvertToIntPtr(cb);
			return AudioNativeMethods.nim_audio_reg_get_play_current_position_cb(OnCancelCaptureCallback, ptr);
#endif
		}

		/// <summary>
		/// 获取采集时间，采集时间由所注册的回调返回
		/// </summary>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool GetCaptureTime()
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			return AudioNativeMethods.nim_audio_get_capture_time();
#endif
		}

		/// <summary>
		/// 获取播放文件的时长
		/// </summary>
		/// <returns>播放文件的时长，异常为-1</returns>
		public static int GetPlayTime()
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return -1;
#else
			return AudioNativeMethods.nim_audio_get_play_time();
#endif
		}

		/// <summary>
		/// 获取播放时间，播放时间由所注册的回调返回
		/// </summary>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool GetPlayCurrentPosition()
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			return AudioNativeMethods.nim_audio_get_play_current_position();
#endif
		}

		/// <summary>
		///  设置扬声器 ios,android有效
		/// </summary>
		/// <param name="speaker">true:扬声器开启.false:扬声器关闭</param>
		/// <param name="context">当前上下文，android 必须.ios无效</param>
		public static void SetPlaySpeaker(bool speaker,IntPtr context)
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return ;
#else
			AudioNativeMethods.nim_audio_set_play_speaker(speaker,context);
#endif
		}

		/// <summary>
		/// 获取扬声器状态 ios，android有效
		/// </summary>
		/// <param name="context">当前上下文，android 必须.ios无效</param>
		/// <returns><c>true</c> 扬声器开启 <c>false</c> 扬声器关闭</returns>
		public static bool GetPlaySpeaker(IntPtr context)
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			return AudioNativeMethods.nim_audio_get_play_speaker(context);
#endif
		}

		/// <summary>
		/// 枚举采集设备，结果由所注册回调返回。
		/// </summary>
		/// <returns><c>true</c> 调用成功, <c>false</c> 调用失败</returns>
		public static bool GetCaptureDevices()
		{
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
			return AudioNativeMethods.nim_audio_enum_capture_device();
#endif
		}

        /// <summary>
        /// 设置音量值回调
        /// </summary>
        /// <param name="cb">回调函数</param>
        /// <returns></returns>
        public static bool RegCaptureVolumeCb(CaptureVolumeCallbackDelegate cb)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
			return false;
#else
            var ptr = NimUtility.DelegateConverter.ConvertToIntPtr(cb);
            return AudioNativeMethods.nim_audio_reg_capture_volume_cb(OnCaptureVolume, ptr);
#endif
        }


		[MonoPInvokeCallback(typeof(NIMAudio.NIMRESCodeCb))]
		static void StartCaptureCallback(int resCode, IntPtr user_data)
		{
			NimUtility.DelegateConverter.Invoke<StatusCallbackDelegate>(user_data, resCode);
		}

		[MonoPInvokeCallback(typeof(NIMAudio.NIMStopCaptureCb))]
		static void StopCaptureCallback(int resCode, string file_path, string file_ext, int file_size, int audio_duration,IntPtr user_data)
		{
			NimUtility.DelegateConverter.Invoke<StopCaptureCallbackDelegate>(user_data, resCode,file_path,file_ext,file_size,audio_duration);
		}

		[MonoPInvokeCallback(typeof(NIMAudio.NIMRESCodeCb))]
		static void CancelCaptureCallback(int resCode, IntPtr user_data)
		{
			NimUtility.DelegateConverter.Invoke<StatusCallbackDelegate>(user_data, resCode);
		}

		[MonoPInvokeCallback(typeof(NIMAudio.NIMRESCodeCb))]
		static void GetCaptureTimeCallback(int resCode, IntPtr user_data)
		{
			NimUtility.DelegateConverter.Invoke<StatusCallbackDelegate>(user_data, resCode);
		}

		[MonoPInvokeCallback(typeof(NIMAudio.NIMRESCodeCb))]
		static void GetPlayCurrentPositionCallback(int resCode, IntPtr user_data)
		{
			NimUtility.DelegateConverter.Invoke<StatusCallbackDelegate>(user_data, resCode);
		}

		[MonoPInvokeCallback(typeof(NIMAudio.NIMEnumCaptureDevicesCb))]
		static void GetCaptureDevicesCallback(int resCode, string device,IntPtr user_data)
		{
			List<string> devices = null;
			if (!String.IsNullOrEmpty(device))
				devices = device.Split(';').ToList<string>();
			NimUtility.DelegateConverter.Invoke<GetCaptureDevicesCallbackDelegate>(user_data, resCode,devices);
		}

		[MonoPInvokeCallback(typeof(NIMAudio.ResCodeIdCb))]
		static void StartPlayCallback(int resCode,string filePath,IntPtr user_data)
		{
			NimUtility.DelegateConverter.Invoke<PlayCallbackDelegate>(user_data,resCode,filePath);
		}

		[MonoPInvokeCallback(typeof(NIMAudio.ResCodeIdCb))]
		static void StopPlayCallback(int resCode, string filePath, IntPtr user_data)
		{
			NimUtility.DelegateConverter.Invoke<PlayCallbackDelegate>(user_data, resCode, filePath);
		}

		[MonoPInvokeCallback(typeof(NIMAudio.ResCodeIdCb))]
		static void PlayEndCallback(int resCode, string filePath,  IntPtr user_data)
		{
			NimUtility.DelegateConverter.Invoke<PlayCallbackDelegate>(user_data, resCode, filePath);
		}

		[MonoPInvokeCallback(typeof(NIMAudio.NIMRESCodeCb))]
		static void AudioErrorCallback(int resCode,IntPtr user_data)
		{
			NimUtility.DelegateConverter.Invoke<StatusCallbackDelegate>(user_data, resCode);
		}


        [MonoPInvokeCallback(typeof(NIMAudio.NIMCaptureVolumeCb))]
        static void AudioVolumeCallback(int volume,string json,IntPtr user_data)
        {
            NimUtility.DelegateConverter.Invoke<CaptureVolumeCallbackDelegate>(user_data, volume);
        }
    }
}
