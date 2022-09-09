//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using Unity.LiveCapture.ARKitFaceCapture.DefaultMapper.Editor;
//using Unity.LiveCapture.ARKitFaceCapture.DefaultMapper;

//public class EditorDebugWindow : EditorWindow
//{
//    Vector2 scrollPos;

//    [MenuItem("YunXin/Face Mapper Debug Window")]
//    static void Init()
//    {
//        // Get existing open window or if none, make a new one:
//        EditorDebugWindow window = (EditorDebugWindow)EditorWindow.GetWindow(typeof(EditorDebugWindow));
//        window.Show();
//    }

//    private Editor m_FaceMapperEditor = null;
//    private UnityEngine.Object m_FaceMapperObject = null;

//    Editor GetFaceMapperEditor
//    {
//        get
//        {
//            Editor.CreateCachedEditor(m_FaceMapperObject, typeof(DefaultFaceMapperEditor), ref m_FaceMapperEditor);
//            return m_FaceMapperEditor;
//        }
//    }

//    private void OnEnable()
//    {
//        Undo.undoRedoPerformed += OnUndoRedoPerformed;
//    }

//    private void OnDisable()
//    {
//        Undo.undoRedoPerformed -= OnUndoRedoPerformed;
//    }

//    void OnUndoRedoPerformed()
//    {
//        Repaint();
//    }

//    private void OnGUI()
//    {
//        m_FaceMapperObject = EditorGUILayout.ObjectField("Target", m_FaceMapperObject, typeof(DefaultFaceMapper), true);

//        if (m_FaceMapperObject != null)
//        {
//            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
//            GetFaceMapperEditor.OnInspectorGUI();
//            if (GUI.changed)
//            {
//                EditorUtility.SetDirty(m_FaceMapperObject);
//            }
//            EditorGUILayout.EndScrollView();
//        }
//    }
//}

