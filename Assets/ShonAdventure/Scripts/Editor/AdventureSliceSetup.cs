#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace ShonAdventure.Editor
{
    public static class AdventureSliceSetup
    {
        [MenuItem("ShonAdventure/Create Chapter One Puzzle Scene")]
        public static void MakeScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("ShonAdventure", "Exit Play mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string directory = "Assets/ShonAdventure/Scenes";
            const string path = directory + "/HauntedHouse_ChapterOne.unity";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            if (File.Exists(path) && !EditorUtility.DisplayDialog("ShonAdventure", "Replace the existing chapter-one puzzle scene? The original test scene is untouched.", "Replace", "Cancel")) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var controller = new GameObject("Chapter 1 - Interactive Puzzle Controller");
            controller.AddComponent<HauntedAdventureSlice>();
            EditorSceneManager.MarkSceneDirty(scene);
            if (EditorSceneManager.SaveScene(scene, path))
            {
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("ShonAdventure", "Created Chapter One. Press Play to test inventory puzzles and two ways through the gate.", "OK");
            }
        }
    }
}
#endif
