using System.Collections.Generic;
using System.IO;
using SerpentRush.Gameplay;
using SerpentRush.Managers;
using SerpentRush.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SerpentRush.EditorTools
{
    public static class SerpentRushSetupMenu
    {
        const string ConfigPath = "Assets/_Game/Data/GameConfig.asset";
        const string ScenePath = "Assets/_Game/Scenes/Bootstrap.unity";

        [MenuItem("Serpent Rush/Setup/Create Default Game Config")]
        public static void CreateDefaultGameConfig()
        {
            EnsureProjectFolders();
            var config = LoadOrCreateConfig();
            if (config == null)
                return;

            EditorGUIUtility.PingObject(config);
            Selection.activeObject = config;
        }

        [MenuItem("Serpent Rush/Setup/Create Bootstrap Scene")]
        public static void CreateBootstrapScene()
        {
            EnsureProjectFolders();

            if (File.Exists(ScenePath))
            {
                var overwrite = EditorUtility.DisplayDialog(
                    "Bootstrap Scene",
                    "Assets/_Game/Scenes/Bootstrap.unity already exists. Overwrite it?",
                    "Overwrite",
                    "Cancel");

                if (!overwrite)
                    return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var config = LoadOrCreateConfig();
            if (config == null)
                return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            var game = new GameObject("Game");
            game.AddComponent<GameManager>();
            var bootstrapper = game.AddComponent<GameBootstrapper>();

            var serialized = new SerializedObject(bootstrapper);
            var configProperty = serialized.FindProperty("_config");
            if (configProperty == null)
            {
                Debug.LogError("[SerpentRush] GameBootstrapper is missing the _config field.");
                return;
            }

            configProperty.objectReferenceValue = config;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureBootstrapIsFirstBuildScene();
            AssetDatabase.SaveAssets();

            Selection.activeGameObject = game;
            Debug.Log("[SerpentRush] Bootstrap scene created at " + ScenePath + " and set as the first scene in Build Settings.");
        }

        static GameConfig LoadOrCreateConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameConfig>(ConfigPath);
            if (existing != null)
                return existing;

            if (File.Exists(ConfigPath))
            {
                Debug.LogError("[SerpentRush] GameConfig exists but failed to load: " + ConfigPath);
                return null;
            }

            var config = ScriptableObject.CreateInstance<GameConfig>();
            AssetDatabase.CreateAsset(config, ConfigPath);
            AssetDatabase.SaveAssets();
            return config;
        }

        static void EnsureProjectFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game"))
                AssetDatabase.CreateFolder("Assets", "_Game");
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Data"))
                AssetDatabase.CreateFolder("Assets/_Game", "Data");
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Scenes"))
                AssetDatabase.CreateFolder("Assets/_Game", "Scenes");
        }

        [MenuItem("Serpent Rush/Setup/Place Snake Head")]
        public static void PlaceSnakeHead()
        {
            const string PrefabPath = "Assets/_Game/Prefabs/SnakeHead.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError("[SerpentRush] Missing snake head prefab at " + PrefabPath);
                return;
            }

            var existing = Object.FindAnyObjectByType<SnakeController>();
            GameObject snakeObject;
            if (existing != null)
            {
                snakeObject = existing.gameObject;
            }
            else
            {
                snakeObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(snakeObject, "Place Snake Head");
            }

            var view = Camera.main;
            if (view != null)
            {
                var follow = view.GetComponent<SnakeCameraFollow>();
                if (follow != null)
                    follow.enabled = false;
            }

            var rig = Object.FindAnyObjectByType<SnakeCinemachineRig>();
            if (rig == null)
            {
                var rigObject = new GameObject("Snake Camera Rig");
                Undo.RegisterCreatedObjectUndo(rigObject, "Add Snake Camera Rig");
                rig = Undo.AddComponent<SnakeCinemachineRig>(rigObject);
            }

            var rigSerialized = new SerializedObject(rig);
            var rigTarget = rigSerialized.FindProperty("_target");
            if (rigTarget != null)
                rigTarget.objectReferenceValue = snakeObject.transform;
            var rigCamera = rigSerialized.FindProperty("_outputCamera");
            if (rigCamera != null && view != null)
                rigCamera.objectReferenceValue = view;
            rigSerialized.ApplyModifiedPropertiesWithoutUndo();

            if (Object.FindAnyObjectByType<MobileSteerOverlay>() == null)
            {
                var controls = new GameObject("Mobile Controls");
                Undo.RegisterCreatedObjectUndo(controls, "Add Mobile Controls");
                Undo.AddComponent<MobileSteerOverlay>(controls);
            }

            if (Object.FindAnyObjectByType<RunHud>() == null)
            {
                var hud = new GameObject("Game HUD");
                Undo.RegisterCreatedObjectUndo(hud, "Add Game HUD");
                Undo.AddComponent<RunHud>(hud);
            }

            var manager = Object.FindAnyObjectByType<GameManager>();
            if (manager != null)
            {
                var link = manager.GetComponent<SnakeSessionLink>();
                if (link == null)
                    link = Undo.AddComponent<SnakeSessionLink>(manager.gameObject);

                var serialized = new SerializedObject(link);
                var snakeProperty = serialized.FindProperty("_snake");
                if (snakeProperty != null)
                {
                    snakeProperty.objectReferenceValue = snakeObject.GetComponent<SnakeController>();
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            EditorSceneManager.MarkSceneDirty(snakeObject.scene);
            Selection.activeGameObject = snakeObject;
            Debug.Log("[SerpentRush] Snake head is in the scene. Press Play, then steer with WASD, the arrow keys, or the on-screen stick.");
        }

        static void EnsureBootstrapIsFirstBuildScene()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(scene => scene.path == ScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
