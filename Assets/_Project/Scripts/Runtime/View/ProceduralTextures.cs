using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Текстуры-заглушки, которые генерируются кодом: круг и штрих для пунктира.
    /// </summary>
    public static class ProceduralTextures
    {
        private static Sprite _circle;
        private static Texture2D _dash;

        /// <summary>Белый круг диаметром 1 юнит с мягким краем.</summary>
        public static Sprite Circle
        {
            get
            {
                if (_circle != null) return _circle;

                const int size = 128;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    name = "Circle",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };

                float radius = size / 2f;
                var pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dx = x + 0.5f - radius;
                        float dy = y + 0.5f - radius;
                        float alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy));
                        pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                    }
                }
                texture.SetPixels32(pixels);
                texture.Apply();

                _circle = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
                _circle.name = "Circle";
                return _circle;
            }
        }

        /// <summary>Штрих для пунктирной линии: первая половина непрозрачна, вторая прозрачна.</summary>
        public static Texture2D Dash
        {
            get
            {
                if (_dash != null) return _dash;

                const int width = 32;
                const int height = 4;
                _dash = new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    name = "Dash",
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Repeat
                };

                var pixels = new Color32[width * height];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        pixels[y * width + x] = new Color32(255, 255, 255, (byte)(x < width / 2 ? 255 : 0));
                    }
                }
                _dash.SetPixels32(pixels);
                _dash.Apply();
                return _dash;
            }
        }
    }
}
