using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Editors
{
    [Overlay(typeof(SceneView), "Scene Selection")]
    public class SceneSelectionOverlay : ToolbarOverlay
    { 
        SceneSelectionOverlay() : base(SceneDropdownToggle.k_id) { }

        [EditorToolbarElement(k_id, typeof(SceneView))]
        class SceneDropdownToggle : EditorToolbarDropdownToggle, IAccessContainerWindow
        {
            public const string k_id = "SceneSelectionOverlay/SceneDropdownToggle";
            public EditorWindow containerWindow { get; set; }
            SceneDropdownToggle() 
            { 
                text= "Scenes";
                tooltip = "Select a scene to load";

                dropdownClicked += ShowBuildSettingsSceneMenu;
            }

            private void ShowSceneMenu()
            {
                GenericMenu menu = new GenericMenu();

                var currentScene = EditorSceneManager.GetActiveScene();

                string[] sceneGuids = AssetDatabase.FindAssets("t:scene", new string[] { "Assets" });

                for (int i = 0; i < sceneGuids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);

                    string name = Path.GetFileNameWithoutExtension(path);

                    menu.AddItem(new GUIContent(name), string.Compare(currentScene.name, name) == 0, () => OpenScene(currentScene, path));
                }

                menu.ShowAsContext();
            }
            
            private void ShowBuildSettingsSceneMenu()
            {
                GenericMenu menu = new GenericMenu();
                
                var currentScene = SceneManager.GetActiveScene();
                
                List<string> scenePaths = GetBuildSettingsScenes();

                foreach (var path in scenePaths)
                {
                    string name = Path.GetFileNameWithoutExtension(path);
                    
                    menu.AddItem(new GUIContent(name), String.CompareOrdinal(currentScene.name, name) == 0, () => OpenScene(currentScene, path));
                }

                menu.ShowAsContext();
            }

            void OpenScene(UnityEngine.SceneManagement.Scene currentScene, string path)
            {
                if (currentScene.isDirty)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                        EditorSceneManager.OpenScene(path);
                }
                else
                {
                    EditorSceneManager.OpenScene(path);
                }
            }
            
            private List<string> GetBuildSettingsScenes()
            {
                List<string> scenes = new List<string>();
                
                foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                {
                    if (scene.enabled)
                    {
                        scenes.Add(scene.path);
                    }
                }
                
                return scenes;
            }
        }
    }
}
