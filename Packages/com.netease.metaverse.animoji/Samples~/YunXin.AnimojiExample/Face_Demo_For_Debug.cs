using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Video;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using YunXinAnimoji;
using Unity.Collections;

#if ENABLE_NETEASE_ANIMOJI
using FUXI;
#endif

class Face_Demo_For_Debug : MonoBehaviour
{
#if ENABLE_NETEASE_ANIMOJI
    // AnimojiPlayer
    [Header("驱动的ReadyPlayerMe角色")]
    public DefaultAnimojiPlayer mAnimojiPlayer;

    //采集图像按钮
    public Button captureButton;
    //视频显示区域
    public RawImage captureImage;
    public RenderTexture videoTexture;
    public VideoPlayer videoPlayer;
    private Texture2D textureBuffer;

    //脸部表情的各种参数
    private List<float> faceInfo = new List<float>();
    private bool isCapturing = false;
    private bool isCalib = false;
    private bool shouldUseFrontFacing = true;
    string hexResult;
    private bool setupExpEstimator = false;
    string model_path = "";

    // 采样时间
    private float sampleTime = 0.5f;
    // 经过帧数
    private int frame;
    // 运行时间
    private float time = 0;
    private int exp_num = 52;
    private int output_num = 67;

    InitializeProcess process;
    AnimojiSDKWrapper exp = null;

    public int mWebCameraIndex = 0;

    private void Start()
    {
        process = AssetManager.copy_streaming_asset(this);
        process.onCompleted += () =>
        {
            model_path = Application.persistentDataPath;
            Application.targetFrameRate = 60;
            for (int i = 0; i < output_num; i++)
                faceInfo.Add(0);

            if (!useCamera)
            {
                videoPlayer.enabled = true;
                videoPlayer.isLooping = true;
                videoPlayer.Play();
                captureImage.texture = videoTexture;
                CreateAnimojiWrapper();
            }
            else
            {
                OnCaptureButtonClick();
            }
            textureBuffer = new Texture2D(0, 0);
        };
        captureButton.onClick.AddListener(OnCalibButtonClick);
    }

    private WebCamTexture webCamTexture = null;
    public bool useCamera = false;
    private WebCamTexture GetWebCamTexture(bool shouldUseFrontFacing)
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            webCamTexture = null;
        }
        for (int cameraIndex = mWebCameraIndex; cameraIndex < WebCamTexture.devices.Length; cameraIndex++)
        {
            var device = WebCamTexture.devices[cameraIndex];
            Debug.Log("cameraIndex: " + cameraIndex + ", is frontFace: " + device.isFrontFacing + ", name: " + device.name);
            if (device.isFrontFacing == shouldUseFrontFacing)
            {
                webCamTexture = new WebCamTexture(device.name);
                webCamTexture.requestedFPS = 60;
                break;
            }
        }

        if (webCamTexture == null)
        {
            Debug.Log("webCamTexture is null");
            if (WebCamTexture.devices.Length > 0)
            {
                webCamTexture = new WebCamTexture(WebCamTexture.devices[0].name);//, requestedWidth, requestedHeight, requestedFPS);
            }
            else
            {
                webCamTexture = new WebCamTexture();//requestedWidth, requestedHeight);
            }
        }
        Debug.Log($"WebCameraTexture width:{webCamTexture.width}, height:{webCamTexture.height}");
        return webCamTexture;
    }

    public void OnCalibButtonClick()
    {
        if (setupExpEstimator)
        {
            Debug.Log("normalizeFace");
            int ans = 0;
            if (useCamera == true)
            {
                Color32Array colorArray = new Color32Array();
                colorArray.colors = new Color32[webCamTexture.width * webCamTexture.height];
                webCamTexture.GetPixels32(colorArray.colors);
                ans = exp.normalizeFaceFromByte(colorArray.byteArray, webCamTexture.width, webCamTexture.height, 2, 3, 0);
                Debug.Log(ans);
            }
            else
            {
                RenderTexture pre = RenderTexture.active;
                RenderTexture.active = videoTexture;
                Texture2D texture = CreateTexture2D(videoTexture.width, videoTexture.height, TextureFormat.ARGB32);
                Rect rc = new Rect(0, 0, videoTexture.width, videoTexture.height);
                texture.ReadPixels(rc, 0, 0);
                byte[] input = GetTextureBuffer(texture);
                ans = exp.normalizeFaceFromByte(input, videoTexture.width, videoTexture.height, 2, 1, 0);
                RenderTexture.active = pre;
                Debug.Log(ans);
            }
            if (ans == 0)
            {
                isCalib = true;
            }
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

    public void OnCancelButtonClick()
    {
        if (isCalib)
        {
            exp.cancelNormalizeFace();
            isCalib = false;
        }
    }

    private void OnCaptureButtonClick()
    {
        CreateAnimojiWrapper();
        if (isCapturing)
        {
            isCapturing = false;
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
            }
            captureImage.texture = null;
        }
        else
        {
            isCapturing = true;
            if (webCamTexture == null)
            {
                webCamTexture = this.GetWebCamTexture(shouldUseFrontFacing);//VIDEO_WIDTH, VIDEO_HEIGHT, VIDEO_FPS,
                webCamTexture.Play();
            }
            else
            {
                captureImage.texture = webCamTexture;
                float scaleY = webCamTexture.videoVerticallyMirrored ? -1.0f : 1.0f;
                captureImage.rectTransform.localScale = new Vector3(-1, scaleY, 1);
                captureImage.rectTransform.localRotation = Quaternion.identity * Quaternion.AngleAxis(-webCamTexture.videoRotationAngle, Vector3.forward);//修正回来
                webCamTexture.Play();
            }
            captureImage.texture = webCamTexture;
            print(webCamTexture.width + "," + webCamTexture.height + "," + webCamTexture.requestedWidth + "," + webCamTexture.requestedHeight);
        }
    }

    private void CreateAnimojiWrapper()
    {
        if (exp == null)
        {
            exp = new AnimojiSDKWrapper();
            if (exp == null)
            {
                print("------------ init ExpressionEstimator error! ------------");
                return;
            }
            int result = exp.initExpressionEstimator(true, model_path);
            if (result == 0)
            {
                setupExpEstimator = true;
            }
            else
            {
                hexResult = String.Format("{0:X}", result);
                print("Init error code is Ox" + hexResult);
            }
        }
    }

    void Update()
    {
        if (process.completed)
        {
            frame += 1;
            time += Time.deltaTime;

            // 刷新帧率
            if (time >= sampleTime)
            {
                float fps = frame / time;
                frame = 0;
                time = 0;
            }
            GetFaceData();
            UpdateFace();
        }
    }

    public Color32[] GetRenderTexturePixels(RenderTexture tex)
    {
        RenderTexture.active = tex;
        //Debug.Log(textureBuffer);
        textureBuffer.Resize(tex.width, tex.height);
        textureBuffer.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
        textureBuffer.Apply();
        //UnityEngine.Object.Destroy(tempTex);
        return textureBuffer.GetPixels32();
    }

    public byte[] Color32ArrayToByteArray(Color32[] colors)
    {
        if (colors == null || colors.Length == 0)
            return null;

        int lengthOfColor32 = Marshal.SizeOf(typeof(Color32));
        int length = lengthOfColor32 * colors.Length;
        byte[] bytes = new byte[length];

        GCHandle handle = default(GCHandle);
        try
        {
            handle = GCHandle.Alloc(colors, GCHandleType.Pinned);
            IntPtr ptr = handle.AddrOfPinnedObject();
            Marshal.Copy(ptr, bytes, 0, length);
        }
        finally
        {
            if (handle != default(GCHandle))
                handle.Free();
        }

        return bytes;
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

    private void GetFaceData()
    {
        float[] expression = new float[output_num];
        float[] facebox = new float[4];

        for (int i = 0; i < output_num; i++)
        {
            faceInfo[i] = (float)0;
        }

        if (setupExpEstimator)
        {
            int ans = 0;
            if (useCamera == true)
            {
                Color32Array colorArray = new Color32Array();
                colorArray.colors = new Color32[webCamTexture.width * webCamTexture.height];
                webCamTexture.GetPixels32(colorArray.colors);
                ans = exp.getUnityExpressionFromByte(colorArray.byteArray, webCamTexture.width, webCamTexture.height, 640, expression, 2, 3, 0, true, true); //second true for open tongue detection
            }
            else
            {
                RenderTexture pre = RenderTexture.active;
                RenderTexture.active = videoTexture;
                Texture2D texture = CreateTexture2D(videoTexture.width, videoTexture.height, TextureFormat.ARGB32);
                Rect rc = new Rect(0, 0, videoTexture.width, videoTexture.height);
                texture.ReadPixels(rc, 0, 0);
                byte[] input = GetTextureBuffer(texture);
                ans = exp.getUnityExpressionFromByte(input, videoTexture.width, videoTexture.height, 640, expression, 2, 1, 0, true, true); //second true for open tongue detection
                RenderTexture.active = pre;

                //Color32[] color = GetRenderTexturePixels(videoTexture);
                //byte[] input = Color32ArrayToByteArray(color);
                //ans = exp.getUnityExpressionFromByte(input, videoTexture.width, videoTexture.height, 640, expression, 2, 1, 0, true, true); //second true for open tongue detection

            }
            // 1064968 for no face
            if (ans == 0 || ans == 1064968)
            {
                for (int i = 0; i < output_num; i++)
                    faceInfo[i] = (float)expression[i];
            }
        }
    }

    private void UpdateFace()
    {
        if (faceInfo.Count != output_num)
            return;
        if (mAnimojiPlayer != null)
            mAnimojiPlayer.UpdateAnimoji(faceInfo);
    }
#endif
}