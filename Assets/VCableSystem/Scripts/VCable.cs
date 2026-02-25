using UnityEngine;

namespace VCableSystem
{
    /// <summary>
    /// Главный компонент кабеля.
    /// Связывает симулятор, рендерер и два endpoint'а.
    /// [ExecuteAlways] — кабель виден прямо в Edit Mode.
    ///
    /// Структура GameObject'а:
    ///   VCable (этот компонент + CableRenderer + LineRenderer)
    ///   ├── StartEndpoint  (CableEndpoint + CableEndpointInteractable + Rigidbody + SphereCollider)
    ///   └── EndEndpoint    (то же самое)
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(CableRenderer))]
    [RequireComponent(typeof(LineRenderer))]
    public class VCable : MonoBehaviour
    {
        [Header("Definition")]
        [SerializeField] private CableDefinition definition;

        [Header("Endpoints")]
        [SerializeField] private CableEndpoint startEndpoint;
        [SerializeField] private CableEndpoint endEndpoint;

        [Header("Simulation")]
        [Tooltip("Слои геометрии с которой кабель сталкивается (пол, стены). Исключи слои самого кабеля и игрока.")]
        [SerializeField] private LayerMask collisionMask = 1; // Default layer
        [SerializeField, Range(0.01f, 0.1f)] private float collisionRadius = 0.035f;
        [SerializeField, Range(4, 64)]    private int   pointCount        = 16;
        [SerializeField, Range(1, 32)]    private int   solverIterations  = 10;
        [SerializeField, Range(0.9f, 1f)] private float damping            = 0.985f;

        private CableSimulator _sim;
        private CableRenderer  _cableRenderer;

        // Кешируем параметры чтобы обнаружить изменение в инспекторе
        private CableDefinition _lastDefinition;
        private int             _lastPointCount;

        // ---- Публичные свойства ----

        public CableDefinition Definition    => definition;
        public CableEndpoint   StartEndpoint => startEndpoint;
        public CableEndpoint   EndEndpoint   => endEndpoint;

        public bool IsFullyConnected
        {
            get
            {
                var sc = startEndpoint != null ? startEndpoint.GetComponent<HPhysic.Connector>() : null;
                var ec = endEndpoint   != null ? endEndpoint.GetComponent<HPhysic.Connector>()   : null;
                return sc != null && sc.IsConnected && ec != null && ec.IsConnected;
            }
        }

        // ---- Unity lifecycle ----

        private void Awake()
        {
            _cableRenderer = GetComponent<CableRenderer>();

            if (Application.isPlaying)
            {
                if (startEndpoint != null) startEndpoint.Cable = this;
                if (endEndpoint   != null) endEndpoint.Cable   = this;
            }
        }

        private void OnEnable()
        {
            _sim = null; // заставит пересоздать при следующем Update
        }

        private void Update()
        {
            // Пересоздаём симулятор если параметры изменились (или он ещё не создан)
            bool definitionChanged = definition != _lastDefinition;
            bool countChanged      = pointCount  != _lastPointCount;

            if (_sim == null || definitionChanged || countChanged)
                InitializeSimulator();

            if (_sim == null || _cableRenderer == null) return;

            // В Edit Mode двигаем только крайние точки (нет симуляции)
            if (!Application.isPlaying)
            {
                _sim.SetPoint(0, startEndpoint != null
                    ? startEndpoint.transform.position
                    : transform.position);

                _sim.SetPoint(_sim.PointCount - 1, endEndpoint != null
                    ? endEndpoint.transform.position
                    : transform.position + transform.forward * 1.5f);
            }

            _cableRenderer.UpdatePositions(_sim);
        }

        private void FixedUpdate()
        {
            if (!Application.isPlaying || _sim == null) return;

            if (startEndpoint != null)
                _sim.SetPoint(0, startEndpoint.transform.position);

            if (endEndpoint != null)
                _sim.SetPoint(_sim.PointCount - 1, endEndpoint.transform.position);

            _sim.Simulate(Time.fixedDeltaTime);
        }

        private void InitializeSimulator()
        {
            if (_cableRenderer == null) _cableRenderer = GetComponent<CableRenderer>();

            float totalLength = definition != null ? definition.maxLength * 0.5f : 1.5f;
            int   segments    = definition != null ? definition.segmentCount : pointCount;
            segments = Mathf.Max(segments, 2);

            _sim = new CableSimulator(segments, totalLength, solverIterations, damping);
            _sim.EnableCollision(collisionMask, collisionRadius);

            Vector3 startPos = startEndpoint != null
                ? startEndpoint.transform.position
                : transform.position;

            Vector3 endPos = endEndpoint != null
                ? endEndpoint.transform.position
                : transform.position + transform.forward * 1.5f;

            _sim.Initialize(startPos, endPos);

            if (definition != null)
                _cableRenderer.Setup(definition.color, definition.thickness);
            else
                _cableRenderer.Setup(Color.gray, 0.025f);

            _lastDefinition = definition;
            _lastPointCount = pointCount;

            if (Application.isPlaying)
                CableManager.Instance?.RegisterCable(this);
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
                CableManager.Instance?.UnregisterCable(this);
        }

        // ---- Публичный API ----

        public void PinPoint(int index, bool pinned) => _sim?.PinPoint(index, pinned);
        public void MovePoint(int index, Vector3 worldPos) => _sim?.SetPoint(index, worldPos);
        public int PointCount => _sim?.PointCount ?? 0;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (startEndpoint == null || endEndpoint == null) return;
            Gizmos.color = definition != null ? definition.color : Color.white;
            Gizmos.DrawLine(startEndpoint.transform.position, endEndpoint.transform.position);
        }
#endif
    }
}
