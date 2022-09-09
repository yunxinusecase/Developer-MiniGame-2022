using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace MetaVerse.Animoji
{
    //
    // Animoji Player 
    //
    public abstract class AnimojiPlayer : MonoBehaviour
    {
        public bool mEnableHeadRotation = true;
        public bool mEnableBlendShape = true;
        public bool mEnableEye = true;

        public bool mRigDrivenEye = false;
        public bool mJustSet = true;

        public FaceMapperData mFaceMapData = null;

        public Transform mLeftEyeTransform;
        public Transform mRightEyeTransform;
        public Transform mHeadTransform;

        protected Vector3 mHeadRotation;
        protected Vector3 mRightEyeRotation;
        protected Vector3 mLeftEyeRotation;

        private Vector3 mHeadEulerV3;
        private bool mHeadStopped = false;

        protected float Epsilon
        {
            get { return 0.0001f; }
        }

        [SerializeField, Range(0f, 0.5f)] protected float m_ClampBoundApprox = 0.052f;
        [SerializeField, Range(0f, 1f)] protected float m_ClampFaceSize = 0.03f;

        public Rect NormalizeBBox;
        private float[] BBoxRawData = new float[4];
        private float ImageWidth = 0;
        private float ImageHeight = 0;

        protected virtual void Start()
        {
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual bool CanUpdateBlendShape
        {
            get
            {
                return mEnableBlendShape;
            }
        }

        protected virtual bool CanUpdateEye
        {
            get { return mEnableEye && mRigDrivenEye; }
        }

        protected virtual bool CanUpdateHead
        {
            get
            {
                return mEnableHeadRotation;
            }
        }

        public virtual void SetFaceValid(bool validFace)
        {
        }

        public void SetBlendShapeEvaluatorMultiplier(FaceBlendShapeDefine.FaceBlendShape fs, float newvalue)
        {
            if (mFaceMapData == null)
                return;
            int count = mFaceMapData.m_BlendShapeConfig.Count;
            for (int i = 0; i < count; i++)
            {
                foreach (var VARIABLE in mFaceMapData.m_BlendShapeConfig[i].m_Binds)
                {
                    if (VARIABLE.Location == fs)
                    {
                        VARIABLE.m_Config.GetSimpleEvaluator.m_Multiplier = newvalue;
                    }
                }
            }
        }

        protected void FindProperty()
        {
            if (mFaceMapData != null)
            {
                if (mLeftEyeTransform == null)
                    mLeftEyeTransform = this.transform.Find(mFaceMapData.m_LeftEyeBonePath);
                if (mRightEyeTransform == null)
                    mRightEyeTransform = this.transform.Find(mFaceMapData.m_RightEyeBonePath);
                if (mHeadTransform == null)
                    mHeadTransform = this.transform.Find(mFaceMapData.m_HeadBonePath);
            }
        }

        protected bool CheckValidFaceData()
        {
            if (ImageHeight == 0 || ImageWidth == 0)
                return false;
            return true;
        }

        private Rect GetBBox()
        {
            return new Rect(BBoxRawData[0], BBoxRawData[1], BBoxRawData[2] - BBoxRawData[0], BBoxRawData[3] - BBoxRawData[1]);
        }

        protected static bool IsTooCloseToBound(Rect rect, float threshold)
        {
            var bbox = rect;
            var boundApprox = Mathf.Min(bbox.xMin, 1f - bbox.xMax, bbox.yMin, 1f - bbox.yMax);
            return boundApprox > threshold;
        }

        protected static bool IsFaceDisanceValid(Rect rect, float sizeThreshold)
        {
            return Mathf.Min(rect.width, rect.height) > sizeThreshold;
        }

        protected void RefreshBBoxData(IEnumerable<float> bboxArray)
        {
            if (bboxArray == null || bboxArray.Count() != 4)
            {
                BBoxRawData[0] = BBoxRawData[1] = BBoxRawData[2] = BBoxRawData[3] = 0f;
                return;
            }
            BBoxRawData[0] = bboxArray.ElementAt(0);
            BBoxRawData[1] = bboxArray.ElementAt(1);
            BBoxRawData[2] = bboxArray.ElementAt(2);
            BBoxRawData[3] = bboxArray.ElementAt(3);

            Rect bbox = GetBBox();
            bbox = new Rect(
                bbox.x / ImageWidth,
                1f - (bbox.y + bbox.height) / ImageHeight,
                bbox.width / ImageWidth,
                bbox.height / ImageHeight);
            NormalizeBBox = bbox;
        }

        public virtual void UpdateBBox(IEnumerable<float> bbox)
        {
            if (CheckValidFaceData())
                RefreshBBoxData(bbox);
        }

        public virtual void UpdateImageData(float width, float height)
        {
            ImageHeight = height;
            ImageWidth = width;
        }

        protected void CheckFaceInfor(IEnumerable<float> faceInfo)
        {
            if (faceInfo == null || mHeadStopped)
                SetFaceValid(false);
            else
                SetFaceValid(true);
        }

        protected bool mCurValidFace = false;

        public virtual void UpdateAnimoji(IEnumerable<float> faceInfo)
        {
            CheckFaceInfor(faceInfo);
            if (mFaceMapData == null || faceInfo == null)
                return;

            FindProperty();
            if (CanUpdateBlendShape)
            {
                // Blend Shape
                ApplyBlendShapesToRig(faceInfo);
            }

            if (CanUpdateEye)
            {
                // Eye Rotation
                ApplyEyeRotationToRig(faceInfo);
            }

            if (CanUpdateHead)
            {
                // Head Rotation
                ApplyHeadRotationToRig(faceInfo);
            }
        }

        protected virtual bool CheckIfHeadStoppedInValid()
        {
            return !mHeadStopped;
        }

        protected virtual void ApplyHeadRotationToRig(IEnumerable<float> faceInfo)
        {
            if (mHeadTransform == null)
                return;
            int rotation_start = 64;

            Vector3 headpose = Vector3.zero;
            headpose.x = faceInfo.ElementAt(rotation_start + mFaceMapData.m_Headpose_reindex[0]);
            headpose.y = faceInfo.ElementAt(rotation_start + mFaceMapData.m_Headpose_reindex[1]);
            headpose.z = faceInfo.ElementAt(rotation_start + mFaceMapData.m_Headpose_reindex[2]);

            float xdelta = Mathf.Abs(headpose.x - mHeadEulerV3.x);
            float ydelta = Mathf.Abs(headpose.y - mHeadEulerV3.y);
            float zdelta = Mathf.Abs(headpose.z - mHeadEulerV3.z);

            mHeadEulerV3 = headpose;

            if (xdelta <= Epsilon && ydelta <= Epsilon && zdelta <= Epsilon)
            {
                mHeadStopped = true;
            }
            else
            {
                mHeadStopped = false;
            }


            float xvalue = (float)(mFaceMapData.m_Headpose_orientation[0] * faceInfo.ElementAt(rotation_start + mFaceMapData.m_Headpose_reindex[0])) + mFaceMapData.m_Headpose_offset[0];
            float yvalue = (float)(mFaceMapData.m_Headpose_orientation[1] * faceInfo.ElementAt(rotation_start + mFaceMapData.m_Headpose_reindex[1])) + mFaceMapData.m_Headpose_offset[1];
            float zvalue = (float)(mFaceMapData.m_Headpose_orientation[2] * faceInfo.ElementAt(rotation_start + mFaceMapData.m_Headpose_reindex[2])) + mFaceMapData.m_Headpose_offset[2];
            Vector3 headRot = new Vector3(xvalue, yvalue, zvalue);
            if (mJustSet)
                mHeadTransform.localEulerAngles = headRot;
            if (mCurValidFace)
            {
                mHeadRotation = headRot;
            }
        }

        protected virtual void ApplyEyeRotationToRig(IEnumerable<float> faceInfo)
        {
            int eye_start = 52;
            int right_eye_start = 58;
            float right_left_ratio = 1.0f;

            if (mLeftEyeTransform != null)
            {
                var rotation = mLeftEyeTransform.localEulerAngles;
                // 需要根据眼球模型的运动方向，需要设置
                // 1. rotation轴
                // 2. 正负号
                // 3. 默认值offset

                // pitch value 上下
                float up_down_pitch_bias = (faceInfo.ElementAt(eye_start + 2) == 0.0f) ? -faceInfo.ElementAt(eye_start + 3) : faceInfo.ElementAt(eye_start + 2);
                float pitchrange = mFaceMapData.m_EyeRange_Pitch;
                float pitch_value = up_down_pitch_bias * (pitchrange / 40f);
                float final_set_pitch_value = mFaceMapData.m_Leftpupil_coef[0] * pitch_value + mFaceMapData.m_Leftpupil_offset[0];

                if (mFaceMapData.m_Leftpupil_reindex[0] == 0)
                    rotation.x = final_set_pitch_value;
                else if (mFaceMapData.m_Leftpupil_reindex[0] == 1)
                    rotation.y = final_set_pitch_value;
                else
                    rotation.z = final_set_pitch_value;

                // yaw value 左右
                float left_right_yaw_bias = (faceInfo.ElementAt(eye_start + 0) == 0.0f) ? -faceInfo.ElementAt(eye_start + 1) * right_left_ratio : faceInfo.ElementAt(eye_start + 0);
                float yawrange = mFaceMapData.m_EyeRange_Yaw;
                float yaw_value = left_right_yaw_bias * (yawrange / 40f);
                float final_set_yaw_value = mFaceMapData.m_Leftpupil_coef[1] * yaw_value + mFaceMapData.m_Leftpupil_offset[1];

                // Debug.Log("pitch range:" + pitchrange + "," + yawrange);

                if (mFaceMapData.m_Leftpupil_reindex[1] == 0)
                    rotation.x = final_set_yaw_value;
                else if (mFaceMapData.m_Leftpupil_reindex[1] == 1)
                    rotation.y = final_set_yaw_value;
                else
                    rotation.z = final_set_yaw_value;

                if (mFaceMapData.m_Leftpupil_reindex[2] == 0)
                    rotation.x = mFaceMapData.m_Leftpupil_offset[2];
                else if (mFaceMapData.m_Leftpupil_reindex[2] == 1)
                    rotation.y = mFaceMapData.m_Leftpupil_offset[2];
                else
                    rotation.z = mFaceMapData.m_Leftpupil_offset[2];

                if (mJustSet)
                    mLeftEyeTransform.localEulerAngles = rotation;
                if (mCurValidFace)
                    mLeftEyeRotation = rotation;
            }

            if (mRightEyeTransform != null)
            {
                var rotation = mRightEyeTransform.localEulerAngles;

                // yaw value
                float left_right_yaw_bias = (faceInfo.ElementAt(right_eye_start + 0) == 0.0f) ? -faceInfo.ElementAt(right_eye_start + 1) * right_left_ratio : faceInfo.ElementAt(right_eye_start + 0);
                float yawrange = mFaceMapData.m_EyeRange_Yaw;
                float yaw_value = left_right_yaw_bias * (yawrange / 40f);
                float final_set_yaw_value = mFaceMapData.m_Rightpupil_coef[1] * yaw_value + mFaceMapData.m_Rightpupil_offset[1];

                // pitch value
                float up_down_pitch_bias = (faceInfo.ElementAt(right_eye_start + 2) == 0.0f) ? -faceInfo.ElementAt(right_eye_start + 3) : faceInfo.ElementAt(right_eye_start + 2);
                float pitchrange = mFaceMapData.m_EyeRange_Pitch;
                float pitch_value = up_down_pitch_bias * (pitchrange / 40f);
                float final_set_pitch_value = mFaceMapData.m_Rightpupil_coef[0] * pitch_value + mFaceMapData.m_Rightpupil_offset[0];


                if (mFaceMapData.m_Rightpupil_reindex[0] == 0)
                    rotation.x = final_set_pitch_value;
                else if (mFaceMapData.m_Rightpupil_reindex[0] == 1)
                    rotation.y = final_set_pitch_value;
                else
                    rotation.z = final_set_pitch_value;

                if (mFaceMapData.m_Rightpupil_reindex[1] == 0)
                    rotation.x = final_set_yaw_value;
                else if (mFaceMapData.m_Rightpupil_reindex[1] == 1)
                    rotation.y = final_set_yaw_value;
                else
                    rotation.z = final_set_yaw_value;

                if (mFaceMapData.m_Rightpupil_reindex[2] == 0)
                    rotation.x = mFaceMapData.m_Leftpupil_offset[2];
                else if (mFaceMapData.m_Rightpupil_reindex[2] == 1)
                    rotation.y = mFaceMapData.m_Leftpupil_offset[2];
                else
                    rotation.z = mFaceMapData.m_Leftpupil_offset[2];

                if (mJustSet)
                    mRightEyeTransform.localEulerAngles = rotation;
                if (mCurValidFace)
                    mRightEyeRotation = rotation;
            }
        }

        protected virtual void ApplyBlendShapesToRig(IEnumerable<float> faceInfo)
        {
            int bsCount = mFaceMapData.m_BlendShapeConfig.Count;
            for (int i = 0; i < bsCount; i++)
            {
                BlendShapeConfig bsc = mFaceMapData.m_BlendShapeConfig[i];
                string meshPath = bsc.m_MeshPath;

                // Need Cache Transform
                Transform trs = this.transform.Find(meshPath);
                if (trs == null)
                    continue;

                SkinnedMeshRenderer meshrender = trs.GetComponent<SkinnedMeshRenderer>();
                int meshBlendShapeCount = meshrender.sharedMesh.blendShapeCount;
                BindProperty[] bps = bsc.m_Binds;
                int bpscount = bps.Length;
                for (int bpsindex = 0; bpsindex < bpscount; bpsindex++)
                {
                    int targetindex = bps[bpsindex].ShapeIndex;
                    if (targetindex < 0 || targetindex >= meshBlendShapeCount)
                        continue;
                    BindProperty bp = bps[bpsindex];
                    if (mRigDrivenEye && (bp.Location == FaceBlendShapeDefine.FaceBlendShape.EyeLookInLeft
                                          || bp.Location == FaceBlendShapeDefine.FaceBlendShape.EyeLookOutLeft
                                          || bp.Location == FaceBlendShapeDefine.FaceBlendShape.EyeLookDownLeft
                                          || bp.Location == FaceBlendShapeDefine.FaceBlendShape.EyeLookUpLeft
                                          || bp.Location == FaceBlendShapeDefine.FaceBlendShape.EyeLookInRight
                                          || bp.Location == FaceBlendShapeDefine.FaceBlendShape.EyeLookOutRight
                                          || bp.Location == FaceBlendShapeDefine.FaceBlendShape.EyeLookUpRight
                                          || bp.Location == FaceBlendShapeDefine.FaceBlendShape.EyeLookDownRight))
                        continue;
                    int faceblendshapeIndex = (int)(bp.Location);
                    float value = faceInfo.ElementAt(0 + faceblendshapeIndex);
                    IEvaluator evaluator = bp.Config.GetEvaluator();
                    float fvalue = evaluator.Evaluate(value);
                    meshrender.SetBlendShapeWeight(targetindex, fvalue);
                }
            }
        }
    }
}