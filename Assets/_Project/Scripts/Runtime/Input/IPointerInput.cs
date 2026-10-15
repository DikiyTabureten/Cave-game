using UnityEngine;

namespace CaveGame
{
    public enum PointerPhase
    {
        None,
        Pressed,
        Moved,
        Released
    }

    public readonly struct PointerSample
    {
        public readonly PointerPhase Phase;
        public readonly Vector2 ScreenPosition;

        public PointerSample(PointerPhase phase, Vector2 screenPosition)
        {
            Phase = phase;
            ScreenPosition = screenPosition;
        }
    }

    /// <summary>
    /// Абстракция указателя: нажат / двигается / отпущен. Мышь, перо и тач подключаются отдельными реализациями.
    /// </summary>
    public interface IPointerInput
    {
        /// <summary>Состояние указателя в текущем кадре. Вызывается один раз за кадр.</summary>
        PointerSample Poll();
    }
}
