#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

namespace ShonAdventure.Editor
{
    public static class LibrarySceneBuilder
    {
        const string Folder="Assets/ShonAdventure/Scenes";
        const string LibraryScene="HauntedHouse_Library3D";
        [MenuItem("ShonAdventure/Create 3D Library Scene")]
        public static void Create()
        {
            if(EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("ShonAdventure","Stop Play first.","OK");return;
            }
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            if(!Directory.Exists(Folder))Directory.CreateDirectory(Folder);
            string path=Folder+"/"+LibraryScene+".unity";
            if(File.Exists(path) && !EditorUtility.DisplayDialog("ShonAdventure","Replace existing library scene?","Replace","Cancel"))return;
            var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
            new GameObject("Library Chapter Controller").AddComponent<HauntedLibrary3D>();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene,path)) return;
            RoomBuildSettings.RegisterExistingRooms();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("ShonAdventure","3D Library scene created and added to Build Settings. Open the Doll Salon and complete its puzzle to enter the library.","OK");
        }
    }
}
#endif
