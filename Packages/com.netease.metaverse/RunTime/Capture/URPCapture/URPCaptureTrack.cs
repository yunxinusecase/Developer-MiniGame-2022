#if RENDERING_PIPE_LINE_URP

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using MetaVerse.FrameWork;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using Unity.Collections;

// if in URP Rendering Pipeline

namespace MetaVerse.FrameWork.Capture
{
    public class URPCaptureTrack : MonoBehaviour, ICaptureController
    {
        private Camera mCamera;
        private ICaptureSource mCaptureSource;
        private bool mEnableGPURead = false;
        private bool mEnableCapture = false;
        private int sliceBuffLength = 0;
        private byte[] allocBytes = null;
        private string TAG = "URPCaptureTrack";

        public void SetSource(ICaptureSource source)
        {
            mCaptureSource = source;
        }
        
        public void OnUpdate(float deltaTime)
        {
            if (!mEnableCapture)
                return;
            if (mCamera == null || _mPassCustomRtcBlitRenderTextureFeature == null)
                return;
            if (!mEnableGPURead || mCaptureSource == null || !mCaptureSource.CheckValidCapture())
                return;
            if (_mPassCustomRtcBlitRenderTextureFeature.GetRenderTexture == null)
                return;
            AsyncGPUReadback.Request(_mPassCustomRtcBlitRenderTextureFeature.GetRenderTexture, 0, TextureFormat.ARGB32, OnCompleteReadback);
        }

        public void SetCamera(Camera camera)
        {
            mCamera = camera;
        }

        public void EnableCapture(bool enable)
        {
            mEnableCapture = enable;
        }

        public void SetCaptureData(int fps, int width, int height)
        {
        }

        public void EnableGPURead(bool enable)
        {
            mEnableGPURead = enable;
        }

        private CustomRTCBlitRenderTextureFeature _mPassCustomRtcBlitRenderTextureFeature = null;

        public void SetBlitCaptureRenderPassFeature(CustomRTCBlitRenderTextureFeature feature)
        {
            _mPassCustomRtcBlitRenderTextureFeature = feature;
        }

        void OnCompleteReadback(AsyncGPUReadbackRequest request)
        {
            if (!mEnableCapture)
                return;
            if (request.hasError)
            {
                Debug.LogError("GPU read CallBack Error Happened:");
                return;
            }

            if (mCaptureSource == null)
                return;
            if (!mCaptureSource.CheckValidCapture())
                return;
            NativeArray<byte> array = request.GetData<byte>();
            var slice = new NativeSlice<byte>(array).SliceConvert<byte>();
            if (allocBytes == null || sliceBuffLength != slice.Length)
            {
                YXUnityLog.LogInfo(TAG, $"create new alloc bytes slice:{slice.Length}");
                sliceBuffLength = slice.Length;
                allocBytes = new byte[slice.Length];
            }

            slice.CopyTo(allocBytes);
            mCaptureSource.OnCaptureOneFrameOutPut(allocBytes, request.width, request.height);
        }
    }
}
#endif