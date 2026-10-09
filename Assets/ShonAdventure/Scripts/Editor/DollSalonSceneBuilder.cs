#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace ShonAdventure.Editor
{
    public static class DollSalonSceneBuilder
    {
        [MenuItem("ShonAdventure/Create Doll Salon Puzzle Scene")]
        public static void CreateScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("ShonAdventure", "Stop Play mode first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string folder="Assets/ShonAdventure/Scenes";
            const string path=folder+"/HauntedHouse_DollSalon.unity";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            if (File.Exists(path) && !EditorUtility.DisplayDialog("Replace Doll Salon?", "Replace the existing Doll Salon scene? Other scenes stay untouched.", "Replace", "Cancel")) return;
            var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
            var manager=new GameObject("Chapter 2 - Doll Salon Puzzle Controller");
            manager.AddComponent<HauntedDollSalon>();
            EditorSceneManager.MarkSceneDirty(scene);
            if (EditorSceneManager.SaveScene(scene,path))
            {
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("ShonAdventure", "Doll Salon created. Press Play to test the puzzle.", "OK");
            }
        }
    }
}
#endif
