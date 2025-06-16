using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneSelect : EditorWindow
{
    /// <summary>
    /// Tracks scroll position.
    /// </summary>
    private Vector2 scrollPos;
    private static SceneSelect window;

    [MenuItem("Tools/Scene Select Window")]
    internal static void Init()
    {
        window = (SceneSelect)GetWindow(typeof(SceneSelect), false, "Scene Select");
        window.position = new Rect(window.position.xMin + 100f, window.position.yMin + 100f, 200f, 150f);
    }
    
    internal void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        this.scrollPos = EditorGUILayout.BeginScrollView(this.scrollPos, false, false);
        GUILayout.Label("Scenes In Build", EditorStyles.boldLabel);
        for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            var scene = EditorBuildSettings.scenes[i];
            if (scene.enabled)
            {
                var sceneName = Path.GetFileNameWithoutExtension(scene.path);
                var pressed = GUILayout.Button(i + ": " + sceneName, new GUIStyle(GUI.skin.GetStyle("Button")) { alignment = TextAnchor.MiddleLeft });
                if (pressed)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(scene.path);
                    }
                }
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
}
