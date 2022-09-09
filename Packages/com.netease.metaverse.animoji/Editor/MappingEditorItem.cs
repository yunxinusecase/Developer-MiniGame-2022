using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.ComponentModel;
using System.Linq;

namespace MetaVerse.Animoji.Tools.Editor
{
    public class MappingEditorItem
    {
        protected static class Contents
        {
            public static readonly GUIContent FaceShape = new GUIContent("Blend Shape", "The ARKit blend shape to influence this mesh blend shape with.");
            public static readonly GUIContent MeshShape = new GUIContent("Blend Shape", "The blend shape in the target mesh to influence.");

            public static readonly float LocationWidth = 160f;
            public static readonly float ButtonWidth = 18f;
            public static readonly float BindingSpacing = 6f * EditorGUIUtility.standardVerticalSpacing;
            public static readonly float BindingSeparatorHeight = 2f;
            public static readonly Vector2 OptionDropdownSize = new Vector2(300f, 250f);

            public static readonly Color ConfigBackground1 = new Color(0f, 0f, 0f, 0.04f);
            public static readonly Color ConfigBackground2 = new Color(1f, 1f, 1f, 0.04f);
            public static readonly Color SeparatorColor = new Color(0f, 0f, 0f, 0.1f);
            public static readonly GUIContent IconToolbarPlus = EditorGUIUtility.TrIconContent("Toolbar Plus", "Add mapping.");
            public static readonly GUIContent IconToolbarMinus = EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove mapping.");
        }

        class ShapeIndexBinding
        {
            public int m_ShapeIndex;
            public BindConfig m_Config;
        }

        private FaceBlendShapeDefine.FaceBlendShape m_Location;
        private readonly List<ShapeIndexBinding> m_Bindings;
        private readonly List<int> m_UnUsedShapeIndices = new List<int>();

        // private int m_ShapeIndex;
        private bool m_IsExpanded;

        public FaceBlendShapeDefine.FaceBlendShape Location => m_Location;

        // public int ShapeIndex => m_ShapeIndex;
        public bool IsExpanded => m_IsExpanded;

        private MappingEditorList m_MappingList;

        public int BindingCount => m_Bindings.Count;

        // Get All Bind
        public BindProperty[] GetBindings()
        {
            return m_Bindings
                .Select(b => new BindProperty(m_Location, b.m_ShapeIndex, b.m_Config, m_IsExpanded))
                .ToArray();
        }

        public MappingEditorItem(
            MappingEditorList mappingList,
            FaceBlendShapeDefine.FaceBlendShape location,
            IEnumerable<(int shapeIndex, BindConfig config, bool isExpanded)> bindings,
            bool isExpand)
        {
            m_MappingList = mappingList;
            m_Location = location;
            m_Bindings = bindings.Select(binding => new ShapeIndexBinding
            {
                m_ShapeIndex = binding.shapeIndex,
                m_Config = binding.config,
            }).ToList();
            m_IsExpanded = bindings.Any(binding => binding.isExpanded);
            RefreshUnUsedShapeIndices();
        }

        public MappingEditorItem(MappingEditorList mappingList, FaceBlendShapeDefine.FaceBlendShape location)
        {
            m_MappingList = mappingList;
            m_Location = location;
            m_IsExpanded = true;
            m_Bindings = new List<ShapeIndexBinding>();
            ShapeIndexBinding defaultBinding = new ShapeIndexBinding();
            defaultBinding.m_Config = new BindConfig();
            defaultBinding.m_ShapeIndex = 0;
            m_Bindings.Add(defaultBinding);
            RefreshUnUsedShapeIndices();
        }

        private void RefreshUnUsedShapeIndices()
        {
            m_UnUsedShapeIndices.Clear();
            Debug.Assert(m_MappingList != null);
            for (var i = 0; i < m_MappingList.MeshBlendShapeCount; i++)
            {
                if (!m_Bindings.Any(b => b.m_ShapeIndex == i))
                    m_UnUsedShapeIndices.Add(i);
            }
        }

        public float GetHeight()
        {
            var height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (m_IsExpanded)
            {
                for (var i = 0; i < BindingCount; i++)
                {
                    height += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    height += GetConfig(i).GetHeight() + Contents.BindingSpacing;
                }
            }
            return height;
        }

        BindConfig GetConfig(int index)
        {
            return m_Bindings[index].m_Config;
        }

        public void OnGUI(Rect rect)
        {
            var pos = new Rect(rect)
            {
                height = EditorGUIUtility.singleLineHeight,
            };

            var locationRect = new Rect(pos)
            {
                width = Contents.LocationWidth,
            };
            var foldoutRect = new Rect(pos)
            {
                xMin = locationRect.xMax,
                xMax = pos.xMax - Contents.ButtonWidth,
            };
            var buttonRect = new Rect(pos)
            {
                xMin = foldoutRect.xMax,
            };

            OnMappingValueGUI(locationRect);
            OnFoldoutGUI(foldoutRect);
            OnAddBindingGUI(buttonRect);

            if (m_IsExpanded)
            {
                for (var i = 0; i < BindingCount; i++)
                {
                    var config = GetConfig(i);
                    pos.y = pos.yMax;
                    var separatorRect = new Rect
                    {
                        xMin = pos.xMin,
                        xMax = pos.xMax,
                        y = pos.y - Contents.BindingSeparatorHeight / 2f,
                        height = Contents.BindingSeparatorHeight,
                    };
                    var bindingRect = new Rect
                    {
                        xMin = pos.xMin,
                        xMax = pos.xMax - 20f,
                        y = pos.y + (Contents.BindingSpacing / 2f),
                        height = EditorGUIUtility.singleLineHeight,
                    };
                    var configRect = new Rect
                    {
                        xMin = pos.xMin,
                        xMax = pos.xMax - 20f,
                        y = bindingRect.yMax + EditorGUIUtility.standardVerticalSpacing,
                        height = config.GetHeight(),
                    };
                    buttonRect.y = bindingRect.y;
                    pos.yMax = configRect.yMax + (Contents.BindingSpacing / 2f);
                    // draw the shape config background with alternating colors
                    if (Event.current.type == EventType.Repaint)
                    {
                        EditorGUI.DrawRect(pos, i % 2 == 0 ? Contents.ConfigBackground1 : Contents.ConfigBackground2);
                        EditorGUI.DrawRect(separatorRect, Contents.SeparatorColor);
                    }

                    OnBindingGUI(bindingRect, i);
                    config.OnGUI(configRect);
                    if (GUI.Button(buttonRect, Contents.IconToolbarMinus, GUIStyle.none))
                    {
                        RemoveBinding(i);
                    }
                }
            }
        }

        void RemoveBinding(int index)
        {
            m_Bindings.RemoveAt(index);
            RefreshUnUsedShapeIndices();
        }

        void OnMappingValueGUI(Rect locationRect)
        {
            DrawLocation(locationRect, false, m_Location, m_MappingList.UnusedLocations, (value) =>
            {
                m_Location = value;
                m_MappingList.RefreshUnUsedMappings();
            });
        }

        void OnFoldoutGUI(Rect foldoutRect)
        {
            string text;
            switch (BindingCount)
            {
                case 0:
                    text = string.Empty;
                    break;
                case 1:
                    text = $"{BindingCount} binding";
                    break;
                default:
                    text = $"{BindingCount} bindings";
                    break;
            }
            var foldoutLabel = new GUIContent(text);
            if (BindingCount == 0)
            {
                EditorGUI.LabelField(foldoutRect, foldoutLabel);
            }
            else
            {
                // foldouts don't clip the label, so we do it manually
                using (new GUI.GroupScope(foldoutRect))
                {
                    m_IsExpanded = EditorGUI.Foldout(new Rect(0, 0, foldoutRect.width, foldoutRect.height), m_IsExpanded, foldoutLabel, true);
                }
            }
        }

        void OnAddBindingGUI(Rect buttonRect)
        {
            using (new EditorGUI.DisabledScope(m_UnUsedShapeIndices.Count == 0))
            {
                if (GUI.Button(buttonRect, Contents.IconToolbarPlus, GUIStyle.none))
                {
                    var unusedShape = m_UnUsedShapeIndices[0];
                    m_Bindings.Add(new ShapeIndexBinding
                    {
                        m_ShapeIndex = unusedShape,
                        m_Config = new BindConfig()
                    });
                    RefreshUnUsedShapeIndices();
                }
            }
        }

        void OnBindingGUI(Rect rect, int index)
        {
            var binding = m_Bindings[index];
            DrawShapeIndex(rect, true, binding.m_ShapeIndex, m_UnUsedShapeIndices, (value) =>
            {
                binding.m_ShapeIndex = value;
                RefreshUnUsedShapeIndices();
            });
        }

        void DrawShapeIndex(Rect rect, bool drawLabel, int shapeIndex, List<int> unusedShapeIndices,
            Action<int> onSelect)
        {
            var content = m_MappingList.GetShapeName(shapeIndex);
            if (content == null)
            {
                using (var change = new EditorGUI.ChangeCheckScope())
                {
                    shapeIndex = EditorGUI.IntField(rect, drawLabel ? Contents.MeshShape : GUIContent.none, shapeIndex);

                    if (change.changed)
                        onSelect?.Invoke(shapeIndex);
                }
            }
            else
            {
                if (drawLabel)
                {
                    rect = EditorGUI.PrefixLabel(rect, Contents.MeshShape);
                }

                if (GUI.Button(rect, new GUIContent(content.text, Contents.MeshShape.tooltip), EditorStyles.popup))
                {
                    var options = unusedShapeIndices
                        .Select(i => m_MappingList.GetShapeName(i))
                        .ToArray();

                    OptionSelectWindow.SelectOption(rect, Contents.OptionDropdownSize, options, (index, value) =>
                    {
                        onSelect.Invoke(m_MappingList.GetShapeIndex(value));
                        m_MappingList.ApplyToProperties();
                    });
                }
            }
        }

        void DrawLocation(Rect rect, bool drawLabel, FaceBlendShapeDefine.FaceBlendShape location, List<FaceBlendShapeDefine.FaceBlendShape> unusedLocations, Action<FaceBlendShapeDefine.FaceBlendShape> onSelect)
        {
            if (drawLabel)
            {
                rect = EditorGUI.PrefixLabel(rect, Contents.FaceShape);
            }
            var current = location.ToString();
            var content = new GUIContent(current, Contents.FaceShape.tooltip);
            if (GUI.Button(rect, content, EditorStyles.popup))
            {
                var options = unusedLocations
                    .Select(l => new GUIContent(l.ToString()))
                    .ToArray();

                OptionSelectWindow.SelectOption(rect, Contents.OptionDropdownSize, options, (index, value) =>
                {
                    onSelect.Invoke(BlendShapeUtil.GetLocation(value));
                    m_MappingList.ApplyToProperties();
                });
            }
        }

        public void Collapse()
        {
            if (m_IsExpanded)
            {
                m_IsExpanded = false;
                GUI.changed = true;
            }
        }
    }
}
