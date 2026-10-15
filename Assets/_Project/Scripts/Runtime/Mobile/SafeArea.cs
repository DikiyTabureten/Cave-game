using UnityEngine;

namespace CaveGame
{
    public static class SafeAreaMath
    {
        /// <summary>Якоря RectTransform, которые вписывают его в безопасную область экрана.</summary>
        public static void ToAnchors(Rect safeArea, Vector2 screenSize, out Vector2 anchorMin, out Vector2 anchorMax)
        {
            if (screenSize.x <= 0f || screenSize.y <= 0f)
            {
                anchorMin = Vector2.zero;
                anchorMax = Vector2.one;
                return;
            }

            anchorMin = new Vector2(safeArea.xMin / screenSize.x, safeArea.yMin / screenSize.y);
            anchorMax = new Vector2(safeArea.xMax / screenSize.x, safeArea.yMax / screenSize.y);
        }
    }

    /// <summary>
    /// Растягивает RectTransform на Screen.safeArea: интерфейс не уходит под вырез камеры
    /// и закругления экрана. Обновляется при смене ориентации и размера окна.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _appliedArea;
        private Vector2Int _appliedScreen;

        private void Awake()
        {
            _rect = (RectTransform)transform;
            Apply();
        }

        private void Update()
        {
            if (Screen.safeArea != _appliedArea || _appliedScreen.x != Screen.width || _appliedScreen.y != Screen.height)
            {
                Apply();
            }
        }

        private void Apply()
        {
            _appliedArea = Screen.safeArea;
            _appliedScreen = new Vector2Int(Screen.width, Screen.height);
            SafeAreaMath.ToAnchors(_appliedArea, _appliedScreen, out var min, out var max);
            _rect.anchorMin = min;
            _rect.anchorMax = max;
            _rect.offsetMin = _rect.offsetMax = Vector2.zero;
        }
    }
}
