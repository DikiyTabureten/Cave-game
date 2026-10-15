using UnityEditor;
using UnityEngine;

namespace CaveGame.EditorTools
{
    /// <summary>
    /// Создаёт каталог узлов Data/KnotCatalog.asset. Порядок в каталоге: Восьмёрка, Штык, Булинь.
    /// Штыка пока нет — когда он появится, его ставят между Восьмёркой и Булинем.
    /// </summary>
    public static class KnotCatalogBuilder
    {
        public const string AssetPath = "Assets/_Project/Data/KnotCatalog.asset";

        [MenuItem("Tools/Cave Game/Create Knot Catalog")]
        public static KnotCatalog CreateOrUpdate()
        {
            var figureEight = AssetDatabase.LoadAssetAtPath<KnotDefinition>(FigureEightRecipe.AssetPath);
            if (figureEight == null) figureEight = FigureEightRecipe.CreateOrUpdateAsset();
            var bowline = AssetDatabase.LoadAssetAtPath<KnotDefinition>(BowlineRecipe.AssetPath);
            if (bowline == null) bowline = BowlineRecipe.CreateOrUpdateAsset();

            var catalog = AssetDatabase.LoadAssetAtPath<KnotCatalog>(AssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<KnotCatalog>();
                AssetDatabase.CreateAsset(catalog, AssetPath);
            }

            // Существующий порядок не трогаем: Восьмёрка всегда первая, Булинь — в конце, если его ещё нет.
            catalog.knots.Remove(figureEight);
            catalog.knots.Insert(0, figureEight);
            if (!catalog.knots.Contains(bowline)) catalog.knots.Add(bowline);

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }
    }
}
