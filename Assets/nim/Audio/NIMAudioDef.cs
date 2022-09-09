using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NIMAudio
{
	/// <summary>
	/// audio模块调用返回错误码
	/// </summary>
	public enum NIMAudioResCode
	{
		/// <summary>
		/// 成功
		/// </summary>
		kNIMAudioSuccess = 200,
		/// <summary>
		/// 操作失败
		/// </summary>
		kNIMAudioFailed = 100,
		/// <summary>
		/// 未初始化或未成功初始化
		/// </summary>
		kNIMAudioUninitError = 101,
		/// <summary>
		/// 正在播放中，操作失败
		/// </summary>
		kNIMAudioClientPlaying = 102,
		/// <summary>
		/// 正在采集中，操作失败
		/// </summary>
		kNIMAudioClientCapturing = 103,
		/// <summary>
		/// 采集设备初始化失败（e.g. 找不到mic设备）
		/// </summary>
		kNIMAudioCaptureDeviceInitError = 104,
		/// <summary>
		/// 采集或播放对象或操作不存在
		/// </summary>
		kNIMAudioClientNotExist = 105,
		/// <summary>
		/// 录音文件或转码文件保存失败
		/// </summary>
		kRecordFileError = 106, 
		/// <summary>
		/// 播放文件不存在
		/// </summary>
		kPlayFileNotExist = 107,

	}

	/// <summary>
	/// 音频编码方式
	/// </summary>
	public enum NIMAudioType
	{
		/// <summary>
		/// 音频AAC编码
		/// </summary>
		kNIMAudioAAC = 0,
		/// <summary>
		/// 音频AMR编码
		/// </summary>
		kNIMAudioAMR = 1,
	}

	public delegate void PlayCallbackDelegate(int resCode, string filePath);
	public delegate void StatusCallbackDelegate(int resCode);
	public delegate void StopCaptureCallbackDelegate(int resCode,string file_path, string file_ext, int file_size, int audio_duration);
	public delegate void GetCaptureDevicesCallbackDelegate(int resCode, List<string> devices);
    public delegate void CaptureVolumeCallbackDelegate(int volume);
}
