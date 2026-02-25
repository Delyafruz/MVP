using UnityEngine;

namespace VCableSystem
{
    public enum PortStatus { Free, Occupied, Error, Disabled }

    /// <summary>
    /// Разъём на устройстве (свитч, сервер, патч-панель).
    /// Знает свой тип порта, принимает или отвергает CableEndpoint.
    /// </summary>
    public class CablePort : MonoBehaviour
    {
        [Header("Port Info")]
        [SerializeField] public PortType portType  = PortType.RJ45;
        [SerializeField] public string   portLabel = "eth0";

        [Header("LED (optional)")]
        [SerializeField] private Renderer statusLed;
        [SerializeField] private Color    colorFree     = Color.green;
        [SerializeField] private Color    colorOccupied = new Color(1f, 0.6f, 0f);
        [SerializeField] private Color    colorError    = Color.red;
        [SerializeField] private Color    colorDisabled = Color.gray;

        public PortStatus    Status           { get; private set; } = PortStatus.Free;
        public CableEndpoint ConnectedEndpoint { get; private set; }

        // ---- Публичный API ----

        public bool CanAccept(CableEndpoint endpoint)
        {
            if (Status != PortStatus.Free)    return false;
            if (endpoint.Definition == null)  return false;
            return endpoint.Definition.plugType == portType;
        }

        public bool Connect(CableEndpoint endpoint)
        {
            if (!CanAccept(endpoint)) return false;

            ConnectedEndpoint = endpoint;
            Status            = PortStatus.Occupied;
            UpdateLed();
            return true;
        }

        public void Disconnect()
        {
            if (ConnectedEndpoint == null) return;

            ConnectedEndpoint = null;
            Status            = PortStatus.Free;
            UpdateLed();
        }

        public void SetError(bool hasError)
        {
            if (Status == PortStatus.Disabled) return;
            Status = hasError ? PortStatus.Error : (ConnectedEndpoint != null ? PortStatus.Occupied : PortStatus.Free);
            UpdateLed();
        }

        public void SetDisabled(bool disabled)
        {
            Status = disabled ? PortStatus.Disabled : PortStatus.Free;
            UpdateLed();
        }

        // ---- Приватное ----

        private void UpdateLed()
        {
            if (statusLed == null) return;

            Color c = Status switch
            {
                PortStatus.Free     => colorFree,
                PortStatus.Occupied => colorOccupied,
                PortStatus.Error    => colorError,
                PortStatus.Disabled => colorDisabled,
                _                   => colorFree
            };

            var mpb = new MaterialPropertyBlock();
            statusLed.GetPropertyBlock(mpb);
            mpb.SetColor("_EmissionColor", c * 2f);
            statusLed.SetPropertyBlock(mpb);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Status == PortStatus.Free ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.04f);
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, transform.forward * 0.08f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.08f, portLabel);
#endif
        }
    }
}
