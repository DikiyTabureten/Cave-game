using UnityEngine;

namespace CaveGame
{
    /// <summary>Настройки приложения, которые задаются один раз при запуске.</summary>
    public static class AppBootstrap
    {
        private const int MobileFrameRate = 60;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Configure()
        {
            // На Android по умолчанию 30 кадров — верёвка за пальцем заметно дёргается.
            // На ПК и в браузере частоту оставляем системе (вертикальная синхронизация / requestAnimationFrame).
            if (Application.isMobilePlatform)
            {
                Application.targetFrameRate = MobileFrameRate;
            }
        }
    }
}
