using UnityEngine;

namespace VCableSystem
{
    /// <summary>
    /// Verlet-симуляция верёвки без Rigidbody.
    /// Коллизии — стабильный per-point OverlapSphere + ClosestPoint.
    /// Нет Linecast между сегментами — нет дрожания.
    /// </summary>
    public class CableSimulator
    {
        private readonly Vector3[] _positions;
        private readonly Vector3[] _prevPositions;
        private readonly bool[]    _pinned;

        private readonly float _segmentLength;
        private readonly int   _iterations;
        private readonly float _damping;

        private bool      _collisionEnabled;
        private LayerMask _collisionMask;
        private float     _pointRadius;

        private static readonly Collider[] _overlapBuffer = new Collider[4];

        public int   PointCount    => _positions.Length;
        public float SegmentLength => _segmentLength;

        public CableSimulator(int pointCount, float totalLength,
                              int iterations = 10, float damping = 0.985f)
        {
            _positions     = new Vector3[pointCount];
            _prevPositions = new Vector3[pointCount];
            _pinned        = new bool[pointCount];
            _segmentLength = totalLength / (pointCount - 1);
            _iterations    = iterations;
            _damping       = damping;
        }

        /// <summary>Включить столкновение с геометрией.</summary>
        public void EnableCollision(LayerMask mask, float pointRadius = 0.025f)
        {
            _collisionEnabled = true;
            _collisionMask    = mask;
            _pointRadius      = pointRadius;
        }

        /// <summary>Расставляет точки по прямой между start и end.</summary>
        public void Initialize(Vector3 start, Vector3 end)
        {
            for (int i = 0; i < _positions.Length; i++)
            {
                float t = (float)i / (_positions.Length - 1);
                _positions[i]     = Vector3.Lerp(start, end, t);
                _prevPositions[i] = _positions[i];
            }
            _pinned[0]                     = true;
            _pinned[_positions.Length - 1] = true;
        }

        public void SetPoint(int index, Vector3 worldPos)
        {
            if (index < 0 || index >= _positions.Length) return;
            _positions[index]     = worldPos;
            _prevPositions[index] = worldPos;
        }

        public void PinPoint(int index, bool pinned)
        {
            if (index < 0 || index >= _positions.Length) return;
            _pinned[index] = pinned;
        }

        public bool IsPinned(int index) =>
            index >= 0 && index < _pinned.Length && _pinned[index];

        public Vector3 GetPoint(int index) => _positions[index];

        public void Simulate(float dt)
        {
            // 1. Verlet-интеграция
            for (int i = 0; i < _positions.Length; i++)
            {
                if (_pinned[i]) continue;

                Vector3 vel       = (_positions[i] - _prevPositions[i]) * _damping;
                _prevPositions[i] = _positions[i];
                _positions[i]    += vel + Vector3.down * (9.81f * dt * dt);
            }

            // 2. Итерации: длина + коллизия вместе
            // Коллизия внутри цикла — исправления длины сразу корректируются коллизией
            for (int iter = 0; iter < _iterations; iter++)
            {
                ApplyLengthConstraints();
                if (_collisionEnabled)
                    ApplyCollisions();
            }
        }

        private void ApplyLengthConstraints()
        {
            for (int i = 0; i < _positions.Length - 1; i++)
            {
                Vector3 dir  = _positions[i + 1] - _positions[i];
                float   dist = dir.magnitude;
                if (dist < 0.0001f) continue;

                float   error = (dist - _segmentLength) * 0.5f;
                Vector3 corr  = dir.normalized * error;

                bool pinA = _pinned[i];
                bool pinB = _pinned[i + 1];

                if      (!pinA && !pinB) { _positions[i] += corr;      _positions[i + 1] -= corr; }
                else if (!pinA)          { _positions[i] += corr * 2f; }
                else if (!pinB)          { _positions[i + 1] -= corr * 2f; }
            }
        }

        // Стабильная коллизия без осцилляций
        private void ApplyCollisions()
        {
            for (int i = 0; i < _positions.Length; i++)
            {
                if (_pinned[i]) continue;

                int count = Physics.OverlapSphereNonAlloc(
                    _positions[i], _pointRadius, _overlapBuffer,
                    _collisionMask, QueryTriggerInteraction.Ignore);

                for (int c = 0; c < count; c++)
                {
                    if (_overlapBuffer[c] == null) continue;

                    Vector3 closest = _overlapBuffer[c].ClosestPoint(_positions[i]);
                    Vector3 delta   = _positions[i] - closest;
                    float   dist    = delta.magnitude;

                    if (dist >= _pointRadius) continue; // вне зоны столкновения

                    // Определяем направление выталкивания
                    Vector3 pushDir;
                    if (dist < 0.0001f)
                    {
                        if (Physics.Raycast(_positions[i] + Vector3.up * 0.5f, Vector3.down,
                            out RaycastHit hit, 1f, _collisionMask, QueryTriggerInteraction.Ignore))
                            pushDir = hit.normal;
                        else
                            pushDir = Vector3.up;
                    }
                    else
                    {
                        pushDir = delta / dist;
                    }

                    // Выдвигаем точку на границу радиуса
                    _positions[i] = closest + pushDir * _pointRadius;

                    // Считаем скорость ПОСЛЕ коррекции позиции (vel = correctedPos - prevPos)
                    // Убираем компоненту вдоль нормали полностью (restitution = 0)
                    Vector3 vel       = _positions[i] - _prevPositions[i];
                    float   normalVel = Vector3.Dot(vel, pushDir);
                    _prevPositions[i] += pushDir * normalVel;
                }
            }
        }
    }
}
