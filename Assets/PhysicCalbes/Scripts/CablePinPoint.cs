using UnityEngine;
using HInteractions;

namespace HPhysic
{
    /// <summary>
    /// Добавляется на каждую физическую точку кабеля.
    /// Можно поднять как обычный объект. При броске рядом с Pinable-поверхностью — прибивается к ней.
    /// Повторный подъём — открепляет.
    /// </summary>
    public class CablePinPoint : Liftable
    {
        [Header("Pin Settings")]
        [SerializeField] private float pinSearchRadius = 0.8f;
        [SerializeField] private Color pinnedColor = new Color(1f, 0.45f, 0f); // оранжевый — "прибит"

        private bool _isPinned;
        private Pinable _currentHighlightedPinable; // Pinable которая сейчас подсвечена
        private PhysicCable _physicCable;

        // Состояния гравитации остальных точек кабеля до подъёма
        private readonly System.Collections.Generic.List<(Rigidbody rb, bool wasGravity, bool wasKinematic)> _savedPointStates
            = new System.Collections.Generic.List<(Rigidbody, bool, bool)>();

        public bool IsPinned => _isPinned;

        protected override void Awake()
        {
            base.Awake();
            _physicCable = GetComponentInParent<PhysicCable>();
        }

        // Пока сегмент держат в руках — ищем ближайший Pinable и подсвечиваем его
        private void Update()
        {
            if (!IsLift) return;

            Pinable nearest = FindNearestPinable().pinable;

            if (nearest != _currentHighlightedPinable)
            {
                _currentHighlightedPinable?.Unhighlight();
                _currentHighlightedPinable = nearest;
                _currentHighlightedPinable?.Highlight();
            }
        }

        public override void PickUp(IObjectHolder holder, int layer)
        {
            // Если был прибит — сначала открепляем
            if (_isPinned)
                Unpin();

            base.PickUp(holder, layer);

            // Отключаем гравитацию всех остальных точек кабеля — иначе они тянут поднятый сегмент вниз
            _savedPointStates.Clear();
            if (_physicCable != null && _physicCable.Points != null)
            {
                foreach (Transform point in _physicCable.Points)
                {
                    if (point == null || point == transform) continue;
                    Rigidbody rb = point.GetComponent<Rigidbody>();
                    if (rb == null) continue;

                    _savedPointStates.Add((rb, rb.useGravity, rb.isKinematic));

                    if (!rb.isKinematic) // не трогаем прибитые (kinematic) точки
                        rb.useGravity = false;
                }
            }
        }

        public override void Drop()
        {
            // Восстанавливаем гравитацию остальных точек кабеля
            foreach (var state in _savedPointStates)
                if (!state.rb.isKinematic) // снова не трогаем kinematic (прибитые)
                    state.rb.useGravity = state.wasGravity;
            _savedPointStates.Clear();
            // Убираем подсветку с Pinable
            _currentHighlightedPinable?.Unhighlight();

            var (nearest, nearestCol) = FindNearestPinable();
            if (nearest != null && nearestCol != null)
            {
                // Бросили рядом с поверхностью — прибиваем точно к ней
                base.Drop();
                Pin(nearestCol);
            }
            else
            {
                // Просто бросить
                base.Drop();
            }

            _currentHighlightedPinable = null;
        }

        public override void Deselect()
        {
            base.Deselect();
            if (_isPinned)
                ApplyPinnedColor();
        }

        private void Pin(Collider surface)
        {
            _isPinned = true;

            // Находим ближайшую точку на поверхности и прилипаем к ней
            Vector3 closestPoint = surface.ClosestPoint(transform.position);
            // Вычисляем нормаль поверхности: от closest к нашей позиции
            Vector3 normal = (transform.position - closestPoint).normalized;
            if (normal == Vector3.zero) normal = Vector3.up;
            // Смещаем сегмент чтобы он лежал на поверхности, а не внутри
            float halfSize = transform.localScale.x * 0.5f;
            transform.position = closestPoint + normal * halfSize;

            Rigidbody.isKinematic = true;
            Rigidbody.velocity = Vector3.zero;
            Rigidbody.angularVelocity = Vector3.zero;
            ApplyPinnedColor();
        }

        private void Unpin()
        {
            _isPinned = false;
            Rigidbody.isKinematic = false;
            Deselect(); // сбрасывает цвет на оригинальный
        }

        private (Pinable pinable, Collider collider) FindNearestPinable()
        {
            Collider[] nearby = Physics.OverlapSphere(transform.position, pinSearchRadius);
            float bestDist = float.MaxValue;
            Pinable best = null;
            Collider bestCol = null;
            foreach (Collider col in nearby)
            {
                if (col.gameObject == gameObject) continue;
                Pinable p = col.GetComponent<Pinable>();
                if (p == null) continue;
                // Используем ближайшую точку на коллайдере, а не центр объекта
                Vector3 closest = col.ClosestPoint(transform.position);
                float d = Vector3.Distance(transform.position, closest);
                if (d < bestDist) { bestDist = d; best = p; bestCol = col; }
            }
            return (best, bestCol);
        }

        private void ApplyPinnedColor()
        {
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                foreach (Material m in r.materials)
                    if (m.HasProperty("_Color")) m.color = pinnedColor;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _isPinned ? new Color(1f, 0.45f, 0f) : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, pinSearchRadius);
        }
#endif
    }
}
