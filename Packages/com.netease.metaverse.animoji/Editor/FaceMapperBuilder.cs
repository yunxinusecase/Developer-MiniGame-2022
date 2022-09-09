using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using MetaVerse.Animoji;
using UnityEditorInternal;
using System.IO;

namespace MetaVerse.Animoji.Tools.Editor
{
    public class FaceMapperBuilder : EditorWindow
    {
        private Vector2 scrollPos;
        private Vector2 scrollHeaderPos;

        // Add menu named "My Window" to the Window menu
        // [MenuItem("YunXin/Face Mapper Config")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            FaceMapperBuilder window = (FaceMapperBuilder)EditorWindow.GetWindow(typeof(FaceMapperBuilder), true, "Face Mapper Config");
            window.Show();
        }

        //
        // Open FaceMapper Config Window
        //
        public static void OpenFaceMapperConfigWindow(DefaultAnimojiPlayer player, FaceMapperData mapdata)
        {
            FaceMapperBuilder window = (FaceMapperBuilder)EditorWindow.GetWindow(typeof(FaceMapperBuilder), true, "Face Mapper Config");
            window.Show();
            window.SetData(player, mapdata);
            Debug.Log("Open FaceMapper Config Window");
        }

        private void SetData(DefaultAnimojiPlayer player, FaceMapperData mapdata)
        {
            mTarget = player;
            mFaceMapperData = mapdata;
            SwitchMapperData(mapdata);
        }

        private DefaultAnimojiPlayer mTarget;

        private Transform mTransformLeftEyeBone;
        private Transform mTransformRightEyeBone;
        private Transform mTransformHeadBone;

        private FaceMapperData mFaceMapperData;
        private string mFaceMapperDataPath = string.Empty;
        private bool mValid = false;
        private bool Valid => mValid;

        private SerializedObject mSerializedObject = null;
        private SerializedProperty mSPMapperName = null;
        private SerializedProperty mSPBlendShapeConfig = null;

        private SerializedProperty m_EyeRange_Pitch = null;
        private SerializedProperty m_EyeRange_Yaw = null;

        public SerializedProperty m_LeftEyeBonePath = null;
        public SerializedProperty m_RightEyeBonePath = null;
        public SerializedProperty m_HeadBonePath = null;

        public SerializedProperty m_Leftpupil_reindex = null;
        public SerializedProperty m_Leftpupil_offset = null;
        public SerializedProperty m_Leftpupil_coef = null;

        public SerializedProperty m_Rightpupil_reindex = null;
        public SerializedProperty m_Rightpupil_offset = null;
        public SerializedProperty m_Rightpupil_coef = null;

        public SerializedProperty m_Headpose_reindex = null;
        public SerializedProperty m_Headpose_orientation = null;
        public SerializedProperty m_Headpose_offset = null;

        static class Contents
        {
            public static readonly GUIContent EyesHeader = new GUIContent("Eye Config - —€æ¶≈‰÷√");
            public static readonly GUIContent HeadHeader = new GUIContent("Head Config - Õ∑≤ø≈‰÷√");
            public static readonly GUIContent LeftEyeHeader = new GUIContent("Left Eye Config - ◊Û—€æ¶≈‰÷√");
            public static readonly GUIContent RightEyeHeader = new GUIContent("Right Eye Config - ”“—€æ¶≈‰÷√");
            public static readonly GUIContent BlendShapesMappingHeader = new GUIContent("Blend Shape Mapping - ±Ì«È”≥…‰≈‰÷√");
            public static readonly GUIContent AddEmpty = new GUIContent("Empty");
            public static readonly float MenuButtonWidth = 16f;
            public static readonly GUIStyle RendererPathHeaderStyle = "RL FooterButton";
            public static readonly GUIContent MenuIcon = EditorGUIUtility.TrIconContent("_Menu");
            public static readonly GUIContent InValidContent = new GUIContent("Invalid Window Please Reopen «Î÷ÿ–¬¥Úø™¥∞ø⁄");

            public static readonly GUIContent Reinitialize = new GUIContent("Initialize",
                "Clear the mappings and initialize from the currently assigned rig prefab.");

            public static readonly GUIContent DeleteMappings =
                new GUIContent("Delete", "Delete the mappings for this renderer.");

            public static readonly GUIContent LeftEyeBoneHeader = new GUIContent("LeftEyeBonePath");
            public static readonly GUIContent LeftEyeTransformHeader = new GUIContent("Left Eye Transfrom");
            public static readonly GUIContent RightEyeTransformHeader = new GUIContent("Right Eye Transfrom");
            public static readonly GUIContent HeadTransformHeader = new GUIContent("Head Transfrom");
            public static readonly GUIContent RightEyeBoneHeader = new GUIContent("RightEyeBonePath");
            public static readonly GUIContent HeadBoneHeader = new GUIContent("HeadBonePath");
            public static readonly GUIContent EyePitchRange = new GUIContent("Eye Rig Pitch Range");
            public static readonly GUIContent EyeYawRange = new GUIContent("Eye Rig Yaw Range");
            public static readonly float EditPathButtonWidth = 20f;
            public static readonly GUIContent[] AnimojiEuler = new GUIContent[3]
            {
                new GUIContent("X Map To "),
                new GUIContent("Y Map To "),
                new GUIContent("Z Map To ")
            };

            public static readonly GUIContent[] SignTips = new GUIContent[3]
            {
                new GUIContent("Sign Of Pitch"),
                new GUIContent("Sign Of Yaw"),
                new GUIContent("Sign of Roll"),
            };

            public static readonly string[] OffSetTips = new string[3]
            {
                "Pitch OffSet Rot's ",
                "Yaw OffSet Rot's ",
                "Roll OffSet Rot's ",
            };

            public static readonly GUIContent RefreshValue = new GUIContent("Refresh");

            public static readonly GUIContent[] AnimojiEyeEuler = new GUIContent[3]
            {
                new GUIContent("Pitch Map To "),
                new GUIContent("Yaw Map To "),
                new GUIContent("Roll Map To ")
            };

            public static string LeftEyeAnimojiEuler = "Left Eye \nPicth:…œœ¬\nYaw:◊Û”“\nRoll:–˝◊™";
        }

        private void ResetWindow()
        {
            mTransformLeftEyeBone = null;
            mTransformRightEyeBone = null;
            mTransformHeadBone = null;
        }

        private void SwitchMapperData(FaceMapperData data)
        {
            if (data == null || mTarget == null)
            {
                mSerializedObject = null;
                mSPMapperName = null;
                ResetWindow();
                mValid = false;
                return;
            }
            else
            {
                ResetWindow();
                mValid = true;
                m_MappingLists.Clear();
                int count = mFaceMapperData.m_BlendShapeConfig.Count;
                for (int i = 0; i < count; i++)
                {
                    int length = mFaceMapperData.m_BlendShapeConfig[i].m_Binds.Length;
                }

                mFaceMapperDataPath = AssetDatabase.GetAssetPath(mFaceMapperData);
                mSerializedObject = new SerializedObject(mFaceMapperData);
                mSPMapperName = mSerializedObject.FindProperty("m_MapperName");
                mSPBlendShapeConfig = mSerializedObject.FindProperty("m_BlendShapeConfig");
                m_Leftpupil_reindex = mSerializedObject.FindProperty("m_Leftpupil_reindex");
                m_Leftpupil_offset = mSerializedObject.FindProperty("m_Leftpupil_offset");
                m_Leftpupil_coef = mSerializedObject.FindProperty("m_Leftpupil_coef");
                m_Rightpupil_reindex = mSerializedObject.FindProperty("m_Rightpupil_reindex");
                m_Rightpupil_offset = mSerializedObject.FindProperty("m_Rightpupil_offset");
                m_Rightpupil_coef = mSerializedObject.FindProperty("m_Rightpupil_coef");
                m_Headpose_reindex = mSerializedObject.FindProperty("m_Headpose_reindex");
                m_Headpose_orientation = mSerializedObject.FindProperty("m_Headpose_orientation");
                m_Headpose_offset = mSerializedObject.FindProperty("m_Headpose_offset");
                m_LeftEyeBonePath = mSerializedObject.FindProperty("m_LeftEyeBonePath");
                m_RightEyeBonePath = mSerializedObject.FindProperty("m_RightEyeBonePath");
                m_HeadBonePath = mSerializedObject.FindProperty("m_HeadBonePath");
                m_EyeRange_Pitch = mSerializedObject.FindProperty("m_EyeRange_Pitch");
                m_EyeRange_Yaw = mSerializedObject.FindProperty("m_EyeRange_Yaw");

                string leftpath = m_LeftEyeBonePath.GetValue<string>();
                mTransformLeftEyeBone = mTarget.transform.Find(leftpath);
                string rightpath = m_RightEyeBonePath.GetValue<string>();
                mTransformRightEyeBone = mTarget.transform.Find(rightpath);
                string headpath = m_HeadBonePath.GetValue<string>();
                mTransformHeadBone = mTarget.transform.Find(headpath);

                Repaint();
            }
        }

        // ªÊ÷∆”≥…‰πÿœµ
        void DrawAnimojiEuler(SerializedProperty property, bool righteye, bool head = true)
        {
            int[] values = property.GetValue<int[]>();
            if (values == null || values.Length != 3)
            {
                property.SetValue(new int[] { 0, 1, 2 });
                return;
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (head)
                    {
                        // DrawProperty(m_Headpose_orientation, "HeadPoseOrientation");
                        values[i] = (int)((AnimojiEuler)(EditorGUILayout.EnumPopup(Contents.AnimojiEuler[i], (AnimojiEuler)values[i])));
                        SerializedProperty sp = m_Headpose_orientation.GetArrayElementAtIndex(i);
                        EditorGUILayout.PropertyField(sp, new GUIContent("Rotation Sign +-∑˚∫≈"));
                    }
                    else
                    {
                        values[i] = (int)((AnimojiEyeEuler)(EditorGUILayout.EnumPopup(Contents.AnimojiEyeEuler[i], (AnimojiEyeEuler)values[i])));
                        // Just Pitch and Yaw Need Set Sign
                        if (i >= 0 && i <= 1)
                        {
                            if (righteye)
                            {
                                SerializedProperty sp = m_Rightpupil_coef.GetArrayElementAtIndex(i);
                                EditorGUILayout.PropertyField(sp, new GUIContent("Rotation Sign +-∑˚∫≈"));
                            }
                            else
                            {
                                SerializedProperty sp = m_Leftpupil_coef.GetArrayElementAtIndex(i);
                                EditorGUILayout.PropertyField(sp, new GUIContent("Rotation Sign +-∑˚∫≈"));
                            }

                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        void DrawProperty(SerializedProperty property, string content)
        {
            EditorGUILayout.PropertyField(property, new GUIContent(content), true);
        }

        void DrawProperty(SerializedProperty property, GUIContent content)
        {
            EditorGUILayout.PropertyField(property, content, true);
        }

        private bool IsRenderUsed(SkinnedMeshRenderer mesh, DefaultAnimojiPlayer target, FaceMapperData inputData)
        {
            if (mesh == null || target == null || inputData == null)
                return true;

            int count = inputData.m_BlendShapeConfig.Count;
            for (int i = 0; i < count; i++)
            {
                BlendShapeConfig config = inputData.m_BlendShapeConfig[i];

                string path = config.m_MeshPath;
                Transform trs = target.transform.Find(path);
                if (trs == null)
                    continue;
                SkinnedMeshRenderer skin = trs.GetComponent<SkinnedMeshRenderer>();
                if (skin != null && skin.sharedMesh == mesh.sharedMesh)
                    return true;
            }
            return false;
        }

        private void Init(SerializedProperty rendererMapping, string newPath, Mesh mesh)
        {
            Debug.Log("Init");
            m_MappingLists.Clear();
            float m_ShapeMatchTolerance = 0.89f;
            var mapping = rendererMapping;
            var binds = mapping.FindPropertyRelative("m_Binds");
            mapping.FindPropertyRelative("m_MeshPath").stringValue = newPath;
            binds.arraySize = 0;
            if (mesh == null)
                return;

            var locationNames = FaceBlendShapeDefine.Shapes.Select(s => s.ToString()).ToArray();
            var meshBlendShapes = BlendShapeUtil.GetBlendShapeNames(mesh);
            foreach (var (indexA, indexB) in BlendShapeUtil.FindMatches(locationNames, meshBlendShapes, m_ShapeMatchTolerance))
            {
                var bind = binds.GetArrayElementAtIndex(binds.arraySize++);
                var location = BlendShapeUtil.GetLocation(locationNames[indexA]);
                var config = new BindConfig();
                bind.SetValue(new BindProperty(location, indexB, config, true));
            }
            mSerializedObject.ApplyModifiedProperties();
        }

        private void AddRenderShapeMapping(
            DefaultAnimojiPlayer player,
            string newPath,
            Mesh mesh)
        {
            var mapping = mSPBlendShapeConfig.GetArrayElementAtIndex(mSPBlendShapeConfig.arraySize++);
            Init(mapping, newPath, mesh);
            mSerializedObject.ApplyModifiedProperties();
        }

        static ReorderableList.Defaults s_ListDefaults;

        private void DrawRendererMapping(DefaultAnimojiPlayer player, SerializedProperty rendererMapping, int index)
        {
            EditorGUILayout.Space();
            var headerRect = GUILayoutUtility.GetRect(0f, 20f, GUILayout.ExpandWidth(true));
            if (s_ListDefaults == null)
                s_ListDefaults = new ReorderableList.Defaults();

            s_ListDefaults.DrawHeaderBackground(headerRect);

            var path = rendererMapping.FindPropertyRelative("m_MeshPath");

            // draw the dropdown menu
            var menuButtonRect = new Rect(headerRect)
            {
                xMin = headerRect.xMax - 2f - Contents.MenuButtonWidth,
                xMax = headerRect.xMax - 2f,
                y = headerRect.y + 2f,
            };

            if (GUI.Button(menuButtonRect, Contents.MenuIcon, Contents.RendererPathHeaderStyle))
            {
                var menu = new GenericMenu();

                if (TryGetMesh(mTarget, rendererMapping, out var m, false))
                {
                    menu.AddItem(Contents.Reinitialize, false, () =>
                    {
                        Init(rendererMapping, path.stringValue, m);
                        mSerializedObject.ApplyModifiedProperties();
                    });
                }
                else
                {
                    menu.AddDisabledItem(Contents.Reinitialize, false);
                }

                menu.AddItem(Contents.DeleteMappings, false, () =>
                {
                    mSPBlendShapeConfig.DeleteArrayElementAtIndex(index);
                    mSerializedObject.ApplyModifiedProperties();
                });

                menu.ShowAsContext();
            }

            var editPathButtonRect = new Rect(headerRect)
            {
                xMin = menuButtonRect.xMin - 2f - Contents.EditPathButtonWidth,
                xMax = menuButtonRect.xMin - 2f,
                y = headerRect.y + 2f,
            };

            // draw the foldout that shows the mappings list
            var pathRect = new Rect(headerRect)
            {
                xMax = editPathButtonRect.xMin
            };

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                // when the path input field is shown we need to prevent the foldout from using the GUI events
                var foldoutRect = pathRect;
                if (false)
                {
                    foldoutRect.width = 20f;
                }

                if (TryGetMappingList(mTarget, rendererMapping, out var list))
                {
                    var rendererName = Path.GetFileName(path.stringValue);
                    var expanded = EditorGUI.Foldout(foldoutRect, list.IsExpanded, rendererName, true);

                    if (change.changed)
                        list.IsExpanded = expanded;
                }
            }
            if (TryGetMappingList(mTarget, rendererMapping, out var mappingList))
            {
                if (!mappingList.IsExpanded)
                    return;
                mappingList.OnGUI();
            }
        }

        readonly Dictionary<string, MappingEditorList> m_MappingLists = new Dictionary<string, MappingEditorList>();

        private bool TryGetMappingList(DefaultAnimojiPlayer player, SerializedProperty rendererMapping, out MappingEditorList list)
        {
            if (!TryGetMesh(player, rendererMapping, out var mesh, false))
            {
                list = null;
                return false;
            }

            var listId = $"{player.GetInstanceID()}/{mesh.GetInstanceID()}";
            if (!m_MappingLists.TryGetValue(listId, out list))
            {
                var blendShapes = BlendShapeUtil.GetBlendShapeNames(mesh)
                    .Select(s => new GUIContent(s))
                    .ToArray();
                list = new MappingEditorList(rendererMapping, null, blendShapes);
                m_MappingLists.Add(listId, list);
            }

            return true;
        }

        private bool TryGetMesh(
            DefaultAnimojiPlayer player,
            SerializedProperty rendererMapping,
            out Mesh mesh,
            bool showMessages)
        {
            var path = rendererMapping.FindPropertyRelative("m_MeshPath");
            var transform = player.transform.Find(path.stringValue);
            if (transform == null || !transform.TryGetComponent<SkinnedMeshRenderer>(out var renderer))
            {
                if (showMessages)
                    EditorGUILayout.HelpBox($"Assign a unique renderer from the rig prefab.", MessageType.Info);
                mesh = default;
                return false;
            }
            mesh = renderer.sharedMesh;
            if (mesh == null)
            {
                if (showMessages)
                    EditorGUILayout.HelpBox($"This renderer does not have a mesh assigned.", MessageType.Warning);
                return false;
            }

            if (mesh.blendShapeCount <= 0)
            {
                if (showMessages)
                    EditorGUILayout.HelpBox($"The mesh assigned to this renderer does not have any blend shapes.", MessageType.Warning);
                return false;
            }
            return true;
        }

        void OnEnable()
        {
            m_MappingLists.Clear();
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        private void OnPlayModeChanged(PlayModeStateChange state)
        {
            Debug.Log("OnPlayModeChanged() state: " + state);
            SetData(null, null);
        }

        private void CloseWindow()
        {
            Close();
        }

        void OnUndoRedoPerformed()
        {
            Debug.Log("OnUndoRedoPerformed()");
            m_MappingLists.Clear();
            Repaint();
        }

        void OnValidate()
        {
        }

        void OnFocus()
        {
            SwitchMapperData(mFaceMapperData);
        }

        void OnGUI()
        {
            if (!Valid)
            {
                EditorGUILayout.LabelField(Contents.InValidContent, EditorStyles.boldLabel);
                return;
            }

            if (mSerializedObject != null)
                mSerializedObject.Update();

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);

            EditorGUI.BeginDisabledGroup(true);
            mTarget = (DefaultAnimojiPlayer)EditorGUILayout.ObjectField("Target", mTarget, typeof(DefaultAnimojiPlayer), true);
            mFaceMapperData = (FaceMapperData)EditorGUILayout.ObjectField("Mapper", mFaceMapperData, typeof(FaceMapperData), false);
            EditorGUI.EndDisabledGroup();
            string datapath = mFaceMapperDataPath;
            datapath = EditorGUILayout.TextField("Asset Path", datapath);
            DrawProperty(mSPMapperName, "Mapper Name");
            bool drawFaceMapperData = false;
            drawFaceMapperData = mSerializedObject != null && mFaceMapperData != null;
            if (drawFaceMapperData)
            {
                EditorGUILayout.Space(10);
                DrawTransforms();
                EditorGUILayout.Space(10);

                // EditorGUILayout.LabelField(Contents.EyesHeader, EditorStyles.boldLabel);
                DrawLabelWithColorInBox(Contents.EyesHeader, Color.green);
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawProperty(m_EyeRange_Pitch, Contents.EyePitchRange);
                    DrawProperty(m_EyeRange_Yaw, Contents.EyeYawRange);
                    DrawLabelWithColorInBox(Contents.LeftEyeHeader, Color.green);
                    DrawAnimojiEuler(m_Leftpupil_reindex, false, false);
                    int size = m_Leftpupil_offset.arraySize;
                    for (int i = 0; i < size; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        SerializedProperty spindex = m_Leftpupil_reindex.GetArrayElementAtIndex(i);
                        AnimojiEyeEuler index = (AnimojiEyeEuler)spindex.GetValue<int>();
                        string msg = Contents.OffSetTips[i] + (index).ToString();
                        SerializedProperty sp = m_Leftpupil_offset.GetArrayElementAtIndex(i);
                        EditorGUILayout.PropertyField(sp, new GUIContent(msg));
                        if (GUILayout.Button(Contents.RefreshValue))
                        {
                            if (mTransformLeftEyeBone != null)
                            {
                                Vector3 rot = UnityEditor.TransformUtils.GetInspectorRotation(mTransformLeftEyeBone);
                                if (index == AnimojiEyeEuler.X)
                                    sp.SetValue(rot.x);
                                else if (index == AnimojiEyeEuler.Y)
                                    sp.SetValue(rot.y);
                                else if (index == AnimojiEyeEuler.Z)
                                    sp.SetValue(rot.z);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    DrawLabelWithColorInBox(Contents.RightEyeHeader, Color.green);
                    DrawAnimojiEuler(m_Rightpupil_reindex, true, false);
                    size = m_Rightpupil_offset.arraySize;
                    for (int i = 0; i < size; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        SerializedProperty spindex = m_Leftpupil_reindex.GetArrayElementAtIndex(i);
                        AnimojiEyeEuler index = (AnimojiEyeEuler)spindex.GetValue<int>();
                        string msg = Contents.OffSetTips[i] + (index).ToString();
                        SerializedProperty sp = m_Rightpupil_offset.GetArrayElementAtIndex(i);
                        EditorGUILayout.PropertyField(sp, new GUIContent(msg));
                        if (GUILayout.Button(Contents.RefreshValue))
                        {
                            if (mTransformRightEyeBone != null)
                            {
                                Vector3 rot = UnityEditor.TransformUtils.GetInspectorRotation(mTransformRightEyeBone);
                                if (index == AnimojiEyeEuler.X)
                                    sp.SetValue(rot.x);
                                else if (index == AnimojiEyeEuler.Y)
                                    sp.SetValue(rot.y);
                                else if (index == AnimojiEyeEuler.Z)
                                    sp.SetValue(rot.z);
                            }
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                EditorGUILayout.Space(10);
                DrawLabelWithColorInBox(Contents.HeadHeader, Color.green);
                EditorGUILayout.LabelField(Contents.HeadHeader, EditorStyles.boldLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawAnimojiEuler(m_Headpose_reindex, false);
                    int size = m_Headpose_offset.arraySize;
                    for (int i = 0; i < size; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        SerializedProperty sp = m_Headpose_offset.GetArrayElementAtIndex(i);
                        string msg = "";
                        float fvalue = 0f;
                        if (mTransformHeadBone != null)
                        {
                            Vector3 rot = UnityEditor.TransformUtils.GetInspectorRotation(mTransformHeadBone);
                            if (i == 0)
                            {
                                msg = "OffSet Rotation's X ";
                                fvalue = rot.x;
                            }
                            else if (i == 1)
                            {
                                msg = "OffSet Rotation's Y ";
                                fvalue = rot.y;
                            }
                            else if (i == 2)
                            {
                                msg = "OffSet Rotation's Z";
                                fvalue = rot.z;
                            }
                        }
                        EditorGUILayout.PropertyField(sp, new GUIContent(msg));
                        if (GUILayout.Button(Contents.RefreshValue))
                            sp.SetValue(fvalue);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.Space(15);
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    DrawLabelWithColorInBox(Contents.BlendShapesMappingHeader, Color.green);
                    using (new EditorGUI.IndentLevelScope())
                    {
                        for (var i = 0; i < mSPBlendShapeConfig.arraySize; ++i)
                        {
                            var rendererMapping = mSPBlendShapeConfig.GetArrayElementAtIndex(i);
                            DrawRendererMapping(mTarget, rendererMapping, i);
                        }

                        if (GUILayout.Button("Add Render"))
                        {
                            var menu = new GenericMenu();

                            var skinnedMeshRenderers = mTarget.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                                .Where(r => r.sharedMesh != null && r.sharedMesh.blendShapeCount > 0 && !IsRenderUsed(r, mTarget, mFaceMapperData))
                                .ToArray();

                            foreach (var renderer in skinnedMeshRenderers)
                            {
                                menu.AddItem(new GUIContent(renderer.name), false, () =>
                                {
                                    var path = AnimationUtility.CalculateTransformPath(renderer.transform, mTarget.transform);
                                    AddRenderShapeMapping(mTarget, path, renderer.sharedMesh);
                                });
                            }

                            if (skinnedMeshRenderers.Length > 0)
                                menu.AddSeparator(string.Empty);
                            menu.AddItem(Contents.AddEmpty, false, () =>
                            {

                            });
                            menu.ShowAsContext();
                        }
                    }

                    if (change.changed)
                    {
                        EditorUtility.SetDirty(mFaceMapperData);
                        mSerializedObject.ApplyModifiedProperties();
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            //if (GUILayout.Button("Print"))
            //{
            //    Debug.Log("print content");
            //    int bsCount = mFaceMapperData.m_BlendShapeConfig.Count;
            //    for (int i = 0; i < bsCount; i++)
            //    {
            //        BlendShapeConfig bsc = mFaceMapperData.m_BlendShapeConfig[i];

            //        // int meshBlendShapeCount = meshrender.sharedMesh.blendShapeCount;
            //        BindProperty[] bps = bsc.m_Binds;
            //        int bpscount = bps.Length;
            //        for (int bpsindex = 0; bpsindex < bpscount; bpsindex++)
            //        {
            //            int targetindex = bps[bpsindex].ShapeIndex;
            //            int faceblendshapeIndex = (int)(bps[bpsindex].Location);
            //            Debug.Log(bps[bpsindex].Location + "," + faceblendshapeIndex + "," + targetindex);
            //        }
            //    }
            //}

            bool needcreate = true;
            if (mFaceMapperData != null)
                needcreate = false;

            if (mSerializedObject != null)
                mSerializedObject.ApplyModifiedProperties();

        }

        private void UpdateProperty(Transform target, Transform root, SerializedProperty property)
        {
            if (target == null || root == null)
                property.SetValue("");
            else
            {
                string path = AnimationUtility.CalculateTransformPath(target, root);
                property.SetValue(path);
            }
        }

        public static void DrawLabelWithColorInBox(string label, Color color)
        {
            Color preColor = GUI.color;
            GUI.color = color;
            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField(label);
            EditorGUILayout.EndHorizontal();
            GUI.color = preColor;
        }

        public static void DrawLabelWithColorInBox(GUIContent content, Color color)
        {
            Color preColor = GUI.color;
            GUI.color = color;
            EditorGUILayout.BeginHorizontal("Box");
            EditorGUILayout.LabelField(content);
            EditorGUILayout.EndHorizontal();
            GUI.color = preColor;
        }

        private void DrawTransforms()
        {
            DrawProperty(m_LeftEyeBonePath, Contents.LeftEyeBoneHeader);
            EditorGUI.BeginChangeCheck();
            Transform tmpLeft = (Transform)EditorGUILayout.ObjectField(
                Contents.LeftEyeTransformHeader,
                mTransformLeftEyeBone,
                typeof(Transform),
                true);
            if (EditorGUI.EndChangeCheck())
            {
                if (tmpLeft == null)
                {
                    mTransformLeftEyeBone = tmpLeft;
                    UpdateProperty(null, null, m_LeftEyeBonePath);
                }
                else if (tmpLeft.IsChildOf(mTarget.transform))
                {
                    mTransformLeftEyeBone = tmpLeft;
                    UpdateProperty(mTransformLeftEyeBone, mTarget.transform, m_LeftEyeBonePath);
                }
                else
                    Debug.LogError("Need Player's Root's Child Transform");
            }
            EditorGUILayout.Space(10);

            EditorGUI.BeginChangeCheck();
            DrawProperty(m_RightEyeBonePath, Contents.RightEyeBoneHeader);
            Transform tmpRight = (Transform)EditorGUILayout.ObjectField(
                Contents.RightEyeTransformHeader,
                mTransformRightEyeBone,
                typeof(Transform),
                true);
            if (EditorGUI.EndChangeCheck())
            {
                if (tmpRight == null)
                {
                    mTransformRightEyeBone = tmpRight;
                    UpdateProperty(null, null, m_RightEyeBonePath);
                }
                else if (tmpRight.IsChildOf(mTarget.transform))
                {
                    mTransformRightEyeBone = tmpRight;
                    UpdateProperty(mTransformRightEyeBone, mTarget.transform, m_RightEyeBonePath);
                }
                else
                    Debug.LogError("Need Player's Root's Child Transform");
            }
            EditorGUILayout.Space(10);

            DrawProperty(m_HeadBonePath, Contents.HeadBoneHeader);
            EditorGUI.BeginChangeCheck();
            Transform tmpHead = (Transform)EditorGUILayout.ObjectField(
                Contents.HeadTransformHeader,
                mTransformHeadBone,
                typeof(Transform),
                true);
            if (EditorGUI.EndChangeCheck())
            {
                if (tmpHead.IsChildOf(mTarget.transform))
                {
                    mTransformHeadBone = tmpHead;
                    UpdateProperty(mTransformHeadBone, mTarget.transform, m_HeadBonePath);
                }
                else
                    Debug.LogError("Need Player's Root's Child Transform");
            }
        }
    }
}

