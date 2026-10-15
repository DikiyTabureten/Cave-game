using UnityEngine;
using UnityEngine.InputSystem;

namespace CaveGame
{
    /// <summary>
    /// Указатель через Input System. <see cref="Pointer.current"/> — последнее активное устройство:
    /// мышь, перо или основное касание тачскрина.
    /// </summary>
    public sealed class InputSystemPointerInput : IPointerInput
    {
        public PointerSample Poll()
        {
            var pointer = Pointer.current;
            if (pointer == null) return new PointerSample(PointerPhase.None, Vector2.zero);

            Vector2 position = pointer.position.ReadValue();
            var press = pointer.press;

            if (press.wasPressedThisFrame) return new PointerSample(PointerPhase.Pressed, position);
            if (press.wasReleasedThisFrame) return new PointerSample(PointerPhase.Released, position);
            if (press.isPressed) return new PointerSample(PointerPhase.Moved, position);
            return new PointerSample(PointerPhase.None, position);
        }
    }
}
