using UnityEngine;
using HInteractions;
using HPhysic;

namespace VCableSystem
{
    /// <summary>
    /// Конец кабеля, взаимодействующий с системой HInteractions.
    /// Подключение — через существующий Connector (FixedJoint, Male/Female, CableType).
    /// Логика Drop полностью повторяет PhysicCableCon.
    /// </summary>
    [RequireComponent(typeof(Connector))]
    [RequireComponent(typeof(CableEndpoint))]
    public class CableEndpointInteractable : Liftable
    {
        private Connector _connector;

        protected override void Awake()
        {
            base.Awake();
            _connector = GetComponent<Connector>();

            // По умолчанию — динамический, чтобы endpoint падал и реагировал на физику.
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity  = true;
        }

        public override void PickUp(IObjectHolder holder, int layer)
        {
            // Отсоединяем если был воткнут
            if (_connector.IsConnected)
                _connector.Disconnect();

            // Включаем динамическую физику — PlayerInteractions двигает через velocity
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity  = false;

            base.PickUp(holder, layer);
        }

        public override void Drop()
        {
            // Возвращаем динамическое поведение: физика и гравитация включены.
            Rigidbody.isKinematic = false;
            Rigidbody.useGravity  = true;
            Rigidbody.velocity    = Vector3.zero;

            // Проверяем, смотрит ли игрок на подходящий Connector
            if (ObjectHolder?.SelectedObject != null &&
                ObjectHolder.SelectedObject.TryGetComponent(out Connector target))
            {
                if (_connector.CanConnect(target))
                {
                    // Подключаем — FixedJoint создаётся внутри Connector.Connect
                    target.Connect(_connector);
                }
                else if (!target.IsConnected)
                {
                    // Не подходит, но порт свободен — приближаем к нему (как в PhysicCableCon)
                    transform.rotation = target.ConnectionRotation * _connector.RotationOffset;
                    transform.position = (target.ConnectionPosition
                                         + target.ConnectedOutOffset * 0.2f)
                                        - (_connector.ConnectionPosition - _connector.transform.position);
                }
            }

            base.Drop();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!IsLift) return;
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, 0.15f);
        }
#endif
    }
}
