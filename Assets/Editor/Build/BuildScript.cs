using System;
using UnityEditor;
using UnityEngine;

public static class BuildScript
{
    static readonly string[] scenes = { 
        "Assets/scenes/MainMenu.unity",
        "Assets/scenes/Match.unity",
    };

    public static void Windows64()
    {
        string pathToDeploy = "builds/Windows64/Ballers.exe";

        var report = BuildPipeline.BuildPlayer(scenes, pathToDeploy, BuildTarget.StandaloneWindows64, BuildOptions.None);

        Debug.Log(report.summary);
    }

    public static void Linux64()
    {
        string pathToDeploy = "builds/Linux64/Ballers.x86_64";

        var report = BuildPipeline.BuildPlayer(scenes, pathToDeploy, BuildTarget.StandaloneLinux64, BuildOptions.None);

        Debug.Log(report.summary);
    }

}
