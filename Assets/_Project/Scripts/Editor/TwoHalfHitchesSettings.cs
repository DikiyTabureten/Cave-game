using UnityEditor;
using UnityEngine;

namespace CaveGame.EditorTools
{
    /// <summary>
    /// Настройки генерации Штыка. Меняются в инспекторе; кнопка «Перегенерировать Штык» (или меню
    /// Tools/Cave Game/Create Two Half Hitches) пересобирает Data/Knots/TwoHalfHitches.asset.
    /// </summary>
    public class TwoHalfHitchesSettings : ScriptableObject
    {
        public const string AssetPath = "Assets/_Project/Data/Knots/TwoHalfHitchesSettings.asset";

        [Tooltip("Длина свободного хвоста после затягивания (прямой участок вдоль коренной части), в юнитах")]
        [Min(0.5f)]
        public float tailLength = TwoHalfHitchesRecipe.DefaultTailLength;

        public static TwoHalfHitchesSettings LoadOrDefault()
        {
            var settings = AssetDatabase.LoadAssetAtPath<TwoHalfHitchesSettings>(AssetPath);
            return settings != null ? settings : CreateInstance<TwoHalfHitchesSettings>();
        }

        [MenuItem("Tools/Cave Game/Two Half Hitches Settings")]
        public static void SelectOrCreate()
        {
            var settings = AssetDatabase.LoadAssetAtPath<TwoHalfHitchesSettings>(AssetPath);
            if (settings == null)
            {
                settings = CreateInstance<TwoHalfHitchesSettings>();
                AssetDatabase.CreateAsset(settings, AssetPath);
                AssetDatabase.SaveAssets();
            }
            Selection.activeObject = settings;
        }
    }

    [CustomEditor(typeof(TwoHalfHitchesSettings))]
    public class TwoHalfHitchesSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            if (GUILayout.Button("Перегенерировать Штык"))
            {
                TwoHalfHitchesRecipe.CreateOrUpdateAsset();
            }
        }
    }
}
