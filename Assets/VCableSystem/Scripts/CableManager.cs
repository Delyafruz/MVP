using System.Collections.Generic;
using UnityEngine;

namespace VCableSystem
{
    /// <summary>
    /// Реестр всех VCable-кабелей в сцене.
    /// Соединения хранятся в Connector (HPhysic) — здесь только список кабелей.
    /// </summary>
    public class CableManager : MonoBehaviour
    {
        public static CableManager Instance { get; private set; }

        private readonly List<VCable> _cables = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void RegisterCable(VCable cable)
        {
            if (!_cables.Contains(cable)) _cables.Add(cable);
        }

        public void UnregisterCable(VCable cable) => _cables.Remove(cable);

        public IReadOnlyList<VCable> Cables => _cables;
    }
}
