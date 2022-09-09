using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MetaVerse.Animoji;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Linq;

namespace MetaVerse.Animoji
{
    public class MeshBlendShapeData
    {
        // Blend Shape Index
        public Dictionary<int, PropertyStreamHandle> mBlendShapePropertySceneHandles = new Dictionary<int, PropertyStreamHandle>();
        public Dictionary<int, float> mBlendShapePropertyValues = new Dictionary<int, float>();
    }

    public struct AnimojiAnimationHandleJob : IAnimationJob
    {
        public TransformStreamHandle mHeadHandle;
        public TransformStreamHandle mLeftEyeHandle;
        public TransformStreamHandle mRightEyeHandle;

        // Motion Data
        public Vector3 mHeadEulerAngle;
        public Vector3 mLeftEyeEulerAngle;
        public Vector3 mRightEyeEulerAngle;

        // Blend Weight
        public float mBlendWeight;

        // skinmesh transform path
        public Dictionary<string, MeshBlendShapeData> mBlendShapeDatas;

        public void SetUpTransform(TransformStreamHandle head, TransformStreamHandle left, TransformStreamHandle right)
        {
            mHeadHandle = head;
            mLeftEyeHandle = left;
            mRightEyeHandle = right;
            mBlendWeight = 0.0f;
        }

        public void SetUpBlendShape(Dictionary<string, MeshBlendShapeData> setupdata)
        {
            mBlendShapeDatas = setupdata;
        }

        public void ProcessRootMotion(AnimationStream stream)
        {
        }

        public void ProcessAnimation(AnimationStream stream)
        {
            // Head Animation's Rotation
            AnimationStream animationStream = stream.GetInputStream(0);
            Quaternion headRotation = mHeadHandle.GetLocalRotation(animationStream);
            Quaternion headAnimojiRotation = Quaternion.Euler(mHeadEulerAngle);

            Quaternion headTarget = Quaternion.Slerp(headRotation, headAnimojiRotation, mBlendWeight);
            mHeadHandle.SetLocalRotation(stream, headTarget);

            // Right Eye Animation's Rotation*
            Quaternion leftEyeRotation = mLeftEyeHandle.GetLocalRotation(animationStream);
            Quaternion leftEyeAnimojiRotation = Quaternion.Euler(mLeftEyeEulerAngle);
            Quaternion leftEyeTarget = Quaternion.Slerp(leftEyeRotation, leftEyeAnimojiRotation, mBlendWeight);
            mLeftEyeHandle.SetLocalRotation(stream, leftEyeTarget);

            // Left Eye Animation's Rotation
            Quaternion rightEyeRotation = mRightEyeHandle.GetLocalRotation(animationStream);
            Quaternion rightEyeAnimojiRotation = Quaternion.Euler(mRightEyeEulerAngle);
            Quaternion rightEyeTarget = Quaternion.Slerp(rightEyeRotation, rightEyeAnimojiRotation, mBlendWeight);
            mRightEyeHandle.SetLocalRotation(stream, rightEyeTarget);

            if (mBlendShapeDatas.Count != 0)
            {
                foreach (var bs in mBlendShapeDatas)
                {
                    foreach (var bsvalue in bs.Value.mBlendShapePropertySceneHandles)
                    {
                        int key = bsvalue.Key;
                        if (bs.Value.mBlendShapePropertyValues.ContainsKey(key))
                        {
                            float originValue = bsvalue.Value.GetFloat(animationStream);
                            float animojiValue = bs.Value.mBlendShapePropertyValues[key];

                            float targetValue = Mathf.Lerp(originValue, animojiValue, mBlendWeight);
                            bsvalue.Value.SetFloat(stream, targetValue);
                        }
                    }
                }
            }
        }
    }

    [RequireComponent(typeof(Animator))]
    public class AnimojiPlayerWithAnimator : DefaultAnimojiPlayer
    {
        private PlayableGraph mGraph;
        private AnimationScriptPlayable mAnimationPlayable;
        private bool mValidStatus = false;
        private float mWeight = 0f;

        // invalid face timer
        public float mCheckValidTimerTime = 0.45f;
        public float mBlendSpeed = 4.5f;
        private float mCurValidTime = 0.0f;

        public Dictionary<string, MeshBlendShapeData> mBlendShapeDatas = new Dictionary<string, MeshBlendShapeData>();

        public override void UpdateAnimoji(IEnumerable<float> faceInfo)
        {
            mJustSet = false;
            base.UpdateAnimoji(faceInfo);

            if (mValidStatus)
            {
                var job = mAnimationPlayable.GetJobData<AnimojiAnimationHandleJob>();
                job.mBlendWeight = mWeight;
                job.mHeadEulerAngle = mHeadRotation;
                job.mLeftEyeEulerAngle = mLeftEyeRotation;
                job.mRightEyeEulerAngle = mRightEyeRotation;
                mAnimationPlayable.SetJobData(job);
            }
        }

        protected override void Start()
        {
        }

        private bool IsValidFaceNow()
        {
            bool valid = true;
            if (CheckValidFaceData())
            {
                bool isclose = IsTooCloseToBound(NormalizeBBox, m_ClampBoundApprox);
                bool isfacevalid = IsFaceDisanceValid(NormalizeBBox, m_ClampFaceSize);
                valid = isclose && isfacevalid;
            }
            return valid;
        }

        private bool mFlagHasValidFace = false;

        public override void SetFaceValid(bool validFace)
        {
            if (validFace)
            {
                validFace = IsValidFaceNow();
            }
            float start = mWeight;
            float end = !mFlagHasValidFace ? 0f : 1f;
            float deltatime = Time.deltaTime;
            if (validFace)
            {
                mFlagHasValidFace = true;
                end = 1f;
                mCurValidTime = 0f;
            }
            else
            {
                mCurValidTime += deltatime;
                if (mCurValidTime >= mCheckValidTimerTime)
                    end = 0f;
            }

            mCurValidFace = validFace;
            mWeight = Mathf.Lerp(start, end, deltatime * mBlendSpeed);
        }

        protected override void OnDisable()
        {
            mFlagHasValidFace = false;
            mAnimationPlayable.Destroy();
            mGraph.Destroy();
            mValidStatus = false;
            Debug.Log("ondisable create animation job");
        }

        protected override void ApplyBlendShapesToRig(IEnumerable<float> faceInfo)
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
                    if (mBlendShapeDatas.TryGetValue(meshPath, out MeshBlendShapeData shapedata) && mCurValidFace)
                    {
                        if (shapedata.mBlendShapePropertyValues.TryGetValue(targetindex, out float newvalue))
                            shapedata.mBlendShapePropertyValues[targetindex] = fvalue;
                        else
                            shapedata.mBlendShapePropertyValues.Add(targetindex, fvalue);
                    }
                }
            }
        }

        protected override void OnEnable()
        {
            mFlagHasValidFace = false;
            FindProperty();
            mWeight = 0f;
            var animator = GetComponent<Animator>();
            mGraph = PlayableGraph.Create("AnimojiStreamWithAnimation");
            var output = AnimationPlayableOutput.Create(mGraph, "OutPut", animator);

            var job = new AnimojiAnimationHandleJob();

            TransformStreamHandle head = animator.BindStreamTransform(mHeadTransform);
            TransformStreamHandle left = animator.BindStreamTransform(mLeftEyeTransform);
            TransformStreamHandle right = animator.BindStreamTransform(mRightEyeTransform);
            job.SetUpTransform(head, left, right);

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

                try
                {
                    for (int bpsindex = 0; bpsindex < bpscount; bpsindex++)
                    {
                        string blendshapename = meshrender.sharedMesh.GetBlendShapeName(bps[bpsindex].ShapeIndex);
                        string propertyname = ($"blendShape.{blendshapename}");
                        PropertyStreamHandle handle = animator.BindStreamProperty(trs, typeof(SkinnedMeshRenderer), propertyname);
                        if (mBlendShapeDatas.TryGetValue(meshPath, out MeshBlendShapeData data))
                        {
                        }
                        else
                        {
                            mBlendShapeDatas.Add(meshPath, new MeshBlendShapeData());
                        }

                        mBlendShapeDatas[meshPath].mBlendShapePropertySceneHandles.Add(bps[bpsindex].ShapeIndex, handle);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogError("Create AnimationJob Fatal Error Has Exception: " + exception.Message);
                }
            }

            job.SetUpBlendShape(mBlendShapeDatas);

            mAnimationPlayable = AnimationScriptPlayable.Create(mGraph, job);

            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            AnimatorControllerPlayable acp = AnimatorControllerPlayable.Create(mGraph, controller);

            mAnimationPlayable.AddInput(acp, 0, 1.0f);

            output.SetSourcePlayable(mAnimationPlayable);
            mGraph.Play();
            mValidStatus = true;
            Debug.Log("onenable create animation job");
        }
    }
}