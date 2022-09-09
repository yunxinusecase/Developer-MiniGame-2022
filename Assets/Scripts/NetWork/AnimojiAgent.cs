using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MetaVerse.Animoji;

namespace Mini.Battle.Core
{
    public class AnimojiAgent : MonoBehaviour
    {
        private Camera mFaceCamera;
        private RenderTexture mRenderTeture;
        private Transform mTrsFaceCamera;
        private string TAG = "AnimojiAgent";

        // Animoji Face Animation Player
        private AnimojiPlayerWithAnimator mAnimojiPlayer;

        public void SetUpAnimojiAgent()
        {
            mAnimojiPlayer = this.GetComponent<AnimojiPlayerWithAnimator>();
            YXUnityLog.LogInfo(TAG, "SetUpAnimojiAgent");
            if (mRenderTeture == null)
            {
                mRenderTeture = (RenderTexture)Object.Instantiate(ResourceManager.Instance.mAvatarFaceRenderTexture);
                mRenderTeture.name = this.gameObject.name;
            }
            mTrsFaceCamera = this.transform.Find("FaceCamera");
            mFaceCamera = mTrsFaceCamera.GetComponent<Camera>();
            mFaceCamera.fieldOfView = 35.6f;
            mFaceCamera.farClipPlane = 0.8f;
            mFaceCamera.nearClipPlane = 0.1f;
            mFaceCamera.depth = -1;
            mFaceCamera.targetTexture = mRenderTeture;
            mTrsFaceCamera.localPosition = new Vector3(0.0f, 1.709f, 0.631f);
            mFaceCamera.clearFlags = CameraClearFlags.SolidColor;
            mFaceCamera.backgroundColor = Color.gray;
            mTrsFaceCamera.localEulerAngles = new Vector3(0.0f, 180f, 0.0f);
            mFaceCamera.cullingMask = 1 << LayerMask.NameToLayer("PlayerBody");
            mFaceCamera.enabled = false;
            mFaceCamera.useOcclusionCulling = false;
        }

        public void ShutDownAnimoji()
        {
            YXUnityLog.LogInfo(TAG, "ShutDownAnimoji");
            if (mRenderTeture != null)
            {
                Object.Destroy(mRenderTeture);
                mRenderTeture = null;
            }
        }

        public RenderTexture GetFaceCameraTexture()
        {
            return mRenderTeture;
        }

        public void ActiveFaceAnimoji(bool active)
        {
            this.mFaceCamera.enabled = active;
        }

        private static AnimojiSDKWrapper mAnimojiSdkWrapper = new AnimojiSDKWrapper();

        private float mInputWidth;
        private float mInputHeight;
        private Queue<float[]> queueExpressionData = new Queue<float[]>();
        private Queue<float[]> queueInputBBoxData = new Queue<float[]>();
        private object mLockObject = new object();

        public void PushFaceData(float[] facedata, float[] bboxdata, float width, float height)
        {
            float[] unityExpression = facedata != null ? new float[AnimojiConst.c_unityExpressionSize] : null;
            float[] bbox = bboxdata;
            mAnimojiSdkWrapper.getUnityExpressionFromArray(facedata, unityExpression);

            lock (mLockObject)
            {
                if (queueExpressionData.Count > 0)
                    queueExpressionData.Clear();
                queueExpressionData.Enqueue(unityExpression);
                if (queueInputBBoxData.Count > 0)
                    queueInputBBoxData.Clear();
                queueInputBBoxData.Enqueue(bbox);

                mInputWidth = width;
                mInputHeight = height;
            }
        }

        void Update()
        {
            MakeOneFrame();
        }

        private void MakeOneFrame()
        {
            lock (mLockObject)
            {
                if (mAnimojiPlayer == null)
                    return;
                if (queueExpressionData.Count != 0)
                {
                    float[] target = queueExpressionData.Dequeue();
                    mAnimojiPlayer.UpdateAnimoji(target);
                    if (mInputHeight * mInputWidth <= 0f)
                    {
                        mInputHeight = 1000f;
                        mInputWidth = 1000f;
                    }
                    mAnimojiPlayer.UpdateImageData(mInputWidth, mInputHeight);
                    float[] bbox = queueInputBBoxData.Dequeue();
                    mAnimojiPlayer.UpdateBBox(bbox);
                }
                else
                {
                    mAnimojiPlayer.UpdateAnimoji(null);
                    mAnimojiPlayer.UpdateBBox(null);
                    mAnimojiPlayer.UpdateImageData(1000f, 1000f);
                }
            }
        }
    }
}
