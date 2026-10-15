using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CaveGame.EditorTools
{
    /// <summary>
    /// Собирает сцену прототипа: камера и объект с KnotPrototypeController. Остальное создаётся кодом на старте.
    /// </summary>
    public static class KnotPrototypeSceneBuilder
    {
        public const string ScenePath = "Assets/_Project/Scenes/KnotPrototype.unity";
        public const string MaterialPath = "Assets/_Project/Materials/LineUnlit.mat";

        private const string UnlitSpriteShader = "Universal Render Pipeline/2D/Sprite-Unlit-Default";

        [MenuItem("Tools/Cave Game/Create Knot Prototype Scene")]
        public static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Ассеты грузим после NewScene: смена сцены выгружает неиспользуемые ассеты, и ссылки обнулились бы.
            var knot = AssetDatabase.LoadAssetAtPath<KnotDefinition>(FigureEightRecipe.AssetPath);
            if (knot == null) knot = FigureEightRecipe.CreateOrUpdateAsset();
            var material = CreateOrLoadMaterial();

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

            var prototypeGo = new GameObject("KnotPrototype");
            var controller = prototypeGo.AddComponent<KnotPrototypeController>();
            var serialized = new SerializedObject(controller);
            serialized.FindProperty("knot").objectReferenceValue = knot;
            serialized.FindProperty("lineMaterial").objectReferenceValue = material;
            serialized.FindProperty("targetCamera").objectReferenceValue = camera;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"Сцена собрана: {ScenePath}");
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
