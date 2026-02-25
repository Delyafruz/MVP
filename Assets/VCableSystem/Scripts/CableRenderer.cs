using UnityEngine;

namespace VCableSystem
{
    /// <summary>
    /// Отображает кабель через LineRenderer с Catmull-Rom сглаживанием.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(LineRenderer))]
    public class CableRenderer : MonoBehaviour
    {
        [SerializeField, Range(1, 8)] private int smoothSegmentsPerPoint = 4;

        [Tooltip("Материал для кабеля. Если не назначен — создаётся автоматически.")]
        [SerializeField] private Material lineMaterial;

        private LineRenderer _lr;

        private LineRenderer LR
        {
            get
            {
                if (_lr == null) _lr = GetComponent<LineRenderer>();
                return _lr;
            }
        }

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
        }

        public void Setup(Color color, float width)
        {
            if (LR == null) return;

            Material mat;
            if (lineMaterial != null)
            {
                mat = new Material(lineMaterial);
            }
            else
            {
                // Default-Line.mat — встроенный ресурс Unity (Built-in RP)
                Material builtIn = Resources.GetBuiltinResource<Material>("Default-Line.mat");
                if (builtIn != null)
                {
                    mat = new Material(builtIn);
                }
                else
                {
                    // Абсолютный fallback — Unlit/Color всегда есть в Built-in RP
                    Shader s = Shader.Find("Unlit/Color");
                    if (s == null) s = Shader.Find("UI/Default");
                    mat = new Material(s);
                }
            }

            // Применяем цвет — проверяем наличие свойства
            if (mat.HasProperty("_Color"))  mat.SetColor("_Color",  color);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color); // URP

            LR.material         = mat;
            LR.useWorldSpace    = true;
            LR.startWidth       = width;
            LR.endWidth         = width;
            LR.startColor       = color;
            LR.endColor         = color;
            LR.numCornerVertices = 4;
            LR.numCapVertices    = 4;
            LR.textureMode      = LineTextureMode.Stretch;
            LR.alignment        = LineAlignment.View;
            LR.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            LR.receiveShadows   = false;
        }

        public void UpdatePositions(CableSimulator sim)
        {
            if (LR == null) return;

            int rawCount    = sim.PointCount;
            int smoothCount = (rawCount - 1) * smoothSegmentsPerPoint + 1;

            LR.positionCount = smoothCount;

            // Собираем сырые точки
            Vector3[] raw = new Vector3[rawCount];
            for (int i = 0; i < rawCount; i++)
                raw[i] = sim.GetPoint(i);

            // Catmull-Rom интерполяция между точками
            int idx = 0;
            for (int i = 0; i < rawCount - 1; i++)
            {
                Vector3 p0 = raw[Mathf.Max(i - 1, 0)];
                Vector3 p1 = raw[i];
                Vector3 p2 = raw[i + 1];
                Vector3 p3 = raw[Mathf.Min(i + 2, rawCount - 1)];

                for (int s = 0; s < smoothSegmentsPerPoint; s++)
                {
                    float t = (float)s / smoothSegmentsPerPoint;
                    LR.SetPosition(idx++, CatmullRom(p0, p1, p2, p3, t));
                }
            }
            LR.SetPosition(idx, raw[rawCount - 1]);
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * (
                2f * p1 +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }
    }
}
