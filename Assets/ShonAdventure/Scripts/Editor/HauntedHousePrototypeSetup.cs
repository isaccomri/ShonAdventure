#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

namespace ShonAdventure.Editor
{
    /// <summary>Create a separate playable prototype scene without touching Shon_TestRoom.</summary>
    public static class HauntedHousePrototypeSetup
    {
        [MenuItem("ShonAdventure/Create Playable Prototype Scene")]
        public static void Create()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("ShonAdventure", "Stop Play mode before creating a scene.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string folder = "Assets/ShonAdventure/Scenes";
            const string path = folder + "/HauntedHouse_Prototype.unity";
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            if (File.Exists(path) && !EditorUtility.DisplayDialog(
                "Scene already exists", "Replace the separate prototype scene? Your Shon_TestRoom will not be changed.", "Replace", "Cancel")) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("Haunted House Prototype Controller");
            root.AddComponent<HauntedHousePrototype>();
            var cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            cameraObj.AddComponent<Camera>();
            var lightObj = new GameObject("Moonlight");
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.3f;
            lightObj.transform.rotation = Quaternion.Euler(30f, -30f, 0f);
            EditorSceneManager.MarkSceneDirty(scene);
            if (EditorSceneManager.SaveScene(scene, path))
            {
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("ShonAdventure", "Created " + path + ". Press Play to test the 20-stage prototype.", "OK");
            }
        }
    }
}
#endif
