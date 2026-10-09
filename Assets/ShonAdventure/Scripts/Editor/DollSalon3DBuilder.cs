#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace ShonAdventure.Editor
{
    // Reuse existing scenes; never regenerate or overwrite room assets here.
    internal static class RoomBuildSettings
    {
        [MenuItem("ShonAdventure/Register Existing 3D Rooms in Build Settings")]
        public static void RegisterExistingRooms()
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (string name in new[] { "DollSalon3D", "Library3D", "SecretPassage3D" })
            {
                string path = "Assets/ShonAdventure/Scenes/HauntedHouse_" + name + ".unity";
                if (!File.Exists(path)) continue;
                var existing = scenes.Find(entry => entry.path == path);
                if (existing == null) scenes.Add(new EditorBuildSettingsScene(path, true));
                else existing.enabled = true;
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }

    public static class DollSalon3DBuilder
    {
        [MenuItem("ShonAdventure/Create 3D Doll Salon Scene")]
        public static void Create()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("ShonAdventure","Stop Play mode first.","OK"); return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string folder="Assets/ShonAdventure/Scenes";
            const string path=folder+"/HauntedHouse_DollSalon3D.unity";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            if (File.Exists(path) && !EditorUtility.DisplayDialog("Replace the existing 3D Doll Salon?","Previous chapters will not be touched.","Replace","Cancel")) return;
            var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
            var controller=new GameObject("3D Salon - Point and Click Controller");
            controller.AddComponent<DollSalon3D>();
            EditorSceneManager.MarkSceneDirty(scene);
            if(EditorSceneManager.SaveScene(scene,path))
            {
                RoomBuildSettings.RegisterExistingRooms();
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("ShonAdventure","3D Doll Salon created. Press Play. Click objects in the room to explore.","OK");
            }
        }
    }
}
#endif
