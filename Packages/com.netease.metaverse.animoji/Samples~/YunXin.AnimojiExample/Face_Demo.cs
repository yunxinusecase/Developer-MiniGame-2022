#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Runtime.InteropServices;

#if ENABLE_NETEASE_ANIMOJI
using FUXI;
#endif

//
// Basic Face Animoji Demo
//
public class Face_Demo : MonoBehaviour
{
#if ENABLE_NETEASE_ANIMOJI
    public Button captureButton; //�ɼ�ͼ��ť
    public RawImage captureImage; //��Ƶ��ʾ����

    public Transform leftPupil;
    public Transform rightPupil;
    public SkinnedMeshRenderer skin;
    public SkinnedMeshRenderer skin_eyelash;
    public SkinnedMeshRenderer skin_teeth;
    public Transform head;

    private int expressionStartIndex = 3;
    public RenderTexture videoTexture;
    public VideoPlayer videoPlayer;
    private Texture2D textureBuffer;

    private List<float> faceInfo = new List<float>(); //��������ĸ��ֲ���
    private bool isCapturing = false;
    private bool isCalib = false;
    private bool shouldUseFrontFacing = true;
    string hexResult;
    private bool setupExpEstimator = false;
    string model_path = "";

    private float sampleTime = 0.5f; // ����ʱ��
    private int frame;              // ����֡��
    private float time = 0;         // ����ʱ��
    private int exp_num = 52;
    private int output_num = 67;

    private MemoryStream WriteBuffer { get; set; }

    ExpressionEstimator exp = null;

    InitializeProcess process;

    private void Start()
    {
        process = AssetManager.copy_streaming_asset(this);
        process.onCompleted += () =>
        {
            model_path = Application.persistentDataPath;
            Application.targetFrameRate = 60;
            for (int i = 0; i < output_num; i++)
            {
                faceInfo.Add(0);
            }
            videoPlayer.Play();
            OnCaptureButtonClick();
            textureBuffer = new Texture2D(0, 0);
        };
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (setupExpEstimator)
            {
                exp.setSensitivity(0.2f, 0.5f, 0.5f, 0.5f, 0.5f);
                Debug.Log("setSensitivity called");
            }
            int[] _mapArray = { 9, 10, 19, 20, 11, 12, 13, 14, 21, 22, 15, 16, 17, 18, 1, 2, 3, 4, 5, 25, 27, 24, 26, 23, 48, 49, 34, 35, 41, 40, 44, 45, 28, 29, 46, 47, 30, 31, 36, 37, 38, 32, 33, 39, 42, 43, 50, 51, 6, 7, 8, 52 };
            Debug.Log("map array length: " + _mapArray.Length);
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            OnCalibButtonClick();
        }

        if (process.completed)
        {
            frame += 1;
            time += Time.deltaTime;

            // ˢ��֡��
            if (time >= sampleTime)
            {
                float fps = frame / time;
                frame = 0;
                time = 0;
            }
            GetFeature();
            UpdateFace();
        }
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
                Color32[] color = GetRenderTexturePixels(videoTexture);
                byte[] input = Color32ArrayToByteArray(color);
                ans = exp.normalizeFaceFromByte(input, videoTexture.width, videoTexture.height, 2, 1, 0);
                Debug.Log(ans);
            }
            if (ans == 0)
            {
                isCalib = true;
            }
        }
    }

    public void OnCancelButtonClick()
    {
        if (isCalib)
        {
            exp.cancelNormalizeFace();
            isCalib = false;
        }
    }

    public void SwitchMultiThread(bool toggle)
    {
        int result = exp.switchMultiThread();
        //exp.setSensitivity(0.2f, 0.5f, 0.5f, 0.5f, 0.5f);
    }

    public void SwitchModelSize(bool toggle)
    {
        int result = exp.switchModelSize(true, model_path, toggle);
    }

    private void OnCaptureButtonClick()
    {
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
            }
            if (useCamera == false)
            {
                captureImage.texture = videoTexture;
            }
            else
            {
                captureImage.texture = webCamTexture;
                float scaleY = webCamTexture.videoVerticallyMirrored ? -1.0f : 1.0f;
                captureImage.rectTransform.localScale = new Vector3(-1, scaleY, 1);
                captureImage.rectTransform.localRotation = Quaternion.identity * Quaternion.AngleAxis(-webCamTexture.videoRotationAngle, Vector3.forward);//��������
                webCamTexture.Play();
            }

            //print(webCamTexture.width + "," + webCamTexture.height + "," + webCamTexture.requestedWidth + "," + webCamTexture.requestedHeight);
            if (exp == null)
            {
                exp = new ExpressionEstimator();
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
            //print("toggle ani on finish");
        }
    }

    private WebCamTexture webCamTexture = null;
    public bool useCamera = false;
    private WebCamTexture GetWebCamTexture(bool shouldUseFrontFacing)//int requestedWidth, int requestedHeight, int requestedFPS,
    {
        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            webCamTexture = null;
        }

        for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++)
        {
            var device = WebCamTexture.devices[cameraIndex];
            Debug.Log("cameraIndex: " + cameraIndex + ", is frontFace: " + device.isFrontFacing);
            if (device.isFrontFacing == shouldUseFrontFacing)
            {
                webCamTexture = new WebCamTexture(device.name);//, requestedWidth, requestedHeight, requestedFPS);
                webCamTexture.requestedFPS = 60;
                //useCamera = true;
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

        return webCamTexture;
    }

    internal void Write(MemoryStream ms)
    {
        ms.Seek(0, SeekOrigin.Begin);
        WriteBuffer.Write(ms.GetBuffer(), 0, (int)ms.Length);
        ms.Seek(0, SeekOrigin.Begin);
    }


    public static void WriteFile(string filePath, byte[] content)
    {
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                fs.Write(content, 0, content.Length);
            }
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

    public void GetFeature()
    {
        float[] expression = new float[output_num];
        float[] facebox = new float[4];
        //int length = (int)(WriteBuffer.Length);

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
                ans = exp.getExpressionFromByte(colorArray.byteArray, webCamTexture.width, webCamTexture.height, 640, expression, 2, 3, 0, true, true); //second true for open tongue detection
            }
            else
            {
                Color32[] color = GetRenderTexturePixels(videoTexture);
                byte[] input = Color32ArrayToByteArray(color);
                ans = exp.getExpressionFromByte(input, videoTexture.width, videoTexture.height, 640, expression, 2, 1, 0, true, true); //second true for open tongue detection
            }
            if (ans == 0 || ans == 1064968) // 1064968 for no face
            {
                for (int i = 0; i < output_num; i++)
                {
                    faceInfo[i] = (float)expression[i];
                }
            }
        }
    }

    //// different config for different model
    [SerializeField]
    private FacialAnimComponent facialAnimComp;

    public void UpdateFace()
    {
        if (faceInfo.Count != output_num)
            return;

        if (skin != null)
        {
            for (int i = 0; i < facialAnimComp.bs_reindex.Length; i++)
            {
                int a = facialAnimComp.bs_reindex[i] - 1;
                if (a >= faceInfo.Count || a < 0)
                    continue;
                int bscount = skin.sharedMesh.blendShapeCount;
                if (i >= bscount)
                    continue;
                skin.SetBlendShapeWeight(i, (float)faceInfo[a]);
            }
        }

        if (skin_eyelash != null)
        {
            for (int i = 0; i < facialAnimComp.bs_reindex.Length; i++)
            {
                skin_eyelash.SetBlendShapeWeight(i, (float)faceInfo[facialAnimComp.bs_reindex[i] - 1]);
            }
        }

        if (skin_teeth != null)
        {
            for (int i = 0; i < facialAnimComp.bs_reindex.Length; i++)
            {
                skin_teeth.SetBlendShapeWeight(i, (float)faceInfo[facialAnimComp.bs_reindex[i] - 1]);
            }
        }

        if (head != null)
        {
            int rotation_start = 64;
            head.localEulerAngles = new Vector3(
                (float)(facialAnimComp.headpose_orientation[0] * faceInfo[rotation_start + facialAnimComp.headpose_reindex[0]]) + facialAnimComp.headpose_offset[0],
                (float)(facialAnimComp.headpose_orientation[1] * faceInfo[rotation_start + facialAnimComp.headpose_reindex[1]]) + facialAnimComp.headpose_offset[1],
                (float)(facialAnimComp.headpose_orientation[2] * faceInfo[rotation_start + facialAnimComp.headpose_reindex[2]]) + facialAnimComp.headpose_offset[2]);
        }

        int eye_start = 52;
        int right_eye_start = 58;

        if (facialAnimComp.driver_pupil_by_rotate)
        {
            if (leftPupil != null)
            {
                var rotation = leftPupil.localEulerAngles;
                // right_left_ratio: �����Щģ�͵����򲻴����۾����ģ�����һ��ϵ����ͫ����߽��Զһ�����ת�ǶȽ���������ϵ�����壺��ģ������Ϊ����ͫ���Ҳ���ת�Ƕ�Ҫ��������󼸱���
                // left_right_bias: ����faceInfo�����������˶�ʱ��������άֻ��һά�ᱻ���left_right_bias������ʾ�������������������ת�ĽǶȣ�
                // up_down_bias: ����faceInfo�����������˶�ʱ��������άֻ��һά�ᱻ���up_down_bias������ʾ�������������������ת�ĽǶȣ�

                // yaw������ת�ĽǶ�
                float left_right_bias = (faceInfo[eye_start + 0] == 0.0f) ? -faceInfo[eye_start + 1] * facialAnimComp.right_left_ratio : faceInfo[eye_start + 0];
                // pitch������ת�ĽǶ�
                float up_down_bias = (faceInfo[eye_start + 2] == 0.0f) ? -faceInfo[eye_start + 3] : faceInfo[eye_start + 2];

                // 0 -> pitch
                if (facialAnimComp.leftpupil_reindex[0] == 0)
                {
                    rotation.x = facialAnimComp.leftpupil_coef[0] * up_down_bias + facialAnimComp.leftpupil_offset[0]; // ����
                }
                else if (facialAnimComp.leftpupil_reindex[0] == 1)
                {
                    rotation.y = facialAnimComp.leftpupil_coef[0] * up_down_bias + facialAnimComp.leftpupil_offset[0]; // ����
                }
                else
                {
                    rotation.z = facialAnimComp.leftpupil_coef[0] * up_down_bias + facialAnimComp.leftpupil_offset[0]; // ����
                }

                // 1 -> yaw
                if (facialAnimComp.leftpupil_reindex[1] == 0)
                {
                    rotation.x = facialAnimComp.leftpupil_coef[1] * left_right_bias + facialAnimComp.leftpupil_offset[1]; // ����
                }
                else if (facialAnimComp.leftpupil_reindex[1] == 1)
                {
                    rotation.y = facialAnimComp.leftpupil_coef[1] * left_right_bias + facialAnimComp.leftpupil_offset[1]; // ����
                }
                else
                {
                    rotation.z = facialAnimComp.leftpupil_coef[1] * left_right_bias + facialAnimComp.leftpupil_offset[1]; // ����
                }

                if (facialAnimComp.leftpupil_reindex[2] == 0)
                {
                    rotation.x = facialAnimComp.leftpupil_offset[2];
                }
                else if (facialAnimComp.leftpupil_reindex[2] == 1)
                {
                    rotation.y = facialAnimComp.leftpupil_offset[2];
                }
                else
                {
                    rotation.z = facialAnimComp.leftpupil_offset[2];
                }

                leftPupil.localEulerAngles = rotation;
            }

            if (rightPupil != null)
            {
                var rotation = rightPupil.localEulerAngles;

                float left_right_bias = (faceInfo[right_eye_start + 0] == 0.0f) ? -faceInfo[right_eye_start + 1] : faceInfo[right_eye_start + 0] * facialAnimComp.right_left_ratio;
                float up_down_bias = (faceInfo[right_eye_start + 2] == 0.0f) ? -faceInfo[right_eye_start + 3] : faceInfo[right_eye_start + 2];

                if (facialAnimComp.rightpupil_reindex[0] == 0)
                {
                    rotation.x = facialAnimComp.rightpupil_coef[0] * up_down_bias + facialAnimComp.rightpupil_offset[0];
                }
                else if (facialAnimComp.rightpupil_reindex[0] == 1)
                {
                    rotation.y = facialAnimComp.rightpupil_coef[0] * up_down_bias + facialAnimComp.rightpupil_offset[0];
                }
                else
                {
                    rotation.z = facialAnimComp.rightpupil_coef[0] * up_down_bias + facialAnimComp.rightpupil_offset[0];
                }

                if (facialAnimComp.rightpupil_reindex[1] == 0)
                {
                    rotation.x = facialAnimComp.rightpupil_coef[1] * left_right_bias + facialAnimComp.rightpupil_offset[1];
                }
                else if (facialAnimComp.rightpupil_reindex[1] == 1)
                {
                    rotation.y = facialAnimComp.rightpupil_coef[1] * left_right_bias + facialAnimComp.rightpupil_offset[1];
                }
                else
                {
                    rotation.z = facialAnimComp.rightpupil_coef[1] * left_right_bias + facialAnimComp.rightpupil_offset[1];
                }

                if (facialAnimComp.rightpupil_reindex[2] == 0)
                {
                    rotation.x = facialAnimComp.rightpupil_offset[2];
                }
                else if (facialAnimComp.rightpupil_reindex[2] == 1)
                {
                    rotation.y = facialAnimComp.rightpupil_offset[2];
                }
                else
                {
                    rotation.z = facialAnimComp.rightpupil_offset[2];
                }

                rightPupil.localEulerAngles = rotation;
            }
        }
        else
        {
            Debug.Log("Use Blend Shape Eye Control");
            if (faceInfo[exp_num + 0 + expressionStartIndex] > 0.0)
                skin.SetBlendShapeWeight(facialAnimComp.leftpupil_udlr_reindex[0], (float)faceInfo[eye_start + 2] * facialAnimComp.leftpupil_coef[0] + facialAnimComp.leftpupil_offset[0]);
            else
                skin.SetBlendShapeWeight(facialAnimComp.leftpupil_udlr_reindex[1], -(float)faceInfo[eye_start + 2] * facialAnimComp.leftpupil_coef[0] + facialAnimComp.leftpupil_offset[0]);

            if (faceInfo[exp_num + 1 + expressionStartIndex] < 0.0)
                skin.SetBlendShapeWeight(facialAnimComp.leftpupil_udlr_reindex[2], -(float)faceInfo[eye_start + 0] * facialAnimComp.leftpupil_coef[1] + facialAnimComp.leftpupil_offset[1]);
            else
                skin.SetBlendShapeWeight(facialAnimComp.leftpupil_udlr_reindex[3], (float)faceInfo[eye_start + 0] * facialAnimComp.leftpupil_coef[1] + facialAnimComp.leftpupil_offset[1]);

            if (faceInfo[exp_num + 2 + expressionStartIndex] > 0.0)
                skin.SetBlendShapeWeight(facialAnimComp.rightpupil_udlr_reindex[0], (float)faceInfo[right_eye_start + 1] * facialAnimComp.rightpupil_coef[0] + facialAnimComp.rightpupil_offset[0]);
            else
                skin.SetBlendShapeWeight(facialAnimComp.rightpupil_udlr_reindex[1], -(float)faceInfo[right_eye_start + 1] * facialAnimComp.rightpupil_coef[0] + facialAnimComp.rightpupil_offset[0]);

            if (faceInfo[exp_num + 3 + expressionStartIndex] < 0.0)
                skin.SetBlendShapeWeight(facialAnimComp.rightpupil_udlr_reindex[2], -(float)faceInfo[right_eye_start + 0] * facialAnimComp.rightpupil_coef[1] + facialAnimComp.rightpupil_offset[1]);
            else
                skin.SetBlendShapeWeight(facialAnimComp.rightpupil_udlr_reindex[3], (float)faceInfo[right_eye_start + 0] * facialAnimComp.rightpupil_coef[1] + facialAnimComp.rightpupil_offset[1]);
        }

    }
#endif
}
#endif