using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Опора-заглушка: перекладина как толстая линия с тёмной обводкой. Лежит над основной верёвкой,
    /// но под верхними участками там, где верёвка проходит над ней.
    /// </summary>
    public sealed class KnotSupportView
    {
        private const float OutlineExtra = 0.08f;

        public KnotSupportView(Transform parent, KnotSupport support, Material material, KnotVisualSettings settings)
        {
            var root = new GameObject("Support").transform;
            root.SetParent(parent, false);

            support.GetSegment(out Vector2 start, out Vector2 end);
            // Обводка чуть длиннее перекладины, чтобы торцы тоже были очерчены.
            Vector2 extra = (end - start).normalized * OutlineExtra;

            CreateBar(root, "Outline", material, support.thickness + OutlineExtra * 2f, settings.supportOutlineColor,
                KnotRopeView.SupportOrder - 1, start - extra, end + extra);
            CreateBar(root, "Bar", material, support.thickness, settings.supportColor,
                KnotRopeView.SupportOrder, start, end);
        }

        private static void CreateBar(Transform root, string name, Material material, float width, Color color, int order,
            Vector2 start, Vector2 end)
        {
            var line = LineFactory.Create(root, name, material, width, color, order);
            line.numCapVertices = 0;
            var buffer = new Vector3[2];
            LineFactory.SetPoints(line, new List<Vector2> { start, end }, ref buffer);
        }
    }
}
