using System.Collections;
using UnityEngine;
using HInteractions;

namespace HPhysic
{
    [RequireComponent(typeof(Connector))]
    public class PhysicCableCon : Liftable
    {
        private Connector _connector;
        private PhysicCable _physicCable;
        private System.Collections.Generic.List<(GameObject obj, int defaultLayer)> _cablePointsDefaultLayers;

        protected override void Awake()
        {
            base.Awake();

            _connector = gameObject.GetComponent<Connector>();
            _physicCable = GetComponentInParent<PhysicCable>();
            _cablePointsDefaultLayers = new System.Collections.Generic.List<(GameObject, int)>();
        }

        public override void PickUp(IObjectHolder holder, int layer)
        {
            base.PickUp(holder, layer);

            if (_connector.ConnectedTo)
                _connector.Disconnect();

            // Меняем слой всех точек кабеля на heldObjectLayer чтобы избежать коллизий с игроком
            if (_physicCable != null)
            {
                _cablePointsDefaultLayers.Clear();
                var cablePoints = _physicCable.Points;
                if (cablePoints != null)
                {
                    foreach (Transform point in cablePoints)
                    {
                        if (point != null)
                        {
                            foreach (Collider col in point.GetComponentsInChildren<Collider>())
                            {
                                _cablePointsDefaultLayers.Add((col.gameObject, col.gameObject.layer));
                                col.gameObject.layer = layer;
                            }
                        }
                    }
                }
            }
        }

        public override void Drop()
        {
            // Восстанавливаем исходные слои для всех точек кабеля
            foreach ((GameObject obj, int defaultLayer) item in _cablePointsDefaultLayers)
                item.obj.layer = item.defaultLayer;
            _cablePointsDefaultLayers.Clear();

            if (ObjectHolder.SelectedObject && ObjectHolder.SelectedObject.TryGetComponent(out Connector secondConnector))
            {
                if (_connector.CanConnect(secondConnector))
                    secondConnector.Connect(_connector);
                else if (!secondConnector.IsConnected)
                {
                    transform.rotation = secondConnector.ConnectionRotation * _connector.RotationOffset;
                    transform.position = (secondConnector.ConnectionPosition + secondConnector.ConnectedOutOffset * 0.2f) - (_connector.ConnectionPosition - _connector.transform.position);
                }
            }

            base.Drop();
        }
    }
}