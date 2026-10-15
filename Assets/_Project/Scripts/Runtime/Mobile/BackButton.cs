using UnityEngine.InputSystem;

namespace CaveGame
{
    /// <summary>
    /// Системная кнопка «Назад» Android. В Input System она приходит как клавиша Escape,
    /// поэтому на ПК то же действие выполняет Esc.
    /// </summary>
    public static class BackButton
    {
        public static bool WasPressed()
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard.escapeKey.wasPressedThisFrame;
        }
    }
}
