using UnityEngine;
using NaughtyAttributes;

namespace HInteractions
{
    [DisallowMultipleComponent]
    public class Interactable : MonoBehaviour
    {
        [field: SerializeField] public bool ShowPointerOnInterract { get; private set; } = true;

        [field: SerializeField, ReadOnly] public bool IsSelected { get; private set; }

        [Header("Highlight")]
        [SerializeField] private bool enableHighlight = true;
        [SerializeField] private Color highlightColor = Color.yellow;

        // Renderers & original colors for restore
        private Renderer[] cachedRenderers;
        private Color[][] originalColors;

        protected virtual void Awake()
        {
            CacheRenderers();
            Deselect();
        }

        private void CacheRenderers()
        {
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
            originalColors = new Color[cachedRenderers.Length][];

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                var mats = cachedRenderers[i].materials;
                originalColors[i] = new Color[mats.Length];
                for (int j = 0; j < mats.Length; j++)
                {
                    if (mats[j].HasProperty("_Color"))
                        originalColors[i][j] = mats[j].color;
                    else
                        originalColors[i][j] = Color.white;
                }
            }
        }

        public virtual void Select()
        {
            IsSelected = true;
            if (!enableHighlight || cachedRenderers == null)
                return;

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                var mats = cachedRenderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    if (mats[j].HasProperty("_Color"))
                        mats[j].color = highlightColor;
                }
            }
        }

        public virtual void Deselect()
        {
            IsSelected = false;
            if (cachedRenderers == null || originalColors == null)
                return;

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                var mats = cachedRenderers[i].materials;
                for (int j = 0; j < mats.Length && j < originalColors[i].Length; j++)
                {
                    if (mats[j].HasProperty("_Color"))
                        mats[j].color = originalColors[i][j];
                }
            }
        }

        /// <summary>
        /// Вызывается когда игрок кликает на объект, но он не является Liftable.
        /// Переопределите в наследниках для кастомного поведения (например, прибивание кабеля).
        /// </summary>
        public virtual void Interact() { }
    }
}