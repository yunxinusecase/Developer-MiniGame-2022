using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MetaVerse.Animoji
{
    [CreateAssetMenu(fileName = "NewYunXinCurveEvaluator", menuName = "YunXin/Animoji/Face Mapper/Evaluator/Curve")]
    public class ValueCurveEvaluator : EvaluatorBasePreset
    {
        [System.Serializable]
        public class Impl : IEvaluator
        {
            [SerializeField]
            AnimationCurve m_Curve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

            public float Evaluate(float value)
            {
                float time = value * 0.01f;
                return Mathf.Clamp(m_Curve.Evaluate(time) * 100f, 0f, 100f);
            }

#if UNITY_EDITOR
            static class Contents
            {
                public static readonly GUIContent Curve = new GUIContent("Curve", "The curve defining a custom evaluation function. It is expected to map values in the domain [0, 1].");
            }

            public float GetHeight()
            {
                return EditorGUIUtility.singleLineHeight;
            }

            public void OnGUI(Rect rect)
            {
                m_Curve = EditorGUI.CurveField(rect, Contents.Curve, m_Curve);
            }
#endif
        }

        [SerializeField]
        Impl m_Evaluator = new Impl();
        public override IEvaluator Evaluator => m_Evaluator;

    }
}

