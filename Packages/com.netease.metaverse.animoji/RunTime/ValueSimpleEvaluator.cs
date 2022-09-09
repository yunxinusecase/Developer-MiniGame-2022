using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MetaVerse.Animoji
{
    [CreateAssetMenu(fileName = "NewYunXinSimpleEvaluator", menuName = "YunXin/Animoji/Face Mapper/Evaluator/Simple")]
    public class ValueSimpleEvaluator : EvaluatorBasePreset
    {
        [System.Serializable]
        public class Impl : IEvaluator
        {
            enum Clamping
            {
                Hard,
                Soft,
            }

            [SerializeField] public float m_Multiplier = 1f;
            [SerializeField] private float m_Offset = 0f;
            [SerializeField] private float m_Max = 100f;
            [SerializeField] private Clamping m_Clamping = Clamping.Hard;

            public float Evaluate(float value)
            {
                switch (m_Clamping)
                {
                    case Clamping.Hard:
                        return Mathf.Min((m_Multiplier * value) + m_Offset, m_Max);
                    case Clamping.Soft:
                        return SmoothClamp(m_Offset, m_Max, m_Multiplier * value);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            float SmoothClamp(float a, float b, float t)
            {
                if (Mathf.Approximately(a, b))
                    return a;

                // This doesn't use Mathf smoothstep since that uses much higher
                // precision than needed. We remap to only use the last half of the
                // smoothstep curve, as we don't want smoothing near t=0.
                t = Mathf.Clamp01((t - a) / (b - a));
                t = (0.5f * t) + 0.5f;
                return ((t * t * (6f - 4f * t)) - 1f) * (b - a) + a;
            }

#if UNITY_EDITOR
            static class Contents
            {
                public static readonly GUIContent Multiplier = new GUIContent("Multiplier", "The scaling coefficient applied to the blend shape value. " +
                                                                                            "Larger values make the character more expressive.");
                public static readonly GUIContent Offset = new GUIContent("Offset", "Offsets the zero value of the blend shape. " +
                                                                                    "Non-zero values will change the face's resting pose.");
                public static readonly GUIContent Max = new GUIContent("Max", "The maximum value the blend shape can reach. " +
                                                                              "Values larger than 100 allow the blend shape to go past its default extremes, while smaller values constrain them.");
                public static readonly GUIContent Clamping = new GUIContent("Clamping", "Controls how the evaluated blend shape value should behave as it reaches the maximum value. " +
                                                                                        "Soft clamping will ease near the max value, while hard clamping will not.");
            }

            /// <inheritdoc/>
            public float GetHeight()
            {
                return (3 * EditorGUIUtility.singleLineHeight) + (3 * EditorGUIUtility.standardVerticalSpacing);
            }

            /// <inheritdoc/>
            public void OnGUI(Rect rect)
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                m_Multiplier = EditorGUI.Slider(rect, Contents.Multiplier, m_Multiplier, 0f, 10f);

                MetaVerse.Animoji.Tools.Editor.EditorUtil.NextLine(ref rect);
                m_Offset = EditorGUI.Slider(rect, Contents.Offset, m_Offset, -100f, 100f);

                MetaVerse.Animoji.Tools.Editor.EditorUtil.NextLine(ref rect);
                m_Max = EditorGUI.Slider(rect, Contents.Max, m_Max, 0f, 200);

                MetaVerse.Animoji.Tools.Editor.EditorUtil.NextLine(ref rect);
                m_Clamping = (Clamping)EditorGUI.EnumPopup(rect, Contents.Clamping, m_Clamping);
            }
#endif
        }

        [SerializeField]
        Impl m_Evaluator = new Impl();
        public override IEvaluator Evaluator => m_Evaluator;
    }
}
