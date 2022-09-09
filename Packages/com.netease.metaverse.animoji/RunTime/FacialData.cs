using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FaceModule
{
    [CreateAssetMenu(menuName ="Facial/New FacialData")]
    public class FacialData : ScriptableObject
    {
        [System.Serializable]
        public class FacialPart
        {
            public FacialAvatar[] avatarList;
        }
        [System.Serializable]
        public class FacialAvatar
        {
            public int avatarID;
            public string target;
            public Vector3 oriPos;
            public Quaternion oriRotation;

            public string r_target;
            public Vector3 r_OriPos;
            public Quaternion r_OriRotation;
        }

        [System.Serializable]
        public class FacialExpression
        {
            public string target;

            //x,y,z
            public string mulTarget;
            public int mulTargetAvatarID;
            public float[] mulFloat;
            public float[] addFloat;
        }

        [System.Serializable]
        public class FacialConstraint
        {
            public string target;
            //快速访问ID，约束是Bn_head_Center下的子节点
            public int targetChildID;

            //快速访问ID，约束是avatar下的第一个子节点
            public int[] posConstrantIDArray;
            public string[] posConstraintList;
            public float[] posWeight;

            public string[] rotateConstraintList;
            public float[] rotateWeight;
            //快速访问ID，约束是avatar
            public int[] scaleParentIDArray;
        }

        [System.Serializable]
        public class AvatarEmotionConstraint
        {
            public string avatarName;
            public int avatarID;
            public string boneName;
            public int boneChildID;
            public bool isNeedRotate = true;
            public bool isLeft = true;
            public Matrix4x4 localToParentMatrix;
        }
        public FacialConstraint[] facialConstraintArray;
        public FacialExpression[] facialExpressionArray;
        public FacialAvatar[] facialAvatarArray;
        public AvatarEmotionConstraint[] faicalEmConArray;
        public Vector3 facialTouchcolliderPos;
    }
}
