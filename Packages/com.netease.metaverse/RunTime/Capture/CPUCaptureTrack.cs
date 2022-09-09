using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.Collections;

namespace MetaVerse.FrameWork.Capture
{
    public interface ICaptureSource
    {
        void OnCaptureOneFrameOutPut(byte[] bytes, int width, int height);
        bool CheckValidCapture();
    }

    public interface ICaptureController
    {
        void SetSource(ICaptureSource source);
        void EnableCapture(bool enable);

        void OnUpdate(float deltaTime);
        void SetCaptureData(int fps, int width, int height);
    }

    public class CPUCaptureTrack : MonoBehaviour, ICaptureController
    {
        public enum CaptureType
        {
            // 手动调用Caemra.Render()
            ManualRender,

            // 直接使用Camera.TargetRenderTexture
            TargetRenderTexture,
        }

        /* capture and decode data*/

        private static string TAG = "CaptureSystem";
        private Texture2D mCaptureBackUpTexture2D;
        private Material mRenderMaterial;

        [Header("Capture Config")] private int mCaptureWidth;
        private int mCaptureHeight;
        private Camera mCaptureCamera;
        private bool mDebugMode = false;
        private CaptureType mCaptureType = CaptureType.ManualRender;

        private ICaptureSource mCaptureSource;

        private Queue<byte[]> mCaptureFrameBytesQueue = new Queue<byte[]>(10);

        public void SetSource(ICaptureSource source)
        {
            mCaptureSource = source;
        }

        private Texture2D CreateTexture2D(int width, int height, TextureFormat format)
        {
            bool needcreate = false;
            if (mCaptureBackUpTexture2D == null || mCaptureBackUpTexture2D.width != width || mCaptureBackUpTexture2D.height != height)
                needcreate = true;
            if (needcreate)
            {
                if (mCaptureBackUpTexture2D != null)
                {
                    GameObject.Destroy(mCaptureBackUpTexture2D);
                }

                mCaptureBackUpTexture2D = new Texture2D(width, height, format, false);
                int id = mCaptureBackUpTexture2D.GetInstanceID();
                mCaptureBackUpTexture2D.name = "Temp-Texture-2d-" + id.ToString();

                YXUnityLog.LogInfo(TAG, "CreateTexture2D() create new Texture2D");
            }

            return mCaptureBackUpTexture2D;
        }

        private RenderTextureFormat mSupportRenderTextureFormat = RenderTextureFormat.ARGB32;
        private TextureFormat mSupportTexture2DFormat = TextureFormat.BGRA32;

        void Start()
        {
        }

        void Awake()
        {
            mRenderMaterial = Resources.Load<Material>("Unlit_CaptureBlit");
        }

        public bool InitCaptureSystem(
            CaptureType captureType,
            bool debugMode,
            Camera targetCamera,
            int captureWidth,
            int captureHeight,
            RenderTextureFormat textureFormat)
        {
            Debug.Assert(textureFormat == RenderTextureFormat.BGRA32 || textureFormat == RenderTextureFormat.ARGB32);
            mSupportRenderTextureFormat = textureFormat;
            mSupportTexture2DFormat = mSupportRenderTextureFormat == RenderTextureFormat.ARGB32
                ? TextureFormat.ARGB32
                : TextureFormat.BGRA32;
            YXUnityLog.LogInfo(TAG, $"mSupportRenderTextureFormat: {mSupportRenderTextureFormat.ToString()}, Texture2D Format: {mSupportTexture2DFormat.ToString()}");
            // check texture format support
            mCaptureCamera = targetCamera;
            mCaptureHeight = captureHeight;
            mCaptureWidth = captureWidth;
            mDebugMode = debugMode;
            mCaptureType = captureType;
            CheckCaptureCameraRenderTexture(captureWidth, captureHeight, mCaptureType, mCaptureCamera);
            YXUnityLog.LogInfo(TAG, $"InitCaptureSystem() width:{mCaptureWidth},height:{mCaptureHeight}");
            if (mCaptureCamera == null)
            {
                YXUnityLog.LogError(TAG, "InitCaptureSystem() Input Camera is Null...");
            }
            else
            {
                if (mCaptureType == CaptureType.TargetRenderTexture && mCaptureCamera.targetTexture == null)
                {
                    YXUnityLog.LogError(TAG, "InitCaptureSystem() Input Camera's TargetRenderTexture shouldn't be Null in CaptureType.TargetRenderTexture Mode");
                }
            }

            return true;
        }

        public void ModifyCaptureConfig(
            CaptureType captureType,
            bool debugMode,
            int captureWidth,
            int captureHeight)
        {
            mCaptureHeight = captureHeight;
            mCaptureWidth = captureWidth;
            mDebugMode = debugMode;
            mCaptureType = captureType;
            CheckCaptureCameraRenderTexture(captureWidth, captureHeight, mCaptureType, mCaptureCamera);
            if (mCaptureCamera == null)
            {
                YXUnityLog.LogError(TAG, "ModifyCaptureConfig() Input Camera is Null...");
            }
            else
            {
                if (mCaptureType == CaptureType.TargetRenderTexture && mCaptureCamera.targetTexture == null)
                {
                    YXUnityLog.LogError(TAG, "ModifyCaptureConfig() Input Camera's TargetRenderTexture shouldn't be Null in CaptureType.TargetRenderTexture Mode");
                }
            }
        }

        private void CheckCaptureCameraRenderTexture(int width, int height, CaptureType type, Camera camera)
        {
            if (camera == null)
            {
                YXUnityLog.LogError(TAG, "Camera is Null");
            }
            else
            {
                if (type == CaptureType.TargetRenderTexture)
                {
                    bool recreate = false;
                    if (camera.targetTexture == null || camera.targetTexture.width != width || camera.targetTexture.height != height)
                        recreate = true;
                    if (recreate)
                    {
                        RenderTexture prert = camera.targetTexture;
                        if (prert != null)
                            GameObject.Destroy(prert);

                        RenderTexture rt = new RenderTexture(width, height, 16, mSupportRenderTextureFormat, RenderTextureReadWrite.Default);
                        camera.targetTexture = rt;
                    }
                }
            }
        }

        private int sliceBuffLength = 0;
        private byte[] allocBytes = null;

        private byte[] GetTextureBuffer(Texture2D texture)
        {
            if (texture == null)
                return null;
            UnityEngine.Profiling.Profiler.BeginSample("getrawData capture");
            NativeArray<byte> array = texture.GetRawTextureData<byte>();
            var slice = new NativeSlice<byte>(array).SliceConvert<byte>();
            if (allocBytes == null || sliceBuffLength != slice.Length)
            {
                Debug.Log($"create new alloc bytes slice:{slice.Length}");
                sliceBuffLength = slice.Length;
                allocBytes = new byte[slice.Length];
            }

            slice.CopyTo(allocBytes);
            UnityEngine.Profiling.Profiler.EndSample();
            return allocBytes;
        }

        public void OnUpdate(float deltaTime)
        {
            if (mCaptureSource == null || !mCaptureSource.CheckValidCapture())
                return;
            DoCapture();
            byte[] bytes = DequeCaptureFrameData();
            mCaptureSource.OnCaptureOneFrameOutPut(bytes, mCaptureWidth, mCaptureHeight);
        }

        public void EnableCapture(bool enable)
        {
            this.enabled = enable;
        }

        public void SetCaptureData(int fps, int width, int height)
        {
            YXUnityLog.LogInfo(TAG, $"fps:{fps}, width:{width}, height: {height}");
            ModifyCaptureConfig(mCaptureType, false, width, height);
        }

        public void DoCapture()
        {
            if (mCaptureCamera == null)
            {
                return;
            }

            // just save one frame
            if (mCaptureFrameBytesQueue.Count >= 0)
                mCaptureFrameBytesQueue.Clear();

            if (mCaptureType == CaptureType.ManualRender)
            {
                RenderTexture preactive = RenderTexture.active;
                RenderTexture rt = RenderTexture.GetTemporary(mCaptureWidth, mCaptureHeight, 16, mSupportRenderTextureFormat);
                mCaptureCamera.targetTexture = rt;
                mCaptureCamera.Render();

                RenderTexture desrt = RenderTexture.GetTemporary(mCaptureWidth, mCaptureHeight, 16, mSupportRenderTextureFormat);
                Graphics.Blit(rt, desrt, mRenderMaterial);
                RenderTexture.active = desrt;

                Texture2D texture = CreateTexture2D(mCaptureWidth, mCaptureHeight, mSupportTexture2DFormat);
                Rect rc = new Rect(0, 0, mCaptureWidth, mCaptureHeight);
                texture.ReadPixels(rc, 0, 0);

                // for debug to show debug rawimage
                // we just need bytes no need to upload to texture by Texture2D.Apply()
                if (mDebugMode)
                    texture.Apply(false);

                byte[] rgbColor = GetTextureBuffer(texture);
                mCaptureFrameBytesQueue.Enqueue(rgbColor);

                mCaptureCamera.targetTexture = null;
                RenderTexture.ReleaseTemporary(rt);
                RenderTexture.ReleaseTemporary(desrt);
                RenderTexture.active = preactive;
            }
            else
            {
                if (mCaptureCamera.targetTexture != null)
                {
                    RenderTexture preactive = RenderTexture.active;
                    RenderTexture desrt = RenderTexture.GetTemporary(mCaptureWidth, mCaptureHeight, 16, mSupportRenderTextureFormat);
                    Graphics.Blit(mCaptureCamera.targetTexture, desrt, mRenderMaterial);
                    RenderTexture.active = desrt;

                    Texture2D texture = CreateTexture2D(mCaptureWidth, mCaptureHeight, mSupportTexture2DFormat);
                    Rect rc = new Rect(0, 0, mCaptureWidth, mCaptureHeight);
                    texture.ReadPixels(rc, 0, 0);

                    if (mDebugMode)
                        texture.Apply(false);

                    byte[] rgbColor = GetTextureBuffer(texture);
                    mCaptureFrameBytesQueue.Enqueue(rgbColor);
                    RenderTexture.ReleaseTemporary(desrt);
                    RenderTexture.active = preactive;
                }
            }
        }

        public byte[] DequeCaptureFrameData()
        {
            if (mCaptureFrameBytesQueue.Count > 0)
                return mCaptureFrameBytesQueue.Dequeue();
            else
                return null;
        }
    }
}