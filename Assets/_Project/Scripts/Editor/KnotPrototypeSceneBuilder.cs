using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CaveGame.EditorTools
{
    /// <summary>
    /// Собирает сцены прототипа кодом: главное меню и сцену вязки. В каждой — камера и один
    /// компонент-контроллер, остальное создаётся на старте. Ещё настраивает список сцен в Build Settings.
    /// </summary>
    public static class KnotPrototypeSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/KnotPrototype.unity";
        public const string MenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        public const string MaterialPath = "Assets/_Project/Materials/LineUnlit.mat";

        private const string UnlitSpriteShader = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

        [MenuItem("Tools/Cave Game/Build All Scenes")]
        public static void BuildAll()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            BuildMenuScene();
            BuildKnotScene();
            ConfigureBuildSettings();
            EditorSceneManager.OpenScene(MenuScenePath);
        }

        [MenuItem("Tools/Cave Game/Create Knot Prototype Scene")]
        public static void BuildKnotSceneMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildKnotScene();
        }

        [MenuItem("Tools/Cave Game/Create Main Menu Scene")]
        public static void BuildMenuSceneMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            BuildMenuScene();
        }

        /// <summary>Build Settings: главное меню стартовое, сцена вязки вторая.</summary>
        [MenuItem("Tools/Cave Game/Configure Build Scenes")]
        public static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MenuScenePath, true),
                new EditorBuildSettingsScene(ScenePath, true)
            };
            Debug.Log("Build Settings: MainMenu (0), KnotPrototype (1).");
        }

        private static void BuildKnotScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Ассеты грузим после NewScene: смена сцены выгружает неиспользуемые ассеты, и ссылки обнулились бы.
            var catalog = KnotCatalogBuilder.CreateOrUpdate();
            var material = CreateOrLoadMaterial();
            var camera = CreateCamera();

            var controller = new GameObject("KnotPrototype").AddComponent<KnotPrototypeController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("catalog").objectReferenceValue = catalog;
            serialized.FindProperty("lineMaterial").objectReferenceValue = material;
            serialized.FindProperty("targetCamera").objectReferenceValue = camera;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"Сцена собрана: {ScenePath}");
        }

        private static void BuildMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var catalog = KnotCatalogBuilder.CreateOrUpdate();
            var camera = CreateCamera();

            var controller = new GameObject("MainMenu").AddComponent<MainMenuController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("catalog").objectReferenceValue = catalog;
            serialized.FindProperty("targetCamera").objectReferenceValue = camera;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, MenuScenePath);
            Debug.Log($"Сцена собрана: {MenuScenePath}");
        }

        private static Camera CreateCamera()
        {
            var cameraGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new KnotVisualSettings().backgroundColor;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 100f;
            cameraGo.transform.position = new Vector3(0f, 0f, -10f);
            cameraGo.AddComponent<UniversalAdditionalCameraData>();
            return camera;
        }

        private static Material CreateOrLoadMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null) return material;

            var shader = Shader.Find(UnlitSpriteShader);
            if (shader == null)
            {
                Debug.LogWarning($"Шейдер {UnlitSpriteShader} не найден, беру Sprites/Default.");
                shader = Shader.Find("Sprites/Default");
            }

            material = new Material(shader) { name = "LineUnlit" };
            AssetDatabase.CreateAsset(material, MaterialPath);
            AssetDatabase.SaveAssets();
            return material;
        }
    }
}
