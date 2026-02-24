using UnityEngine;

namespace HPhysic
{
    /// <summary>
    /// Добавьте на стены/пол/потолок чтобы к ним можно было прибивать сегменты кабеля.
    /// </summary>
    public class Pinable : MonoBehaviour
    {
        [SerializeField] private Color highlightColor = new Color(0f, 1f, 0.4f, 1f); // зелёный

        private Renderer[] _renderers;
        private Color[][] _originalColors;
        private bool _isHighlighted;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _originalColors = new Color[_renderers.Length][];
            for (int i = 0; i < _renderers.Length; i++)
            {
                var mats = _renderers[i].materials;
                _originalColors[i] = new Color[mats.Length];
                for (int j = 0; j < mats.Length; j++)
                    _originalColors[i][j] = mats[j].HasProperty("_Color") ? mats[j].color : Color.white;
            }
        }

        public void Highlight()
        {
            if (_isHighlighted) return;
            _isHighlighted = true;
            foreach (Renderer r in _renderers)
                foreach (Material m in r.materials)
                    if (m.HasProperty("_Color")) m.color = highlightColor;
        }

        public void Unhighlight()
        {
            if (!_isHighlighted) return;
            _isHighlighted = false;
            for (int i = 0; i < _renderers.Length; i++)
            {
                var mats = _renderers[i].materials;
                for (int j = 0; j < mats.Length && j < _originalColors[i].Length; j++)
                    if (mats[j].HasProperty("_Color")) mats[j].color = _originalColors[i][j];
            }
        }
    }
}
