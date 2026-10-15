namespace CaveGame
{
    /// <summary>Русские подписи с числами.</summary>
    public static class RussianText
    {
        /// <summary>«1 шаг», «3 шага», «5 шагов», «11 шагов», «21 шаг».</summary>
        public static string Steps(int count)
        {
            return $"{count} {Plural(count, "шаг", "шага", "шагов")}";
        }

        /// <param name="one">Форма для 1, 21, 31…</param>
        /// <param name="few">Форма для 2–4, 22–24…</param>
        /// <param name="many">Форма для 0, 5–20, 25–30…</param>
        public static string Plural(int count, string one, string few, string many)
        {
            int lastTwo = System.Math.Abs(count) % 100;
            int last = lastTwo % 10;
            if (lastTwo >= 11 && lastTwo <= 14) return many;
            if (last == 1) return one;
            if (last >= 2 && last <= 4) return few;
            return many;
        }
    }
}
