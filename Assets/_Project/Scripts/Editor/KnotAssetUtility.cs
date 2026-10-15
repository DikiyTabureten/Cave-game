using UnityEditor;
using UnityEngine;

namespace CaveGame.EditorTools
{
    public static class KnotAssetUtility
    {
        /// <summary>
        /// Сохраняет свежесгенерированный узел в ассет. Если ассет уже есть — обновляет его на месте,
        /// чтобы не потерять GUID и ссылки из каталога и сцен.
        /// </summary>
        public static KnotDefinition SaveOrUpdate(KnotDefinition fresh, string assetPath)
        {
            string name = fresh.name;
            var existing = AssetDatabase.LoadAssetAtPath<KnotDefinition>(assetPath);
            if (existing != null)
            {
                EditorUtility.CopySerialized(fresh, existing);
                existing.name = name;
                Object.DestroyImmediate(fresh);
                EditorUtility.SetDirty(existing);
                AssetDatabase.SaveAssets();
                Debug.Log($"Узел обновлён: {assetPath}");
                return existing;
            }

            AssetDatabase.CreateAsset(fresh, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Узел создан: {assetPath}");
            return fresh;
        }
    }
}
