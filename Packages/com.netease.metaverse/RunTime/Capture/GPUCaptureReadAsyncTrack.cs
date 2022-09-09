using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using Unity.Collections;
using System;
using UnityEngine.Experimental.Rendering;
using MetaVerse.FrameWork;

namespace MetaVerse.FrameWork.Capture
{
    public class GPUCaptureReadAsyncTrack : MonoBehaviour, ICaptureController
    {
        // 
        // Use OnRenderImage MonoBehaviour
        // 

        private int sliceBuffLength = 0;
        private byte[] allocBytes = null;

        private Material mRenderMaterial;
        private RenderTexture mFlipRt;

        private string TAG = "GPUReadAsyncManager";
        private bool _mEnabledGpuReadSwitch = false;

        // GPU´ó¿ª¹Ø
        public bool EnabledGPUReadSwitch => _mEnabledGpuReadSwitch;

        private bool mSupportGPURead = false;
        public bool SupportGPURead => mSupportGPURead;
        private Camera mCamera = null;
        private float mReadInterval = 1f / 30f;
        private int mCaptureWidth = 0;
        private int mCaptureHeight = 0;
        private ICaptureSource mCaptureSource;

        public void SetSource(ICaptureSource source)
        {
            mCaptureSource = source;
        }

        private bool isWorking = false;

        void Awake()
        {
            mSupportGPURead = SystemInfo.supportsAsyncGPUReadback;
            mRenderMaterial = Resources.Load<Material>("Unlit_CaptureBlit");
            YXUnityLog.LogInfo(TAG, "mSupportGPURead: " + mSupportGPURead);
        }

        public void OnUpdate(float deltaTime)
        {
            if (mRenderTexture == null || mCommandBuffer == null)
                return;
            if (!SupportGPURead || !EnabledGPUReadSwitch || mCaptureSource == null || !mCaptureSource.CheckValidCapture())
                return;
            AsyncGPUReadback.Request(mRenderTexture, 0, TextureFormat.ARGB32, OnCompleteReadback);
        }

        protected void OnEnable()
        {
            CaptureEventsDefine.Event_OnCameraRendered += OnCameraRenderImage;
        }

        private void OnDisable()
        {
            CaptureEventsDefine.Event_OnCameraRendered -= OnCameraRenderImage;
        }


        private CommandBuffer mCommandBuffer = null;
        private RenderTexture mRenderTexture = null;

        public void SetCamera(Camera camera)
        {
            if (camera == null)
                return;
            if (camera == mCamera)
                return;
            mCamera = camera;

            // https://forum.unity.com/threads/graphics-blit-vs-grabpass-vs-rendertexture.523825/
            // if Use OnRenderImage
            RecordCamera rc = mCamera == null ? null : mCamera.GetComponent<RecordCamera>();
            if (rc != null)
                rc.enabled = false;
            mCamera = camera;
            rc = mCamera.GetComponent<RecordCamera>();
            if (rc == null)
            {
                rc = mCamera.gameObject.AddComponent<RecordCamera>();
                rc.enabled = false;
            }

            // if Use Command Buffer
            //if (mCommandBuffer != null)
            //{
            //    mCommandBuffer.Release();
            //    mCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, mCommandBuffer);
            //    mCommandBuffer = null;
            //}

            //if (mRenderTexture != null)
            //{
            //    GameObject.Destroy(mRenderTexture);
            //    mRenderTexture = null;
            //}
            //mCamera.RemoveAllCommandBuffers();
            //int width = Screen.width;
            //int hegith = Screen.height;
            //mRenderTexture = new RenderTexture(width, hegith, 16, RenderTextureFormat.ARGB32);
            //mCommandBuffer = new CommandBuffer();
            //mCommandBuffer.name = "Blit FrameBuffer";
            //mCommandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, mRenderTexture);
            //mCamera.AddCommandBuffer(CameraEvent.AfterEverything, mCommandBuffer);
        }

        public void EnableCapture(bool enable)
        {
            isWorking = enable;
            EnableCamera(enable);
        }

        private void EnableCamera(bool enable)
        {
            if (mCamera != null)
            {
                RecordCamera rc = mCamera.gameObject.GetComponent<RecordCamera>();
                if (rc != null)
                    rc.enabled = enable;
                YXUnityLog.LogInfo(TAG, $"EnableGPURead() enable: {enable.ToString()}");
            }
        }

        public void SetCaptureData(int fps, int width, int height)
        {
            mReadInterval = 1f / fps;
            mCaptureWidth = width;
            mCaptureHeight = height;
        }

        public void EnableGPURead(bool enable)
        {
            _mEnabledGpuReadSwitch = enable;
            EnableCamera(enable);
        }

        private int renderCount = 0;
        private void OnCameraRenderImage(RenderTexture src)
        {
            if (!isWorking || !SupportGPURead || !EnabledGPUReadSwitch || mCaptureSource == null || !mCaptureSource.CheckValidCapture())
                return;
            YXUnityLog.LogInfo(TAG, "create image");
            RenderTexture desrt = CreateRT(src.width, src.height, src);
            Graphics.Blit(src, desrt, mRenderMaterial);

            renderCount++;

            YXUnityLog.LogInfo(TAG, "create image rendercount: " + renderCount);

            AsyncGPUReadback.Request(desrt, 0, TextureFormat.ARGB32, OnCompleteReadback);
        }

        private RenderTexture CreateRT(int width, int height, RenderTexture rt)
        {
            bool create = false;
            if (mFlipRt == null)
                create = true;
            else if (mFlipRt.width != width || mFlipRt.height != height)
            {
                GameObject.Destroy(mFlipRt);
                mFlipRt = null;
                create = true;
            }

            if (create)
            {
                // On Windows-> rt.format = R16G16B16A16_SFloat
                // On Android-> rt.format = R8G8B8A8_SRGB
                // On IOS-> rt.format = R8G8B8A8_SRGB
                // RTC Push Buffer is ARGB32 so just create ARGB Target Format
                // mFlipRt = new RenderTexture(width, height, 0, rt.format);
                mFlipRt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
            }

            return mFlipRt;
        }

        void OnCompleteReadback(AsyncGPUReadbackRequest request)
        {
            renderCount--;
            if (request.hasError)
            {
                YXUnityLog.LogInfo(TAG, "GPU read CallBack Error Happened:");
                return;
            }

            if (!isWorking)
            {
                YXUnityLog.LogInfo(TAG, "Not Working");
                return;
            }

            if (mCaptureSource == null)
            {
                YXUnityLog.LogInfo(TAG, "Source Null");
                return;
            }

            if (!mCaptureSource.CheckValidCapture())
            {
                YXUnityLog.LogInfo(TAG, "InValid Capture");
                return;
            }

            YXUnityLog.LogInfo(TAG, "start");
            NativeArray<byte> array = request.GetData<byte>();
            var slice = new NativeSlice<byte>(array).SliceConvert<byte>();
            if (allocBytes == null || sliceBuffLength != slice.Length)
            {
                YXUnityLog.LogInfo(TAG, $"create new alloc bytes slice:{slice.Length}");
                sliceBuffLength = slice.Length;
                allocBytes = new byte[slice.Length];
            }
            YXUnityLog.LogInfo(TAG, "end");

            slice.CopyTo(allocBytes);
            YXUnityLog.LogInfo(TAG, "end end");

            mCaptureSource.OnCaptureOneFrameOutPut(allocBytes, request.width, request.height);
            YXUnityLog.LogInfo(TAG, "end end end");
        }
    }
}

