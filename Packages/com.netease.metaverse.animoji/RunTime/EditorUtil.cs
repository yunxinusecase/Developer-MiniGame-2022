#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MetaVerse.Animoji.Tools.Editor
{
    public static class EditorUtil
    {
        public static void NextLine(ref Rect rect)
        {
            rect.y = rect.yMax + EditorGUIUtility.standardVerticalSpacing;
            rect.height = EditorGUIUtility.singleLineHeight;
        }
    }
}
#endif