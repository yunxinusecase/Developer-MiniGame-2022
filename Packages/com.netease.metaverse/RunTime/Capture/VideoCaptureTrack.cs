using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Video;

namespace MetaVerse.FrameWork.Capture
{
    public class VideoCaptureTrack : MonoBehaviour, ICaptureController
    {
        private static string TAG = "CaptureSystem";
        private float _captureCalTime = 0f;
        private ICaptureSource mCaptureSource;
        private float mCaptureInterval = 1f / 30f;
        private Material mRenderMaterial;

        public void SetSource(ICaptureSource source)
        {
            mCaptureSource = source;
        }

        void Awake()
        {
            mRenderMaterial = Resources.Load<Material>("Unlit_CaptureBlit");
        }

        private VideoPlayer mVideoPlayer;
        private RenderTexture mRenderTexture;

        public bool InitVideoCaptureTrack(RenderTexture rt, VideoPlayer player)
        {
            if (rt == null)
                return false;
            mVideoPlayer = player;
            mRenderTexture = rt;
            return true;
        }

        public void OnUpdate(float deltaTime)
        {
            if (mCaptureSource == null || !mCaptureSource.CheckValidCapture())
                return;

            _captureCalTime += deltaTime;
            if (_captureCalTime >= mCaptureInterval)
            {
                byte[] data = DoCaptureVideo();
                mCaptureSource.OnCaptureOneFrameOutPut(data, mRenderTexture.width, mRenderTexture.height);
                _captureCalTime = 0f;
            }
        }

        private Texture2D mCaptureBackUpTexture2D = null;
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

                Debug.Log("CreateTexture2D() create new Texture2D");
            }

            return mCaptureBackUpTexture2D;
        }

        private RenderTexture mmTexture;

        private byte[] DoCaptureVideo()
        {
            RenderTexture pre = RenderTexture.active;
            // RenderTexture desrt = RenderTexture.GetTemporary(mRenderTexture.width, mRenderTexture.height, 0, mRenderTexture.format);
            if (mmTexture == null || mmTexture.width != mRenderTexture.width ||
                mmTexture.height != mRenderTexture.height)
            {
                if (mmTexture != null)
                    GameObject.Destroy(mmTexture);
                Resources.UnloadUnusedAssets();
                mmTexture = new RenderTexture(mRenderTexture.width, mRenderTexture.height, 0, mRenderTexture.format);
            }

            Graphics.Blit(mRenderTexture, mmTexture, mRenderMaterial);
            RenderTexture.active = mmTexture;
            Texture2D texture = CreateTexture2D(mmTexture.width, mmTexture.height, TextureFormat.ARGB32);
            Rect rc = new Rect(0, 0, mmTexture.width, mmTexture.height);
            texture.ReadPixels(rc, 0, 0);
            byte[] input = GetByteArrayFromTexture(texture);
            RenderTexture.active = pre;
            return input;
        }

        public static byte[] GetByteArrayFromTexture(Texture2D texture)
        {
            var rawData = GetRawDataTexture(texture);
            return rawData;
        }

        private static int sliceBuffLength = 0;
        private static byte[] allocBytes = null;

        private static byte[] GetRawDataTexture(Texture2D texture)
        {
            //  (SDKÐÞ¸Ä)
            // Color32[] imageInfo32 = texture.GetPixels32();
            // byte[] imageInfoByte = Color32ArrayToByteArray(imageInfo32);
            // return imageInfoByte;
            if (texture == null)
            {
                return null;
            }
            else
            {
                UnityEngine.Profiling.Profiler.BeginSample("getrawData");
                NativeArray<byte> array = texture.GetRawTextureData<byte>();
                var slice = new NativeSlice<byte>(array).SliceConvert<byte>();
                if (allocBytes == null || sliceBuffLength != slice.Length)
                {
                    Debug.Log($"create new alloc slice:{slice.Length}");
                    sliceBuffLength = slice.Length;
                    allocBytes = new byte[slice.Length];
                }
                // var bytes = new byte[slice.Length];
                slice.CopyTo(allocBytes);
                UnityEngine.Profiling.Profiler.EndSample();
                return allocBytes;
            }

            // if (texture == null)
            //     return null;
            // else
            //     return texture.GetRawTextureData();
        }

        public void EnableCapture(bool enable)
        {
            this.enabled = enable;
            if (enable)
                _captureCalTime = float.MaxValue;
        }

        public void SetCaptureData(int fps, int width, int height)
        {
            YXUnityLog.LogInfo(TAG, $"fps:{fps}, width:{width}, height: {height}");
            mCaptureInterval = 1f / fps;
        }

    }
}
