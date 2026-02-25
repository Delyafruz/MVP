using UnityEngine;

namespace VCableSystem
{
    public enum CableType { EthernetCat5e, EthernetCat6, FiberOM3, PowerIEC, Console }
    public enum PortType  { RJ45, SFP, IEC_C13, IEC_C14, USB_A, USB_B, Console }

    [CreateAssetMenu(fileName = "NewCableDefinition", menuName = "VCableSystem/Cable Definition")]
    public class CableDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string      displayName = "Ethernet Cat5e";
        public CableType   cableType   = CableType.EthernetCat5e;
        public PortType    plugType    = PortType.RJ45;

        [Header("Visuals")]
        public Color color     = Color.blue;
        [Min(0.001f)] public float thickness = 0.02f;

        [Header("Behaviour")]
        [Min(0.5f)]  public float maxLength    = 10f;
        [Min(4)]     public int   segmentCount = 16;

        [Header("Network")]
        public float bandwidthGbps = 1f;
        public bool  isOptical     = false;
    }
}
