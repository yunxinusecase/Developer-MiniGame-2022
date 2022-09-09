using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;
#if UNITY_EDITOR_OSX
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
#endif
using System.IO;

public class Builder
{
    private const string filename = "RtcClient";

    public static List<EditorBuildSettingsScene> GetScenes()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/MainScene.unity", true));
        return scenes;
    }

    public static void ExportiOSProject()
    {

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

        EditorUserBuildSettings.symlinkLibraries = true;
        EditorUserBuildSettings.development = true;
        EditorUserBuildSettings.allowDebugging = true;

        List<EditorBuildSettingsScene> scenesList = GetScenes();
        List<string> scenes = new List<string>();

        for (int i = 0; i < scenesList.Count; i++)
        {
            scenes.Add(scenesList[i].path);
        }

        // for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        // {
        //     if (EditorBuildSettings.scenes[i].enabled)
        //     {
        //         scenes.Add(EditorBuildSettings.scenes[i].path);
        //     }
        // }

        BuildPipeline.BuildPlayer(scenes.ToArray(), "iOSProject", BuildTarget.iOS, BuildOptions.None);
    }
    public static void ExportWindows()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

        EditorUserBuildSettings.symlinkLibraries = true;
        EditorUserBuildSettings.development = true;
        EditorUserBuildSettings.allowDebugging = true;

        List<EditorBuildSettingsScene> scenesList = GetScenes();
        List<string> scenes = new List<string>();

        for (int i = 0; i < scenesList.Count; i++)
        {
            scenes.Add(scenesList[i].path);
        }

        BuildPipeline.BuildPlayer(scenes.ToArray(), string.Format("Win64Project/{0}.exe", filename), BuildTarget.StandaloneWindows64, BuildOptions.None);
    }

    public static void ExportAndroid()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

        EditorUserBuildSettings.symlinkLibraries = true;
        EditorUserBuildSettings.development = true;
        EditorUserBuildSettings.allowDebugging = true;

        List<EditorBuildSettingsScene> scenesList = GetScenes();
        List<string> scenes = new List<string>();

        for (int i = 0; i < scenesList.Count; i++)
        {
            scenes.Add(scenesList[i].path);
        }

        BuildPipeline.BuildPlayer(scenes.ToArray(), string.Format("{0}.apk", filename), BuildTarget.Android, BuildOptions.None);
    }

    public static void ExportmacOS()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);

        EditorUserBuildSettings.symlinkLibraries = true;
        EditorUserBuildSettings.development = true;
        EditorUserBuildSettings.allowDebugging = true;

        List<EditorBuildSettingsScene> scenesList = GetScenes();
        List<string> scenes = new List<string>();

        for (int i = 0; i < scenesList.Count; i++)
        {
            scenes.Add(scenesList[i].path);
        }

        BuildPipeline.BuildPlayer(scenes.ToArray(), "macOSProject", BuildTarget.StandaloneOSX, BuildOptions.None);
    }

    [PostProcessBuild(100)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS)
        {
            return;
        }

#if UNITY_EDITOR_OSX
        string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
        var proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));

        string targetGuid = proj.GetUnityMainTargetGuid();

        string[] sdkPaths = new string[]{
            "Frameworks/com.netease.game.rtc/Runtime/Plugins/iOS/nertc-c-sdk.framework"
        };

        foreach (var sdkPath in sdkPaths)
        {
            string sdkGuid = proj.FindFileGuidByRealPath(sdkPath, PBXSourceTree.Source);
            Debug.Log($"sdkGuid:{sdkGuid},sdkPath:{sdkPath}");
            if (!string.IsNullOrEmpty(sdkGuid))
            {
                proj.AddFileToEmbedFrameworks(targetGuid, sdkGuid);
            }
        }

        //save to project
        File.WriteAllText(projPath, proj.WriteToString());

#endif
    }

}

[InitializeOnLoad]
public class PreloadKeystoreSetting
{
#if UNITY_ANDROID
    static PreloadKeystoreSetting()
    {
        PlayerSettings.Android.keystoreName = "Assets/Resources/user.keystore";
        PlayerSettings.Android.keyaliasName = "key0";
        PlayerSettings.Android.keystorePass = "123456";
        PlayerSettings.Android.keyaliasPass = "123456";

    }
#endif
}

