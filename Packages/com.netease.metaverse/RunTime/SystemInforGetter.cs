using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace MetaVerse.FrameWork
{
    public static class SystemInforGetter
    {
        private static string TAG = "SystemInforGetter";
        public static void GetPrintDeviceInformation()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SytemInformation:");
            sb.AppendLine($"deviceName: {SystemInfo.deviceName}");
            sb.AppendLine($"deviceType: {SystemInfo.deviceType}");
            sb.AppendLine($"deviceModel: {SystemInfo.deviceModel}");
            sb.AppendLine($"operatingSystem: {SystemInfo.operatingSystem}");
            sb.AppendLine($"systemMemorySize: {SystemInfo.systemMemorySize / 1024} GB");

            sb.AppendLine($"processorCount: {SystemInfo.processorCount}");
            sb.AppendLine($"processorType: {SystemInfo.processorType}");
            sb.AppendLine($"processorFrequency: {SystemInfo.processorFrequency}");

            sb.AppendLine($"graphicsDeviceID: {SystemInfo.graphicsDeviceID}");
            sb.AppendLine($"graphicsDeviceType: {SystemInfo.graphicsDeviceType}");
            sb.AppendLine($"graphicsDeviceName: {SystemInfo.graphicsDeviceName}");
            sb.AppendLine($"graphicsMemorySize: {SystemInfo.graphicsMemorySize} MB");
            sb.AppendLine($"graphicsMultiThreaded: {SystemInfo.graphicsMultiThreaded}");
            sb.AppendLine($"graphicsDeviceVersion: {SystemInfo.graphicsDeviceVersion}");

            sb.AppendLine($"maxTextureSize: {SystemInfo.maxTextureSize}");
            sb.AppendLine($"npotSupport: {SystemInfo.npotSupport}");

            sb.AppendLine($"supportsAsyncGPUReadback: {SystemInfo.supportsAsyncGPUReadback}");
            sb.AppendLine($"renderingThreadingMode: {SystemInfo.renderingThreadingMode}");
            sb.AppendLine($"Screen.refreshRate:  {Screen.currentResolution.refreshRate}");
            sb.AppendLine($"vSyncCount: {QualitySettings.vSyncCount}");
            sb.AppendLine($"Support RenderTextureFormat.BGRA32: {SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.BGRA32)}");
            sb.AppendLine($"target frameRate: {Application.targetFrameRate}");
            YXUnityLog.LogInfo(TAG, sb.ToString());
        }
    }
}
