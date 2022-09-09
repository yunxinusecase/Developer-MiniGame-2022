using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using UnityEngine;

namespace YunXinAnimoji
{
    //
    // 网易伏羲面部表情Wrapper
    // 请导入网易伏羲表情迁移SDK
    // 在ProjectSettings->Script Define Symbols 添加 ENABLE_NETEASE_ANIMOJI开启迁移功能
    //
#if ENABLE_NETEASE_ANIMOJI
    public class AnimojiSDKWrapper
    {
        public enum DLL_ENV
        {
            DLL_NONE = 0,
            DLL_OPENVINO = 1,
            DLL_MNN = 2
        }

        private DLL_ENV dll_env_ = DLL_ENV.DLL_NONE;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        public AnimojiSDKWrapper()
        {

            //checkDllEnv();
            IntPtr result = IntPtr.Zero;
            bool flag = false;
            try
            {
                result = GameAnimoji_Openvino.createExpressionEstimator();
                dll_env_ = DLL_ENV.DLL_OPENVINO;
            }
            catch (Exception e)
            {
                flag = true;
                // Debug.LogError(e.ToString());
                Debug.Log("[Warning]Not Find DLL_OPENVINO: " + e.Message);
            }

            if (flag)
            {
                flag = false;
                try
                {
                    result = GameAnimoji_MNN.createExpressionEstimator();
                    dll_env_ = DLL_ENV.DLL_MNN;
                }
                catch (Exception e)
                {
                    flag = true;
                    Debug.LogError(e.ToString());
                }
            }

            if (flag)
            {
                dll_env_ = DLL_ENV.DLL_NONE;
                return;
            }
            this.context = result;

        }
#else
        public AnimojiSDKWrapper()
        {
            dll_env_ = DLL_ENV.DLL_MNN;
            this.context = (dll_env_ == DLL_ENV.DLL_MNN) ? GameAnimoji_MNN.createExpressionEstimator() : GameAnimoji_Openvino.createExpressionEstimator();
        }
#endif
        ~AnimojiSDKWrapper()
        {
            if (dll_env_ == DLL_ENV.DLL_MNN)
            {
                GameAnimoji_MNN.deleteExpressionEstimator(this.context);
            }
            else
            {
                GameAnimoji_Openvino.deleteExpressionEstimator(this.context);
            }
        }

        public int initExpressionEstimator(bool setModelPath, string path, bool useMultiThread = true, bool useLargeModel = true)
        {
            int result = 0;
            if (dll_env_ == DLL_ENV.DLL_MNN)
                result = GameAnimoji_MNN.initExpressionEstimator(this.context, setModelPath, path, useMultiThread, useLargeModel);
            else
                result = GameAnimoji_Openvino.initExpressionEstimator(this.context, setModelPath, path, useMultiThread, useLargeModel);
            return result;
        }

        // C++ [59] index -> Unity [67] index 
        // 面部绑定共67维，[1-52] 维为表情维度参数，[53-64]
        // 维为眼球维度参数，[65-67] 维为头部角度参数。[53-67] 维结合实际情况使用。
        public int getUnityExpressionFromByte(
            byte[] data,
            int width,
            int height,
            int resize_len,
            float[] unityExpression,
            int color_codes,
            int flip_codes,
            int rotate_codes,
            bool use_smooth = true,
            bool use_tongue = false)
        {
            float[] expression_59 = new float[59];

            int result = 0;
            unsafe
            {
                fixed (float* weights = expression_59)
                {
                    if (dll_env_ == DLL_ENV.DLL_MNN)
                    {
                        result = GameAnimoji_MNN.getExpressionFromByte(this.context, data, width, height, resize_len, (IntPtr)weights, use_smooth, color_codes, flip_codes, rotate_codes, use_tongue);
                    }
                    else
                    {
                        result = GameAnimoji_Openvino.getExpressionFromByte(this.context, data, width, height, resize_len, (IntPtr)weights, use_smooth, color_codes, flip_codes, rotate_codes, use_tongue);
                    }
                }
            }

            //blendshape，[3,54]维表示52维表情参数
            for (int i = 0; i < 52; i++)
                unityExpression[i] = expression_59[3 + i];

            //
            // 眼部维度具体含义[针对出来的数据做一次转换封装]
            // 
            // 眼部分为12个维度，每单眼为6个维度。所有维度范围为[0, 100]，0表示初始位置。
            // [53,54,55,56,57,58] 维度为左眼维度，分别为向左看、向右看、向上看、向下看、眼球瞳孔收缩、眼球瞳孔放大。
            // [52,53,54,55,56,57] 索引

            // [59,60,61,62,63,64] 维度为右眼维度，分别为向左看、向右看、向上看、向下看、眼球瞳孔收缩，眼球瞳孔放大。
            // [58,59,60,61,62,63] 索引
            /*
             * C++，[55,56]维/索引为模型左眼球的角度，[57,58]维为模型右眼球的角度。需要特别注明，该眼球角度是基于视线朝向前方的偏差值(delta值)，
             * 三维眼球角度中，其中第一维为pitch，且向上看为正值，向下为负值，范围为[-20，+20]；
             * 第二维为yaw，向左看为正值，向右看为负值，范围为[-20，+20]。如果眼球目视前面非[0,0]，那么我们会在配置文件中加上这个偏差值才可。
             */

            ///////////////////////////////////////////////
            /// 左眼球
            //left eye，expression_59[55,56]维/索引为模型左眼球的角度
            // 55->pitch 上正下负
            // 56->yaw 左正右负
            if (expression_59[3 + 52 + 1] >= 0.0f)
            {
                // 左眼球向左看
                unityExpression[52] = expression_59[3 + 52 + 1];
                unityExpression[53] = 0.0f;
            }
            else
            {
                // 左眼球向右看
                unityExpression[52] = 0.0f;
                unityExpression[53] = -expression_59[3 + 52 + 1];
            }

            if (expression_59[3 + 52] >= 0.0f)
            {
                // 左眼球向上看
                unityExpression[54] = expression_59[3 + 52];
                unityExpression[55] = 0.0f;
            }
            else
            {
                // 左眼球向下看
                unityExpression[54] = 0.0f;
                unityExpression[55] = -expression_59[3 + 52];
            }

            // 左眼球瞳孔收缩放大
            unityExpression[56] = 0.0f;
            unityExpression[57] = 0.0f;

            ///////////////////////////////////////////////
            /// 右眼球
            // right eye，expression_59[57,58]维/索引为模型右眼球的角度。
            // 57-> pitch -> 上正下负
            // 58-> yaw -> 左正右负 
            if (expression_59[3 + 52 + 3] >= 0.0f)
            {
                // 右眼球向左看
                unityExpression[58] = expression_59[3 + 52 + 3];
                unityExpression[59] = 0.0f;
            }
            else
            {
                // 右眼球向右看
                unityExpression[58] = 0.0f;
                unityExpression[59] = -expression_59[3 + 52 + 3];
            }
            if (expression_59[3 + 52 + 2] >= 0.0f)
            {
                // 右眼球向上看
                unityExpression[60] = expression_59[3 + 52 + 2];
                unityExpression[61] = 0.0f;
            }
            else
            {
                // 右眼球向下看
                unityExpression[60] = 0.0f;
                unityExpression[61] = -expression_59[3 + 52 + 2];
            }

            // 右眼球瞳孔收缩放大
            unityExpression[62] = 0.0f;
            unityExpression[63] = 0.0f;

            //head rotation前三维数据[0, 2]表示头部角度，分别为pitch、yaw、roll。pitch向下为正值，yaw向左为正值，roll顺时针为正值。
            unityExpression[64] = expression_59[0];
            unityExpression[65] = expression_59[1];
            unityExpression[66] = expression_59[2];

            return result;
        }

        public int getExpressionBboxFromByte(
            byte[] data,
            int width,
            int height,
            int resize_len,
            float[] expression,
            float[] facebox,
            int color_codes,
            int flip_codes,
            int rotate_codes,
            bool use_smooth = true,
            bool use_tongue = false)
        {
            int result = 0;
            unsafe
            {
                fixed (float* weights = expression, box = facebox)
                {
                    if (dll_env_ == DLL_ENV.DLL_MNN)
                    {
                        result = GameAnimoji_MNN.getExpressionBboxFromByte(this.context, data, width, height, resize_len, (IntPtr)weights, (IntPtr)box, use_smooth, color_codes, flip_codes, rotate_codes, use_tongue);
                    }
                    else
                    {
                        result = GameAnimoji_Openvino.getExpressionBboxFromByte(this.context, data, width, height, resize_len, (IntPtr)weights, (IntPtr)box, use_smooth, color_codes, flip_codes, rotate_codes, use_tongue);
                    }
                }
            }
            return result;
        }

        public int normalizeFaceFromByte(byte[] data, int width, int height, int color_codes, int flip_codes, int rotate_codes)
        {
            if (dll_env_ == DLL_ENV.DLL_MNN)
            {
                return GameAnimoji_MNN.normalizeFaceFromByte(this.context, data, width, height, color_codes, flip_codes, rotate_codes);
            }
            else
            {
                return GameAnimoji_Openvino.normalizeFaceFromByte(this.context, data, width, height, color_codes, flip_codes, rotate_codes);
            }
        }

        public void cancelNormalizeFace()
        {
            if (dll_env_ == DLL_ENV.DLL_MNN)
            {
                GameAnimoji_MNN.cancelNormalizeFace(this.context);
            }
            else
            {
                GameAnimoji_Openvino.cancelNormalizeFace(this.context);
            }
        }

        public int setHeadSensitivity(bool is_head_sensitivity_high)
        {
            if (dll_env_ == DLL_ENV.DLL_MNN)
            {
                return GameAnimoji_MNN.setHeadSensitivity(this.context, is_head_sensitivity_high);
            }
            else
            {
                return GameAnimoji_Openvino.setHeadSensitivity(this.context, is_head_sensitivity_high);
            }
        }

        public int setSensitivity(float head_sensitivity, float brown_and_nose_sensitivity, float mouth_sensitivity, float blink_sensitivity, float gaze_sensitivity)
        {
            if (dll_env_ == DLL_ENV.DLL_MNN)
            {
                return GameAnimoji_MNN.setSensitivity(this.context, head_sensitivity, brown_and_nose_sensitivity, mouth_sensitivity, blink_sensitivity, gaze_sensitivity);
            }
            else
            {
                return GameAnimoji_Openvino.setSensitivity(this.context, head_sensitivity, brown_and_nose_sensitivity, mouth_sensitivity, blink_sensitivity, gaze_sensitivity);
            }
        }

        public int switchMultiThread()
        {
            if (dll_env_ == DLL_ENV.DLL_MNN)
            {
                return GameAnimoji_MNN.switchMultiThread(this.context);
            }
            else
            {
                return GameAnimoji_Openvino.switchMultiThread(this.context);
            }
        }

        public int switchModelSize(bool setModelPath, string path, bool useLargeModel = true)
        {
            if (dll_env_ == DLL_ENV.DLL_MNN)
            {
                return GameAnimoji_MNN.switchModelSize(this.context, setModelPath, path, useLargeModel);
            }
            else
            {
                return GameAnimoji_Openvino.switchModelSize(this.context, setModelPath, path, useLargeModel);
            }
        }

        private void checkDllEnv()
        {
            // check dll file
            Debug.Log("=========================");
            Debug.Log("checking develop environment...");
            string dataPath = Application.dataPath;
            string searchPath = "";
            var searchOption = SearchOption.AllDirectories;
#if UNITY_EDITOR
            searchPath = dataPath + "/Plugins";
#elif UNITY_STANDALONE_WIN
            int i = dataPath.LastIndexOf("/");
            searchPath = dataPath + "/Plugins";
#endif
            Debug.Log(searchPath);
            string streamingAssetPath = (Application.streamingAssetsPath);
            string configPath = streamingAssetPath + "/check_dll_list.txt";
            string[] readText = File.ReadAllLines(configPath);
            List<string> missingFiles = new List<string>();
            foreach (string s in readText)
            {
                var f = Directory.GetFiles(searchPath, s, searchOption);
                if (f.Length == 0)
                {
                    missingFiles.Add(s);
                }
            }

            if (missingFiles.Count > 0)
            {
                Debug.Log("missing dll files:");
                foreach (string s in missingFiles)
                {
                    Debug.Log(s);
                }
            }
        }

        private IntPtr context;
    }

    public static class GameAnimoji_Openvino
    {
        //#if UNITY_STANDALONE_WIN
        [DllImport("GameAnimoji", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr createExpressionEstimator();

        [DllImport("GameAnimoji", CallingConvention = CallingConvention.Cdecl)]
        public static extern int initExpressionEstimator(IntPtr context, bool setPath, string path, bool useMultiThread, bool useLargeModel);

        [DllImport("GameAnimoji", CallingConvention = CallingConvention.Cdecl)]
        public static extern void deleteExpressionEstimator(IntPtr context);

        [DllImport("GameAnimoji", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getExpressionFromByte(IntPtr context, byte[] data, int width, int height, int resize_len, IntPtr expression, bool use_smooth, int color_codes, int flip_codes, int rotate_codes, bool use_tongue);

        [DllImport("GameAnimoji", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getExpressionBboxFromByte(IntPtr context, byte[] data, int width, int height, int resize_len, IntPtr expression, IntPtr box, bool use_smooth, int color_codes, int flip_codes, int rotate_codes, bool use_tongue);

        [DllImport("GameAnimoji", CallingConvention = CallingConvention.Cdecl)]
        public static extern int normalizeFaceFromByte(IntPtr context, byte[] data, int width, int height, int color_codes, int flip_codes, int rotate_codes);

        [DllImport("GameAnimoji", CallingConvention = CallingConvention.Cdecl)]
        public static extern int cancelNormalizeFace(IntPtr context);

        [DllImport("GameAnimoji", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setHeadSensitivity(IntPtr context, bool is_head_sensitivity_high);

        [DllImport("GameAnimoji", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setSensitivity(IntPtr context, float head_sensitivity, float brown_and_nose_sensitivity, float mouth_sensitivity, float blink_sensitivity, float gaze_sensitivity);

        [DllImport("GameAnimoji", CallingConvention = CallingConvention.Cdecl)]
        public static extern int switchMultiThread(IntPtr context);

        [DllImport("GameAnimoji", CallingConvention = CallingConvention.Cdecl)]
        public static extern int switchModelSize(IntPtr context, bool setAssetPath, string path, bool useLargeModel);
        //#endif
    }

    public static class GameAnimoji_MNN
    {
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        [DllImport("GameAnimoji_mnn", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr createExpressionEstimator();

        [DllImport("GameAnimoji_mnn", CallingConvention = CallingConvention.Cdecl)]
        public static extern int initExpressionEstimator(IntPtr context, bool setPath, string path, bool useMultiThread, bool useLargeModel);

        [DllImport("GameAnimoji_mnn", CallingConvention = CallingConvention.Cdecl)]
        public static extern void deleteExpressionEstimator(IntPtr context);

        [DllImport("GameAnimoji_mnn", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getExpressionFromByte(IntPtr context, byte[] data, int width, int height, int resize_len, IntPtr expression, bool use_smooth, int color_codes, int flip_codes, int rotate_codes, bool use_tongue);

        [DllImport("GameAnimoji_mnn", CallingConvention = CallingConvention.Cdecl)]
        public static extern int getExpressionBboxFromByte(IntPtr context, byte[] data, int width, int height, int resize_len, IntPtr expression, IntPtr box, bool use_smooth, int color_codes, int flip_codes, int rotate_codes, bool use_tongue);

        [DllImport("GameAnimoji_mnn", CallingConvention = CallingConvention.Cdecl)]
        public static extern int normalizeFaceFromByte(IntPtr context, byte[] data, int width, int height, int color_codes, int flip_codes, int rotate_codes);

        [DllImport("GameAnimoji_mnn", CallingConvention = CallingConvention.Cdecl)]
        public static extern int cancelNormalizeFace(IntPtr context);

        [DllImport("GameAnimoji_mnn", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setHeadSensitivity(IntPtr context, bool is_head_sensitivity_high);

        [DllImport("GameAnimoji_mnn", CallingConvention = CallingConvention.Cdecl)]
        public static extern int setSensitivity(IntPtr context, float head_sensitivity, float brown_and_nose_sensitivity, float mouth_sensitivity, float blink_sensitivity, float gaze_sensitivity);

        [DllImport("GameAnimoji_mnn", CallingConvention = CallingConvention.Cdecl)]
        public static extern int switchMultiThread(IntPtr context);

        [DllImport("GameAnimoji_mnn", CallingConvention = CallingConvention.Cdecl)]
        public static extern int switchModelSize(IntPtr context, bool setAssetPath, string path, bool useLargeModel);
#endif
#if UNITY_IOS
        [DllImport("__Internal")]
        public static extern IntPtr createExpressionEstimator();

        [DllImport("__Internal")]
        public static extern int initExpressionEstimator(IntPtr context, bool setPath, string path, bool useMultiThread, bool useLargeModel);

        [DllImport("__Internal")]
        public static extern int deleteExpressionEstimator(IntPtr context);

        [DllImport("__Internal")]
        public static extern int getExpressionFromByte(IntPtr context, byte[] data, int width, int height, int resize_len, IntPtr expression, bool use_smooth, int color_codes, int flip_codes, int rotate_codes, bool use_tongue);

        [DllImport("__Internal")]
        public static extern int getExpressionBboxFromByte(IntPtr context, byte[] data, int width, int height, int resize_len, IntPtr expression, IntPtr box, bool use_smooth, int color_codes, int flip_codes, int rotate_codes, bool use_tongue);

        [DllImport("__Internal")]
        public static extern int normalizeFaceFromByte(IntPtr context, byte[] data, int width, int height, int color_codes, int flip_codes, int rotate_codes);

        [DllImport("__Internal")]
        public static extern int cancelNormalizeFace(IntPtr context);

        [DllImport("__Internal")]
        public static extern int setHeadSensitivity(IntPtr context, bool is_head_sensitivity_high);

        [DllImport("__Internal")]
        public static extern int setSensitivity(IntPtr context, float head_sensitivity, float brown_and_nose_sensitivity, float mouth_sensitivity, float blink_sensitivity, float gaze_sensitivity);

        [DllImport("__Internal")]
        public static extern int switchMultiThread(IntPtr context);

        [DllImport("__Internal")]
        public static extern int switchModelSize(IntPtr context, bool setAssetPath, string path, bool useLargeModel);

#endif

#if UNITY_ANDROID
        [DllImport("GameAnimoji")]
        public static extern IntPtr createExpressionEstimator();

        [DllImport("GameAnimoji")]
        public static extern int initExpressionEstimator(IntPtr context, bool setPath, string path, bool useMultiThread, bool useLargeModel);

        [DllImport("GameAnimoji")]
        public static extern int deleteExpressionEstimator(IntPtr context);

        [DllImport("GameAnimoji")]
        public static extern int getExpressionFromByte(IntPtr context, byte[] data, int width, int height, int resize_len, IntPtr expression, bool use_smooth, int color_codes, int flip_codes, int rotate_codes, bool use_tongue);

        [DllImport("GameAnimoji")]
        public static extern int getExpressionBboxFromByte(IntPtr context, byte[] data, int width, int height, int resize_len, IntPtr expression, IntPtr box, bool use_smooth, int color_codes, int flip_codes, int rotate_codes, bool use_tongue);

        [DllImport("GameAnimoji")]
        public static extern int normalizeFaceFromByte(IntPtr context, byte[] data, int width, int height, int color_codes, int flip_codes, int rotate_codes);

        [DllImport("GameAnimoji")]
        public static extern int cancelNormalizeFace(IntPtr context);

        [DllImport("GameAnimoji")]
        public static extern int setHeadSensitivity(IntPtr context, bool is_head_sensitivity_high);

        [DllImport("GameAnimoji")]
        public static extern int setSensitivity(IntPtr context, float head_sensitivity, float brown_and_nose_sensitivity, float mouth_sensitivity, float blink_sensitivity, float gaze_sensitivity);

        [DllImport("GameAnimoji")]
        public static extern int switchMultiThread(IntPtr context);

        [DllImport("GameAnimoji")]
        public static extern int switchModelSize(IntPtr context, bool setAssetPath, string path, bool useLargeModel);

#endif
    }
#endif
}
