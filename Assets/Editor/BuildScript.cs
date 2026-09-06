using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class BuildScript
{
    private static string[] GetScenes()
    {
        return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
    }

    public static void Build()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        var scenes = GetScenes();

        if (scenes.Length == 0)
        {
            Debug.LogError("No scenes in Build Settings.");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"Building {target} with {scenes.Length} scenes...");

        switch (target)
        {
            case BuildTarget.Android:
                BuildAndroid(scenes);
                break;
            case BuildTarget.WebGL:
                BuildWebGL(scenes);
                break;
            default:
                Debug.LogError($"Unsupported build target: {target}");
                EditorApplication.Exit(1);
                break;
        }
    }

    private static void BuildAndroid(string[] scenes)
    {
        PlayerSettings.Android.keystoreName = "";
        PlayerSettings.Android.keystorePass = "";
        PlayerSettings.Android.keyaliasName = "";
        PlayerSettings.Android.keyaliasPass = "";

        var keystorePath = Path.Combine(Environment.CurrentDirectory, "release.keystore");
        var keystoreExists = File.Exists(keystorePath);

        if (!keystoreExists)
        {
            Debug.Log("No keystore found. Building unsigned APK...");
            PlayerSettings.Android.buildApkPerCpuArchitecture = false;
        }
        else
        {
            Debug.Log("Using keystore: " + keystorePath);
            PlayerSettings.Android.keystoreName = keystorePath;
            PlayerSettings.Android.keystorePass = "insos123";
            PlayerSettings.Android.keyaliasName = "insos";
            PlayerSettings.Android.keyaliasPass = "insos123";
        }

        var outputPath = Path.Combine("build", "Android", "INSOS.apk");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.LogError($"Android build failed: {report.summary.totalErrors} errors");
            EditorApplication.Exit(1);
        }
        else
        {
            Debug.Log($"Android build succeeded: {outputPath}");
        }
    }

    private static void BuildWebGL(string[] scenes)
    {
        var outputPath = Path.Combine("build", "WebGL");
        Directory.CreateDirectory(outputPath);

        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.LogError($"WebGL build failed: {report.summary.totalErrors} errors");
            EditorApplication.Exit(1);
        }
        else
        {
            Debug.Log($"WebGL build succeeded: {outputPath}");
        }
    }
}
