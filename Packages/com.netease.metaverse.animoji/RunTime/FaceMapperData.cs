using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System;
using System.Runtime.InteropServices.WindowsRuntime;

namespace MetaVerse.Animoji
{
    public interface IEvaluator : IDrawable
    {
        float Evaluate(float value);
    }

    public interface IDrawable
    {
#if UNITY_EDITOR
        /// <summary>
        /// Gets the vertical space needed to draw the GUI for this instance.
        /// </summary>
        float GetHeight();

        /// <summary>
        /// Draws the inspector GUI for this instance.
        /// </summary>
        /// <param name="rect">The rect to draw the property in.</param>
        void OnGUI(Rect rect);
#endif
    }

    public abstract class EvaluatorBasePreset : ScriptableObject
    {
        public abstract IEvaluator Evaluator { get; }
    }

    [System.Serializable]
    public class BindConfig : IDrawable
    {
        enum ConfigType
        {
            Simple,
            Curve,
        }

        [SerializeField]
        private ConfigType m_Type = ConfigType.Simple;

        [SerializeField]
        private EvaluatorBasePreset m_EvaluatorPreset = null;

        [SerializeField]
        private ValueSimpleEvaluator.Impl m_SimpleEvaluator = new ValueSimpleEvaluator.Impl();
        [SerializeField]
        private ValueCurveEvaluator.Impl m_CurveEvaluator = new ValueCurveEvaluator.Impl();

        public ValueSimpleEvaluator.Impl GetSimpleEvaluator
        {
            get { return m_SimpleEvaluator; }
        }

        public IEvaluator GetEvaluator()
        {
            if (m_EvaluatorPreset != null)
                return m_EvaluatorPreset.Evaluator;
            switch (m_Type)
            {
                case ConfigType.Simple:
                    return m_SimpleEvaluator;
                case ConfigType.Curve:
                    return m_CurveEvaluator;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
#if UNITY_EDITOR
        static class Contents
        {
            public static readonly GUIContent OverrideSmoothing = new GUIContent("Override Smoothing", "Whether the smoothing value set on this binding overrides the default value for the mapper.");
            public static readonly GUIContent Smoothing = new GUIContent("Smoothing", "The amount of smoothing to apply to the blend shape value. " +
                                                                                      "It can help reduce jitter in the face capture, but it will also smooth out fast motions.");
            public static readonly GUIContent EvaluatorPreset = new GUIContent("Evaluator Preset", "A preset evaluation function to use. " +
                                                                                                   "If none is assigned, a new function must be configured for this blend shape.");
            public static readonly GUIContent Type = new GUIContent("Type", "The type of evaluation function to use when a preset is not assigned.");
        }

        public float GetHeight()
        {
            const int lines = 3;
            var height = (lines * EditorGUIUtility.singleLineHeight) + ((lines - 1) * EditorGUIUtility.standardVerticalSpacing);

            if (m_EvaluatorPreset == null)
            {
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;
                height += EditorGUIUtility.standardVerticalSpacing + GetEvaluator().GetHeight();
            }

            return height;
        }

        public void OnGUI(Rect rect)
        {
            var line = rect;
            line.height = EditorGUIUtility.singleLineHeight;
            m_EvaluatorPreset = EditorGUI.ObjectField(line, Contents.EvaluatorPreset, m_EvaluatorPreset, typeof(EvaluatorBasePreset), false) as EvaluatorBasePreset;
            if (m_EvaluatorPreset == null)
            {
                MetaVerse.Animoji.Tools.Editor.EditorUtil.NextLine(ref line);
                m_Type = (ConfigType)EditorGUI.EnumPopup(line, Contents.Type, m_Type);

                var evaluatorRect = rect;
                evaluatorRect.yMin = line.yMax;
                GetEvaluator().OnGUI(evaluatorRect);
            }
        }
#endif
    }

    [System.Serializable]
    public class BlendShapeConfig
    {
        [SerializeField]
        public string m_MeshPath = string.Empty;
        [SerializeField]
        public BindProperty[] m_Binds = new BindProperty[0];
#pragma warning disable CS0414
        [SerializeField, HideInInspector]
        bool m_IsExpanded = true;
#pragma warning restore CS0414
    }

    [System.Serializable]
    public class BindProperty
    {
        [SerializeField] FaceBlendShapeDefine.FaceBlendShape m_Location;
        public FaceBlendShapeDefine.FaceBlendShape Location => m_Location;
        [SerializeField]
        public BindConfig m_Config = null;

#pragma warning disable CS0414
        [SerializeField, HideInInspector]
        bool m_IsExpanded = false;
#pragma warning restore CS0414

        public bool IsExpanded => m_IsExpanded;
        [SerializeField] private int m_ShapeIndex;
        public int ShapeIndex => m_ShapeIndex;

        public BindConfig Config => m_Config;

        public BindProperty(
            FaceBlendShapeDefine.FaceBlendShape location,
            int index,
            BindConfig config,
            bool expanded)
        {
            m_Location = location;
            m_ShapeIndex = index;
            m_IsExpanded = expanded;
            m_Config = config;
        }
    }

    [CreateAssetMenu(fileName = "New YunXin FaceMapper", menuName = "YunXin/Animoji/Face Mapper/Blend Shape Config")]
    public class FaceMapperData : ScriptableObject
    {
        public string m_MapperName = string.Empty;
        // Each Blend Shape Config
        public List<BlendShapeConfig> m_BlendShapeConfig = new List<BlendShapeConfig>();

        // -------------------More Controller Params
        //[SerializeField]
        //public float m_LeftEyeHorizontalCoef = 1f;
        //[SerializeField]
        //public float m_LeftEyeVerticalCoef = 1f;
        //[SerializeField]
        //public float m_RightEyeHorizontalCoef = 1f;
        //[SerializeField]
        //public float m_RightEyeVerticalCoef = 1f;

        //[SerializeField, Range(0f, 1f)]
        //public float m_ClampOverlapSizeRatio;
        //[SerializeField, Range(0f, 0.5f)]
        //public float m_ClampBoundApprox;
        //[SerializeField, Range(0f, 90f)]
        //public float m_ClampNeckRotHor;
        //[SerializeField, Range(0f, 90f)]
        //public float m_ClampNeckRotVer;
        //[SerializeField, Range(0f, 1f)]
        //public float m_ClampFaceSize;
        // -------------------More Controller Params

        // ×óÑÛ¾¦¹Ç÷À
        public string m_LeftEyeBonePath = string.Empty;
        // ÓÒÑÛ¾¦¹Ç÷À
        public string m_RightEyeBonePath = string.Empty;
        // Í·²¿Ðý×ª¹Ç÷À
        public string m_HeadBonePath = string.Empty;

        // 0 : pitch
        // 1 : yaw
        // 2 : roll

        public float m_EyeRange_Pitch = 30;
        public float m_EyeRange_Yaw = 40;

        public int[] m_Leftpupil_reindex = { 2, 0, 1 };
        public float[] m_Leftpupil_offset = { -90.0f, 0, 0 };
        public float[] m_Leftpupil_coef = { 1.0f, -1.0f };

        public int[] m_Rightpupil_reindex = { 2, 0, 1 };
        public float[] m_Rightpupil_offset = { -90.0f, 0, 0 };
        public float[] m_Rightpupil_coef = { 1.0f, -1.0f };

        public int[] m_Headpose_reindex = { 0, 1, 2 };
        public int[] m_Headpose_orientation = { -1, 1, -1 };
        public float[] m_Headpose_offset = { 0, 0, 5 };

#if UNITY_EDITOR
        public static void SetUpFaceMapperDataConfig(FaceMapperData data, AnimojiPlayer player)
        {
            if (data == null || player == null)
            {
                Debug.LogError("SetUpFaceMapperDataConfig FaceMapperData is null or AnimojiPlayer is null");
                return;
            }
            data.m_MapperName = player.name;
            // config blend shape
            // config head
            // config left right eye

            /*
             * Í·²¿Ðý×ª¸ù½Úµã½Úµã
             * ×óÑÛÇò¹Ç÷À½Úµã
             * ÓÒÑÛÇò¹Ç÷À½Úµã
             */

            Vector3 preVector3 = player.transform.localEulerAngles;
            Transform[] bones = new Transform[3] { null, null, null };
            string[] boneperhappaths = new string[3]
            {
                "Neck/Head",
                "Neck/Head/LeftEye",
                "Neck/Head/RightEye"
            };

            FindTransfom(player.transform, player.transform, ref boneperhappaths, ref bones);
            if (bones[0] != null)
            {
                data.m_HeadBonePath = AnimationUtility.CalculateTransformPath(bones[0], player.transform);
                Vector3 angles = UnityEditor.TransformUtils.GetInspectorRotation(bones[0]);
                data.m_Headpose_offset[0] = angles[0];
                data.m_Headpose_offset[1] = angles[1];
                data.m_Headpose_offset[2] = angles[2];
            }

            if (bones[1] != null)
            {

                Vector3 angles = UnityEditor.TransformUtils.GetInspectorRotation(bones[1]);
                data.m_Leftpupil_offset[0] = angles[0];
                data.m_Leftpupil_offset[1] = angles[1];
                data.m_Leftpupil_offset[2] = angles[2];
                data.m_LeftEyeBonePath = AnimationUtility.CalculateTransformPath(bones[1], player.transform);
            }

            if (bones[2] != null)
            {
                Vector3 angles = UnityEditor.TransformUtils.GetInspectorRotation(bones[2]);
                data.m_Rightpupil_offset[0] = angles[0];
                data.m_Rightpupil_offset[1] = angles[1];
                data.m_Rightpupil_offset[2] = angles[2];
                data.m_RightEyeBonePath = AnimationUtility.CalculateTransformPath(bones[2], player.transform);
            }
            player.transform.localEulerAngles = preVector3;
            data.m_Rightpupil_coef[0] = -1.0f;
            data.m_Rightpupil_coef[1] = 1f;

            data.m_Leftpupil_coef[0] = -1.0f;
            data.m_Leftpupil_coef[1] = 1f;

            data.m_Headpose_orientation[0] = 1;
            data.m_Headpose_orientation[1] = 1;
            data.m_Headpose_orientation[2] = 1;

            data.m_Headpose_reindex = new int[3] { 0, 1, 2 };
            data.m_Leftpupil_reindex = new int[3] { 0, 1, 2 };
            data.m_Rightpupil_reindex = new int[3] { 0, 1, 2 };
        }

        private static void FindTransfom(Transform root, Transform node, ref string[] paths, ref Transform[] bones)
        {
            if (bones == null || root == null || node == null)
                return;
            if (bones.Length != 3)
                return;

            bool findall = true;
            foreach (var bone in bones)
            {
                if (bone == null)
                {
                    findall = false;
                    break;
                }
            }
            if (findall)
                return;

            int count = node.childCount;
            for (int i = 0; i < count; i++)
            {
                Transform trs = node.GetChild(i);
                string path = AnimationUtility.CalculateTransformPath(trs, root);
                if (bones[0] == null && path.EndsWith(paths[0]))
                    bones[0] = trs;
                if (bones[1] == null && path.EndsWith(paths[1]))
                    bones[1] = trs;
                if (bones[2] == null && path.EndsWith(paths[2]))
                    bones[2] = trs;
                FindTransfom(root, trs, ref paths, ref bones);
            }
        }
#endif
    }
}
