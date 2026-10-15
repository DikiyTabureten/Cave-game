using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace CaveGame
{
    /// <summary>
    /// Связывает ввод, логику вязки (<see cref="KnotSession"/>) и представление.
    /// Всё визуальное создаётся кодом на старте; в сцене нужны только этот компонент и камера.
    /// </summary>
    public class KnotPrototypeController : MonoBehaviour
    {
        [Tooltip("Каталог узлов: откуда берётся выбранный узел и следующий за ним")]
        [SerializeField] private KnotCatalog catalog;
        [Tooltip("Неосвещаемый спрайтовый материал для линий и кружков")]
        [SerializeField] private Material lineMaterial;
        [SerializeField] private Camera targetCamera;
        [Tooltip("Отступ вокруг узла при подгонке камеры, в юнитах")]
        [SerializeField] private float cameraPadding = 1.2f;
        [Tooltip("Доля высоты экрана сверху, которую занимает интерфейс")]
        [Range(0f, 0.5f)]
        [SerializeField] private float topUiFraction = 0.28f;
        [SerializeField] private KnotVisualSettings visuals = new KnotVisualSettings();

        private KnotNavigator _navigator;
        private KnotDefinition _knot;
        private KnotSession _session;
        private KnotLayout _layout;
        private KnotRopeView _ropeView;
        private KnotGuideView _guideView;
        private KnotHud _hud;
        private IPointerInput _input;
        private Vector2 _pointer;
        private bool _ignoreUntilRelease;

        public KnotSession Session => _session;
        public KnotLayout Layout => _layout;

        private void Awake()
        {
            if (catalog == null || catalog.AvailableKnots.Count == 0 || lineMaterial == null)
            {
                Debug.LogError("KnotPrototypeController: не назначены каталог узлов или материал. " +
                               "Пересоберите сцены через Tools/Cave Game/Build All Scenes.", this);
                enabled = false;
                return;
            }

            // Узел выбирают в меню. Если сцену открыли напрямую, начинаем с первого узла каталога.
            if (KnotSelection.Navigator == null) KnotSelection.Select(catalog.AvailableKnots, 0);
            _navigator = KnotSelection.Navigator;
            _knot = _navigator.Current;

            if (targetCamera == null) targetCamera = Camera.main;
            targetCamera.backgroundColor = visuals.backgroundColor;
            targetCamera.clearFlags = CameraClearFlags.SolidColor;

            _input = new InputSystemPointerInput();
            _layout = new KnotLayout(_knot);
            _session = new KnotSession(_knot);
            _session.StepCompleted += _ => RefreshStepText();
            _session.Completed += OnCompleted;

            _ropeView = new KnotRopeView(transform, _knot, _layout, lineMaterial, visuals);
            _guideView = new KnotGuideView(transform, lineMaterial, visuals);
            _hud = new KnotHud(transform, _knot, visuals, _navigator.HasNext);
            _hud.RestartRequested += Restart;
            _hud.NextRequested += OpenNextKnot;
            _hud.MenuRequested += OpenMenu;

            _pointer = _session.EndPosition;
            RefreshStepText();
        }

        private void Update()
        {
            FitCamera();
            HandleInput();
            Render();
        }

        public void Restart()
        {
            _session.Reset();
            _ignoreUntilRelease = true;
            _hud.ShowCompletion(false);
            RefreshStepText();
        }

        public void OpenNextKnot()
        {
            if (!_navigator.MoveNext()) return;
            // Сцена пересобирается заново уже со следующим узлом из KnotSelection.
            SceneManager.LoadScene(SceneNames.KnotPrototype);
        }

        public void OpenMenu()
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
        }

        private void HandleInput()
        {
            var sample = _input.Poll();
            if (sample.Phase == PointerPhase.None) return;

            _pointer = ScreenToKnot(sample.ScreenPosition);

            switch (sample.Phase)
            {
                case PointerPhase.Pressed:
                    // Нажатие по кнопке интерфейса не должно хватать верёвку.
                    _ignoreUntilRelease = IsOverUi();
                    if (!_ignoreUntilRelease) _session.PointerDown(_pointer);
                    break;
                case PointerPhase.Moved:
                    if (!_ignoreUntilRelease) _session.PointerMove(_pointer);
                    break;
                case PointerPhase.Released:
                    _ignoreUntilRelease = false;
                    _session.PointerUp();
                    break;
            }
        }

        private void Render()
        {
            bool completed = _session.State == KnotSessionState.Completed;
            float drawn = completed
                ? _layout.Rope.Length
                : _layout.ToRopeDistance(_session.CurrentStepIndex, _session.StepDistance);
            _ropeView.Render(drawn);

            if (completed)
            {
                _guideView.Hide();
                return;
            }

            _guideView.Render(_layout.Step(_session.CurrentStepIndex), _session.StepDistance, _session.EndPosition,
                _pointer, _session.IsHolding, _session.IsDeviating);
            _hud.ShowDeviation(_session.IsHolding && _session.IsDeviating);
        }

        private void OnCompleted()
        {
            _hud.ShowCompletion(true);
        }

        private void RefreshStepText()
        {
            int index = Mathf.Min(_session.CurrentStepIndex, _knot.steps.Count - 1);
            _hud.ShowStep(index, _knot.steps.Count, _knot.steps[index].hint, visuals.SegmentColor(index));
        }

        private Vector2 ScreenToKnot(Vector2 screen)
        {
            float depth = transform.position.z - targetCamera.transform.position.z;
            Vector3 world = targetCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, depth));
            return transform.InverseTransformPoint(world);
        }

        /// <summary>
        /// Вписывает весь узел в кадр под интерфейсом с учётом соотношения сторон экрана.
        /// </summary>
        private void FitCamera()
        {
            var rope = _layout.Rope;
            Vector2 min = rope[0];
            Vector2 max = rope[0];
            for (int i = 1; i < rope.Count; i++)
            {
                min = Vector2.Min(min, rope[i]);
                max = Vector2.Max(max, rope[i]);
            }

            Vector2 size = max - min + Vector2.one * (cameraPadding * 2f);
            float aspect = Mathf.Max(0.1f, targetCamera.aspect);
            float visibleHeightFraction = 1f - topUiFraction;
            float halfHeight = Mathf.Max(size.y / visibleHeightFraction, size.x / aspect) * 0.5f;
            targetCamera.orthographicSize = halfHeight;

            // Сдвигаем камеру вверх, чтобы узел оказался в свободной нижней части экрана.
            Vector2 center = transform.TransformPoint((min + max) * 0.5f);
            float shift = halfHeight * topUiFraction;
            var cameraTransform = targetCamera.transform;
            cameraTransform.position = new Vector3(center.x, center.y + shift, cameraTransform.position.z);
        }

        private static bool IsOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
