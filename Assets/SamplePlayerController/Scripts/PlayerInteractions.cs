using UnityEngine;
using NaughtyAttributes;
using HInteractions;
using System;

namespace HPlayer
{
    public class PlayerInteractions : MonoBehaviour, IObjectHolder
    {
        [Header("Select")]
        [SerializeField, Required] private Transform playerCamera;
        [SerializeField] private float selectRange = 10f;
        [SerializeField] private LayerMask selectLayer;
        [field: SerializeField, ReadOnly] public Interactable SelectedObject { get; private set; } = null;

        [Header("Hold")]
        [SerializeField, Required] private Transform handTransform;
        [SerializeField, Min(1)] private float holdingForce = 0.5f;
        [SerializeField] private int heldObjectLayer;
        [SerializeField] [Range(0f, 90f)] private float heldClamXRotation = 45f;
        [field: SerializeField, ReadOnly] public Liftable HeldObject { get; private set; } = null;

        [field: Header("Input")]
        [field: SerializeField, ReadOnly] public bool Interacting { get; private set; } = false;

        public event Action OnSelect;
        public event Action OnDeselect;

        public event Action OnInteractionStart;
        public event Action OnInteractionEnd;

        private bool canInteract = true;  // Флаг разрешения взаимодействия

        private void OnEnable()
        {
            OnInteractionStart += ChangeHeldObject;

            PlayerController.OnPlayerEnterPortal += CheckHeldObjectOnTeleport;

            // Подписываемся на изменение состояния игрока
            if (PlayerStateManager.Instance != null)
            {
                PlayerStateManager.Instance.OnStateChanged += OnPlayerStateChanged;
                // Устанавливаем начальное состояние
                OnPlayerStateChanged(PlayerStateManager.Instance.CurrentState);
            }
        }
        private void OnDisable()
        {
            OnInteractionStart -= ChangeHeldObject;

            PlayerController.OnPlayerEnterPortal -= CheckHeldObjectOnTeleport;

            // Отписываемся от события
            if (PlayerStateManager.Instance != null)
            {
                PlayerStateManager.Instance.OnStateChanged -= OnPlayerStateChanged;
            }
        }

        // Обработчик изменения состояния игрока
        private void OnPlayerStateChanged(PlayerState newState)
        {
            switch (newState)
            {
                case PlayerState.TerminalFocus:
                    // Блокируем взаимодействие с объектами при работе с терминалом
                    canInteract = false;
                    // Если что-то держим - бросаем
                    if (HeldObject != null)
                        DropObject(HeldObject);
                    break;

                case PlayerState.Roaming:
                    // Разрешаем взаимодействие
                    canInteract = true;
                    break;
            }
        }

        private void Update()
        {
            UpdateInput();

            UpdateSelectedObject();

            if (HeldObject)
                UpdateHeldObjectPosition();
        }

        #region -input-

        private void UpdateInput()
        {
            // Блокируем ввод если взаимодействие запрещено
            if (!canInteract)
            {
                Interacting = false;
                return;
            }

            bool interacting = Input.GetMouseButton(0);
            if (interacting != Interacting)
            {
                Interacting = interacting;
                if (interacting)
                    OnInteractionStart?.Invoke();
                else
                    OnInteractionEnd?.Invoke();
            }
        }

        #endregion

        #region -selected object-

        private void UpdateSelectedObject()
        {
            // Если взаимодействие запрещено - снимаем выделение
            if (!canInteract)
            {
                if (SelectedObject != null)
                {
                    SelectedObject.Deselect();
                    OnDeselect?.Invoke();
                    SelectedObject = null;
                }
                return;
            }

            Interactable foundInteractable = null;
            if (Physics.SphereCast(playerCamera.position, 0.2f, playerCamera.forward, out RaycastHit hit, selectRange, selectLayer))
                foundInteractable = hit.collider.GetComponent<Interactable>();

            if (SelectedObject == foundInteractable)
                return;

            if (SelectedObject)
            {
                SelectedObject.Deselect();
                OnDeselect?.Invoke();
            }

            SelectedObject = foundInteractable;

            if (foundInteractable && foundInteractable.enabled)
            {

                foundInteractable.Select();
                OnSelect?.Invoke();
            }
        }

        #endregion

        #region -held object-

        private void UpdateHeldObjectPosition()
        {
            HeldObject.Rigidbody.velocity = (handTransform.position - HeldObject.transform.position) * holdingForce;

            Vector3 handRot = handTransform.rotation.eulerAngles;
            if (handRot.x > 180f)
                handRot.x -= 360f;
            handRot.x = Mathf.Clamp(handRot.x, -heldClamXRotation, heldClamXRotation);
            HeldObject.transform.rotation = Quaternion.Euler(handRot + HeldObject.LiftDirectionOffset);
        }
        private void ChangeHeldObject()
        {
            if (HeldObject)
                DropObject(HeldObject);
            else if (SelectedObject is Liftable liftable)
                PickUpObject(liftable);
        }
        private void PickUpObject(Liftable obj)
        {
            if (obj == null)
            {
                Debug.LogWarning($"{nameof(PlayerInteractions)}: Attempted to pick up null object!");
                return;
            }

            HeldObject = obj;
            obj.PickUp(this, heldObjectLayer);
        }
        private void DropObject(Liftable obj)
        {
            if (obj == null)
            {
                Debug.LogWarning($"{nameof(PlayerInteractions)}: Attempted to drop null object!");
                return;
            }

            HeldObject = null;
            obj.Drop();
        }

        private void CheckHeldObjectOnTeleport()
        {
            if (HeldObject != null)
                DropObject(HeldObject);
        }

        #endregion
    }
}