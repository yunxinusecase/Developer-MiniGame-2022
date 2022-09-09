using System.Collections;
using System.Collections.Generic;
using PlasticGui.WorkspaceWindow.Items;
using Siccity.GLTFUtility;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class EditorTools
{
    static EditorTools()
    {
        Debug.Log("EditorTools");
        EditorApplication.update += RunOnce;
    }

    static void RunOnce()
    {
        Debug.Log("RunOnce!");
        // reimport glb content
        string[] assets = AssetDatabase.GetAllAssetPaths();
        foreach (var item in assets)
        {
            if (item.EndsWith(".glb"))
            {
                Debug.Log("glb:" + item);
                AssetDatabase.ImportAsset(item);
            }
        }
        EditorApplication.update -= RunOnce;
    }
}