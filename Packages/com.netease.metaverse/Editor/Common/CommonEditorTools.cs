using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;

namespace Hammer.Editor.Common
{
    public static class CommonEditorTools
    {
        public const string menuRoot = "Hammer Tools";

        [MenuItem(menuRoot + "/Open Cache Folder %F11")]
        private static void OpenCacheFolder()
        {
            string workpath = Application.persistentDataPath + Path.DirectorySeparatorChar + "Player.log";
            if (!File.Exists(workpath))
                workpath = Application.persistentDataPath;
            EditorUtility.RevealInFinder(workpath);
            Debug.Log("OpenCacheFolder:" + workpath);
        }
    }
}

