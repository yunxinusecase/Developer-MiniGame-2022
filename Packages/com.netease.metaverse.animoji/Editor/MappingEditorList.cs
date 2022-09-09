using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using UnityEngine.Profiling;

namespace MetaVerse.Animoji.Tools.Editor
{
    // Mapping List Editor
    public class MappingEditorList
    {
        readonly SerializedObject m_SerializedObject;
        readonly SerializedProperty m_Bindings;
        readonly SerializedProperty m_DefaultEvaluator;
        readonly GUIContent[] m_MeshBlendShapeNames;
        readonly SerializedProperty m_IsExpanded;

        private ReorderableList m_ReorderableList = null;
        private List<MappingEditorItem> m_LocationMapping = null;

        readonly Dictionary<int, float> m_ElementHeightCache = new Dictionary<int, float>();

        public int MeshBlendShapeCount => m_MeshBlendShapeNames.Length;

        public List<FaceBlendShapeDefine.FaceBlendShape> UnusedLocations { get; } = new List<FaceBlendShapeDefine.FaceBlendShape>();

        public bool IsExpanded
        {
            get => m_IsExpanded.boolValue;
            set => m_IsExpanded.boolValue = value;
        }

        public MappingEditorList(
            SerializedProperty rendererMapping,
            SerializedProperty defaultEvaluator,
            GUIContent[] meshBlendShapeNames)
        {
            m_SerializedObject = rendererMapping.serializedObject;
            m_Bindings = rendererMapping.FindPropertyRelative("m_Binds");
            m_IsExpanded = rendererMapping.FindPropertyRelative("m_IsExpanded");

            m_MeshBlendShapeNames = meshBlendShapeNames;

            UpdateFromProperties();
            ApplyToProperties();
            UpdateFromProperties();
        }

        public GUIContent GetShapeName(int shapeIndex)
        {
            if (m_MeshBlendShapeNames == null || shapeIndex < 0 || shapeIndex >= m_MeshBlendShapeNames.Length)
                return null;
            return m_MeshBlendShapeNames[shapeIndex];
        }

        public int GetShapeIndex(string shapeName)
        {
            for (var i = 0; i < m_MeshBlendShapeNames.Length; i++)
            {
                if (m_MeshBlendShapeNames[i].text == shapeName)
                {
                    return i;
                }
            }
            return -1;
        }

        private void UpdateFromProperties()
        {
            var bindings = m_Bindings.GetValue<BindProperty[]>();
            var count = bindings.Length;
            if (m_LocationMapping == null)
                m_LocationMapping = new List<MappingEditorItem>();
            m_LocationMapping.Clear();
            m_LocationMapping = bindings
                .ToLookup(b => b.Location, b => (shapeIndex: b.ShapeIndex, config: b.m_Config, isExpanded: b.IsExpanded))
                .Select(mapping => new MappingEditorItem(this, mapping.Key, mapping, IsExpanded))
                .ToList();

            CreateReorderableList(m_LocationMapping);
            RefreshUnUsedMappings();
        }

        public void ApplyToProperties()
        {
            var bindings = m_LocationMapping
                .SelectMany(m => m.GetBindings())
                .ToArray();

            m_Bindings.SetValue(bindings);
            m_SerializedObject.ApplyModifiedProperties();
        }

        public void RefreshUnUsedMappings()
        {
            UnusedLocations.Clear();
            foreach (var location in FaceBlendShapeDefine.Shapes)
            {
                if (!m_LocationMapping.Any(m => m.Location == location))
                {
                    UnusedLocations.Add(location);
                }
            }
        }

        void CreateReorderableList<T>(List<T> list) where T : MappingEditorItem
        {
            m_ReorderableList = new ReorderableList(list, typeof(T), true, false, true, true)
            {
                headerHeight = 0f,
                elementHeightCallback = (index) =>
                {
                    if (!m_ElementHeightCache.TryGetValue(index, out var height))
                    {
                        height = list[index].GetHeight();
                        m_ElementHeightCache.Add(index, height);
                    }
                    return height;
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    list[index].OnGUI(rect);
                    // block dragging/selection of the list item in the item rect
                    if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
                    {
                        Event.current.Use();
                    }
                },
                onMouseDragCallback = (_) =>
                {
                    // Collapse all elements so that they all have same height wile dragging,
                    // since the list doesn't behave well with heterogeneous element sizes.
                    foreach (var item in list)
                        item.Collapse();
                },
                onReorderCallback = (_) =>
                {
                    GUI.changed = true;
                },
                onCanAddCallback = (_) => CanAddMapping(),
                onAddCallback = (_) => AddMapping(),
                onRemoveCallback = (l) => RemoveMapping(l.index),
            };
        }

        bool CanAddMapping()
        {
            return UnusedLocations.Count > 0;
        }

        void AddMapping()
        {
            m_LocationMapping.Add(new MappingEditorItem(this, UnusedLocations[0]));
            RefreshUnUsedMappings();
            Debug.Log("AddMapping()");
        }

        void RemoveMapping(int index)
        {
            m_LocationMapping.RemoveAt(index);
            RefreshUnUsedMappings();
        }

        public void OnGUI()
        {
            m_ElementHeightCache.Clear();
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                Profiler.BeginSample($"DoLayoutList()");
                m_ReorderableList.DoLayoutList();
                Profiler.EndSample();

                if (change.changed)
                {
                    ApplyToProperties();
                }
            }
        }
    }
}