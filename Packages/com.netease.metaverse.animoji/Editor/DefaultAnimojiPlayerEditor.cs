using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MetaVerse.Animoji.Tools.Editor
{
    [CustomEditor(typeof(DefaultAnimojiPlayer))]
    public class DefaultAnimojiPlayerEditor : UnityEditor.Editor
    {
        private SerializedProperty mEnableHeadRotation;
        private SerializedProperty mEnableBlendShape;
        private SerializedProperty mEnableEye;
        private SerializedProperty mFaceMapData;
        private SerializedProperty mRigDrivenEye;

        private SerializedProperty mLeftEyeTransform;
        private SerializedProperty mRightEyeTransform;
        private SerializedProperty mHeadTransform;

        protected static class Contents
        {
            public static readonly GUIContent EnableHeadRotation = new GUIContent("Enable HeadRotation", "Enable Head Rotation Update");
            public static readonly GUIContent EnableBlendShape = new GUIContent("Enable BlendShape", "Enable Blend Shape Update");
            public static readonly GUIContent EnableEye = new GUIContent("Enable Eye", "Enable Eye Rotation Update");
            public static readonly GUIContent RigDrivenEye = new GUIContent("Enable Rig Driven Eye", "If Enabled ,Use Rig To Driven Eye Rotation, other Use BlendShape Value to Driven Eye Animation");

            public static readonly GUIContent FaceMapData = new GUIContent("Face Map Data", "表情迁移映射配置");

            public static readonly GUIContent LeftEyeTransform = new GUIContent("LeftEyeTransform", "Enable Eye Rotation Update");
            public static readonly GUIContent RightEyeTransform = new GUIContent("RightEyeTransform", "Enable Eye Rotation Update");
            public static readonly GUIContent HeadTransform = new GUIContent("HeadTransform", "Enable Eye Rotation Update");

            public static readonly GUIContent CheckValidTimerTime = new GUIContent("Check InValid Face Time", "Check Invalid Face Time to idle");
            public static readonly GUIContent BlendSpeed = new GUIContent("Blend Speed", "Blend Animation Speed");

            public static readonly GUIContent ClampBoundApprox = new GUIContent("Clamp Bound Approx", "参数值越小，检测越不严格");
            public static readonly GUIContent ClampFaceSize = new GUIContent("Clam Face Size", "参数值越大，检测越严格");
        }

        protected virtual void OnEnable()
        {
            mEnableHeadRotation = serializedObject.FindProperty("mEnableHeadRotation");
            mEnableBlendShape = serializedObject.FindProperty("mEnableBlendShape");
            mEnableEye = serializedObject.FindProperty("mEnableEye");

            mFaceMapData = serializedObject.FindProperty("mFaceMapData");
            mRigDrivenEye = serializedObject.FindProperty("mRigDrivenEye");

            mLeftEyeTransform = serializedObject.FindProperty("mLeftEyeTransform");
            mRightEyeTransform = serializedObject.FindProperty("mRightEyeTransform");
            mHeadTransform = serializedObject.FindProperty("mHeadTransform");
        }


        protected virtual void OnDisable()
        {

        }

        protected virtual void OnValidate()
        {
        }

        private void CreateFaceMapperAsset(string savepath, DefaultAnimojiPlayer player)
        {
            FaceMapperData data = ScriptableObject.CreateInstance<FaceMapperData>();
            FaceMapperData.SetUpFaceMapperDataConfig(data, player);
            AssetDatabase.CreateAsset(data, savepath);
            EditorUtility.SetDirty(data);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            mFaceMapData.objectReferenceValue = data;
            Debug.Log("create face mapper asset success");
        }

        protected void DrawFaceMapDataConfig()
        {
            EditorGUILayout.PropertyField(mFaceMapData, Contents.FaceMapData);

            if (mFaceMapData.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("请指定迁移配置文件FaceMapData，或者创建新的配置文件", MessageType.Warning, true);
                if (GUILayout.Button("创建FaceMapData"))
                {
                    string filename = string.Format("{0}_FaceMapperData", target.name);
                    string path = EditorUtility.SaveFilePanelInProject("Save FaceMapperData", filename, "asset", "Please enter a file name to save Face Mapper Data");
                    Debug.Log("save to path: " + path);
                    if (File.Exists(path))
                    {
                        AssetDatabase.DeleteAsset(path);
                        AssetDatabase.Refresh();
                    }
                    CreateFaceMapperAsset(path, target as DefaultAnimojiPlayer);
                }
            }
            else
            {
                if (GUILayout.Button("打开编辑器"))
                {
                    FaceMapperBuilder.OpenFaceMapperConfigWindow(target as DefaultAnimojiPlayer, mFaceMapData.objectReferenceValue as FaceMapperData);
                }
            }
        }

        protected virtual void DrawHeadEyeGUI()
        {
            EditorGUILayout.PropertyField(mEnableHeadRotation, Contents.EnableHeadRotation);
            EditorGUILayout.PropertyField(mEnableBlendShape, Contents.EnableBlendShape);
            EditorGUILayout.PropertyField(mEnableEye, Contents.EnableEye);
            EditorGUILayout.PropertyField(mRigDrivenEye, Contents.RigDrivenEye);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawHeadEyeGUI();
            DrawFaceMapDataConfig();
            serializedObject.ApplyModifiedProperties();
        }

        private void CreateAnimojiPlayerFaceMapper(AnimojiPlayer player, string savePath)
        {
            Debug.Log("CreateAnimojiPlayerFaceMapper savePath: " + savePath);
        }
    }
}

