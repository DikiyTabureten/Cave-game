using System.Collections.Generic;
using UnityEngine;

namespace CaveGame
{
    /// <summary>
    /// Пунктирная направляющая текущего шага, отметка его конца и кружок-«хват».
    /// При отклонении хват и направляющая краснеют.
    /// </summary>
    public sealed class KnotGuideView
    {
        private const int GuideOrder = -1;
        private const int TetherOrder = 49;
        private const int GripOrder = 50;
        private const float PulseSpeed = 4f;
        private const float PulseAmount = 0.12f;

        private readonly KnotVisualSettings _settings;
        private readonly float _gripDiameter;
        private readonly Material _dashMaterial;
        private readonly LineRenderer _guide;
        private readonly LineRenderer _tether;
        private readonly SpriteRenderer _grip;
        private readonly SpriteRenderer _target;
        private readonly List<Vector2> _buffer = new List<Vector2>();
        private Vector3[] _positions = new Vector3[256];

        /// <param name="gripDiameter">Диаметр хвата: на телефоне крупнее, чем в настройках.</param>
        public KnotGuideView(Transform parent, Material material, KnotVisualSettings settings, float gripDiameter)
        {
            _settings = settings;

            var root = new GameObject("Guide").transform;
            root.SetParent(parent, false);

            _dashMaterial = new Material(material) { name = "GuideDash", mainTexture = ProceduralTextures.Dash };
            _guide = LineFactory.Create(root, "Path", _dashMaterial, settings.guideWidth, settings.guideColor, GuideOrder);
            _guide.textureMode = LineTextureMode.Tile;
            _guide.textureScale = new Vector2(1f / settings.dashLength, 1f);
            _guide.numCapVertices = 0;

            _tether = LineFactory.Create(root, "Tether", material, settings.guideWidth * 0.6f, settings.errorColor, TetherOrder);

            _target = CreateCircle(root, "StepEnd", material, settings.ropeWidth * 1.4f, GuideOrder);
            _gripDiameter = gripDiameter;
            _grip = CreateCircle(root, "Grip", material, gripDiameter, GripOrder);
        }

        public void Hide()
        {
            _guide.enabled = false;
            _tether.enabled = false;
            _target.enabled = false;
            _grip.enabled = false;
        }

        /// <param name="stepPath">Путь текущего шага.</param>
        /// <param name="stepDistance">Сколько шага уже пройдено.</param>
        /// <param name="ropeEnd">Текущий конец верёвки.</param>
        /// <param name="pointer">Указатель в координатах узла.</param>
        public void Render(Polyline stepPath, float stepDistance, Vector2 ropeEnd, Vector2 pointer, bool holding, bool deviating)
        {
            Color guideColor = deviating ? WithAlpha(_settings.errorColor, 0.75f) : _settings.guideColor;

            stepPath.CopyRange(stepDistance, stepPath.Length, _buffer);
            LineFactory.SetPoints(_guide, _buffer, ref _positions);
            LineFactory.SetColor(_guide, guideColor);
            _guide.enabled = _buffer.Count > 1;

            _target.enabled = true;
            _target.color = guideColor;
            _target.transform.localPosition = stepPath.End;

            // Пока конец держат, хват под указателем; иначе — на конце верёвки и мягко пульсирует, приглашая взять.
            _grip.enabled = true;
            _grip.color = deviating ? _settings.errorColor : _settings.gripColor;
            _grip.transform.localPosition = holding ? pointer : ropeEnd;
            float pulse = holding ? 1f : 1f + Mathf.Sin(Time.time * PulseSpeed) * PulseAmount;
            _grip.transform.localScale = Vector3.one * (_gripDiameter * pulse);

            // При отклонении — тонкая красная линия от конца верёвки к указателю: видно, куда вернуться.
            _tether.enabled = holding && deviating;
            if (_tether.enabled)
            {
                _buffer.Clear();
                _buffer.Add(ropeEnd);
                _buffer.Add(pointer);
                LineFactory.SetPoints(_tether, _buffer, ref _positions);
            }
        }

        private static SpriteRenderer CreateCircle(Transform parent, string name, Material material, float diameter, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one * diameter;

            var sprite = go.AddComponent<SpriteRenderer>();
            sprite.sprite = ProceduralTextures.Circle;
            sprite.sharedMaterial = material;
            sprite.sortingOrder = order;
            return sprite;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
