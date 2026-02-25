using UnityEngine;

namespace VCableSystem
{
    public enum EndpointSide { Start, End }

    /// <summary>
    /// Маркер конца кабеля — только позиция и сторона.
    /// Логика подключения живёт в CableEndpointInteractable (через Connector из HPhysic).
    /// </summary>
    public class CableEndpoint : MonoBehaviour
    {
        [SerializeField] public CableDefinition Definition;
        [SerializeField] public EndpointSide    side;

        /// <summary>Устанавливается родительским VCable при Awake.</summary>
        public VCable Cable { get; internal set; }
    }
}
