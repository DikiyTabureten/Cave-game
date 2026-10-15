using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace CaveGame
{
    /// <summary>
    /// Указатель через Input System: касания тачскрина и левая кнопка мыши. Из нескольких касаний
    /// <see cref="PointerTracker"/> выбирает одно — мультитач не ломает вязку.
    /// </summary>
    public sealed class InputSystemPointerInput : IPointerInput
    {
        // У мыши нет touchId — берём заведомо не пересекающийся с касаниями.
        private const int MouseContactId = -1;

        private readonly PointerTracker _tracker = new PointerTracker();
        private readonly List<PointerContact> _contacts = new List<PointerContact>();

        public PointerSample Poll()
        {
            _contacts.Clear();

            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                foreach (var touch in touchscreen.touches)
                {
                    if (!touch.isInProgress) continue;
                    _contacts.Add(new PointerContact(touch.touchId.ReadValue(), touch.position.ReadValue()));
                }
            }

            var mouse = Mouse.current;
            if (mouse != null && mouse.leftButton.isPressed)
            {
                _contacts.Add(new PointerContact(MouseContactId, mouse.position.ReadValue()));
            }

            return _tracker.Update(_contacts);
        }
    }
}
