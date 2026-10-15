using UnityEngine;

namespace CaveGame
{
    public enum PlatformMode
    {
        /// <summary>По платформе: телефон — мобильный режим, ПК и браузер — нет.</summary>
        Auto,
        /// <summary>Всегда как на ПК (мышь).</summary>
        Desktop,
        /// <summary>Всегда как на телефоне — удобно проверять в редакторе.</summary>
        Mobile
    }

    public static class PlatformProfile
    {
        public static bool IsMobile(PlatformMode mode, bool isMobilePlatform)
        {
            switch (mode)
            {
                case PlatformMode.Mobile: return true;
                case PlatformMode.Desktop: return false;
                default: return isMobilePlatform;
            }
        }

        public static bool IsMobile(PlatformMode mode)
        {
            return IsMobile(mode, Application.isMobilePlatform);
        }
    }
}
