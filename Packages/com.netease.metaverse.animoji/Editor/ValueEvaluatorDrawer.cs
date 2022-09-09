using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;
using MetaVerse.Animoji;

namespace MetaVerse.Animoji.Tools.Editor
{
    [CustomPropertyDrawer(typeof(ValueSimpleEvaluator.Impl))]
    [CustomPropertyDrawer(typeof(ValueCurveEvaluator.Impl))]
    public class ValueEvaluatorDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var values = property.GetValues<IEvaluator>();
            return values.Length > 1 ? EditorGUIUtility.singleLineHeight : values[0].GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var values = property.GetValues<IEvaluator>();

            if (values.Length > 1)
            {
                EditorGUI.HelpBox(position, "Multi-object editing not supported", MessageType.Info);
                return;
            }

            var value = values[0];
            using (var change = new EditorGUI.ChangeCheckScope())
            {
                value.OnGUI(position);
                if (change.changed)
                    property.SetValue(value);
            }
        }
    }
}

#endif
