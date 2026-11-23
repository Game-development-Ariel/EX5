using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Rebuilds the scene of the game from the setup tools, and builds the game for the browser.
/// The methods are public so they can also run from the command line (Unity -batchmode -executeMethod).
/// </summary>
public static class ProjectBuilder
{
    // Outside the project folder (next to it), so the build never gets into git.
    private const string WebGLFolder = "../ex5-webgl-build";

    // Unity's web page draws the game at a fixed size on a computer, so a smaller window (like the itch.io
    // frame) shows only the middle and hides the buttons. On a phone the page fills the window instead.
    // After the build, the computer gets the phone's behavior: the game fills the window at any size,
    // and the UI scales with it (UiFactory's CanvasScaler).
    private const string WebGLPage = WebGLFolder + "/index.html";
    private const string FillTheWindow =
        "document.querySelector(\"#unity-container\").className = \"unity-mobile\";\n" +
        "        canvas.className = \"unity-mobile\";";

    [MenuItem("Tools/Rebuild Scene")]
    public static void BuildScene()
    {
        EditorSceneManager.OpenScene(TunnelAssets.ScenePath);
        TutorialMapBuilder.Build();
        TunnelLevelSetup.Setup();
        GameFeelSetup.Setup();
        DramaSetup.Setup();
        // The game has one scene, and it is where the game starts.
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(TunnelAssets.ScenePath, true) };
        AssetDatabase.SaveAssets();
    }

    /// <summary>Builds the game for the browser into ../ex5-webgl-build, ready to zip and upload to itch.io.</summary>
    [MenuItem("Tools/Build WebGL")]
    public static void BuildWebGL()
    {
        // Gzip with the fallback loader works on itch.io, which doesn't send the compression headers.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;

        Directory.CreateDirectory(WebGLFolder);
        var options = new BuildPlayerOptions
        {
            scenes = new[] { TunnelAssets.ScenePath },
            locationPathName = WebGLFolder,
            target = BuildTarget.WebGL,
        };
        BuildResult result = BuildPipeline.BuildPlayer(options).summary.result;
        Debug.Log("WebGL build: " + result);
        if (result == BuildResult.Succeeded)
        {
            MakeThePageFillTheWindow();
        }
    }

    private static void MakeThePageFillTheWindow()
    {
        string fixedSize =
            $"canvas.style.width = \"{PlayerSettings.defaultWebScreenWidth}px\";\n" +
            $"        canvas.style.height = \"{PlayerSettings.defaultWebScreenHeight}px\";";
        string page = File.ReadAllText(WebGLPage);
        if (!page.Contains(fixedSize))
        {
            Debug.LogWarning("WebGL page: the fixed size was not found, so the game keeps Unity's size on a computer.");
            return;
        }
        File.WriteAllText(WebGLPage, page.Replace(fixedSize, FillTheWindow));
        Debug.Log("WebGL page: the game fills the window.");
    }
}
