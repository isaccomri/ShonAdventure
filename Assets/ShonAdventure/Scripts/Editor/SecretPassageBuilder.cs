#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ShonAdventure.Editor
{
    public static class SecretPassageBuilder
    {
        const string Folder="Assets/ShonAdventure/Scenes";
        const string Scene="HauntedHouse_SecretPassage3D";
        [MenuItem("ShonAdventure/Create 3D Secret Passage Scene")]
        public static void Build()
        {
            if(EditorApplication.isPlaying){EditorUtility.DisplayDialog("ShonAdventure","Stop Play mode first.","OK");return;}
            if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            if(!Directory.Exists(Folder))Directory.CreateDirectory(Folder);
            var path=Folder+"/"+Scene+".unity";
            if(File.Exists(path)&&!EditorUtility.DisplayDialog("ShonAdventure","Replace existing Secret Passage scene?","Replace","Cancel"))return;
            var scene=EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
            new GameObject("Secret Passage Chapter Controller").AddComponent<HauntedSecretPassage3D>();
            EditorSceneManager.SaveScene(scene,path);
            var settings=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach(var p in new[]{Folder+"/HauntedHouse_DollSalon3D.unity",Folder+"/HauntedHouse_Library3D.unity",path}) {
                if(File.Exists(p)&&!settings.Exists(e=>e.path==p))settings.Add(new EditorBuildSettingsScene(p,true));
            }
            EditorBuildSettings.scenes=settings.ToArray();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("ShonAdventure","Secret Passage created. Complete the Library books puzzle to enter it.","OK");
        }
    }
}
#endif
