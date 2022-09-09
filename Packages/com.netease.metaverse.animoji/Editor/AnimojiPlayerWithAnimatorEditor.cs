using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MetaVerse.Animoji.Tools.Editor
{
    [CustomEditor(typeof(AnimojiPlayerWithAnimator))]
    public class AnimojiPlayerWithAnimatorEditor : DefaultAnimojiPlayerEditor
    {
        private SerializedProperty mCheckValidTimerTime = null;
        private SerializedProperty mBlendSpeed = null;
        private SerializedProperty m_ClampBoundApprox = null;
        private SerializedProperty m_ClampFaceSize = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            mCheckValidTimerTime = serializedObject.FindProperty("mCheckValidTimerTime");
            mBlendSpeed = serializedObject.FindProperty("mBlendSpeed");
            m_ClampBoundApprox = serializedObject.FindProperty("m_ClampBoundApprox");
            m_ClampFaceSize = serializedObject.FindProperty("m_ClampFaceSize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawHeadEyeGUI();

            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(mCheckValidTimerTime, Contents.CheckValidTimerTime);
            EditorGUILayout.PropertyField(mBlendSpeed, Contents.BlendSpeed);
            EditorGUILayout.PropertyField(m_ClampBoundApprox, Contents.ClampBoundApprox);
            EditorGUILayout.PropertyField(m_ClampFaceSize, Contents.ClampFaceSize);
            EditorGUILayout.Space(5);

            DrawFaceMapDataConfig();
            serializedObject.ApplyModifiedProperties();
        }
    }
}