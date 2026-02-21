using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using HInteractions;

namespace HPlayer
{
    [RequireComponent(typeof(PlayerInteractions))]
    public class PlayerCursor : MonoBehaviour
    {
        [SerializeField] private GameObject cursorCanvas;
        [SerializeField, Min(0)] private float maxShowDistance = 50f; // прячем курсор если объект слишком далеко
        
        [Header("Cursor Appearance")]
        [SerializeField] private float cursorSize = 60f;
        [SerializeField] private float cursorThickness = 3f;
        [SerializeField] private Color cursorColor = Color.white;

        private PlayerInteractions playerInteractions;
        private IEnumerator cursorUpdater;
        private RectTransform cursorRectTransform;
        private Image cursorImage;

        private void OnEnable()
        {
            playerInteractions = GetComponent<PlayerInteractions>();
            if (playerInteractions == null)
                return;

            playerInteractions.OnSelect += ActiveCursor;
            
            // Настраиваем внешний вид курсора
            SetupCursorAppearance();
        }
        private void OnDisable()
        {
            if (playerInteractions == null)
                return;

            playerInteractions.OnSelect -= ActiveCursor;

            DesactiveCursor();
        }

        private void SetupCursorAppearance()
        {
            if (cursorCanvas == null)
                return;

            // Находим Image компонент курсора
            cursorImage = cursorCanvas.GetComponentInChildren<Image>();
            if (cursorImage != null)
            {
                cursorRectTransform = cursorImage.GetComponent<RectTransform>();
                
                // Устанавливаем размер
                if (cursorRectTransform != null)
                {
                    cursorRectTransform.sizeDelta = new Vector2(cursorSize, cursorSize);
                }
                
                // Устанавливаем цвет
                cursorImage.color = cursorColor;
            }
        }

        private void ActiveCursor()
        {
            if (playerInteractions == null)
                return;

            if (playerInteractions.SelectedObject is Interactable interactable && interactable.ShowPointerOnInterract)
            {
                cursorUpdater = UpdateCursor();
                StartCoroutine(cursorUpdater);
            }
        }
        private void DesactiveCursor()
        {
            cursorCanvas?.SetActive(false);

            if (cursorUpdater != null)
            {
                StopCoroutine(cursorUpdater);
                cursorUpdater = null;
            }
        }


        private IEnumerator UpdateCursor()
        {
            if (cursorCanvas == null)
                yield break;

            while (playerInteractions.SelectedObject != null)
            {
                float distance = Vector3.Distance(playerInteractions.SelectedObject.transform.position, transform.position);
                cursorCanvas.SetActive(distance <= maxShowDistance);

                yield return new WaitForSeconds(0.2f);
            }

            cursorUpdater = null;
            DesactiveCursor();
        }
    }
}