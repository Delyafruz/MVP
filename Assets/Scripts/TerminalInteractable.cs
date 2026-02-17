using UnityEngine;
using System.Collections;

public class TerminalInteractable : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("Основная камера игрока")]
    public Camera mainCamera;
    
    [Tooltip("Transform игрока (родительский объект камеры)")]
    public Transform playerTransform;
    
    [Tooltip("Позиция, куда камера переместится при работе с терминалом")]
    public Transform terminalCameraPosition;
    
    [Tooltip("Скорость перемещения камеры (1 = медленно, 5 = быстро)")]
    public float transitionSpeed = 2f;
    
    [Header("Interaction Settings")]
    // Приватные переменные
    [Header("UI")]
    [Tooltip("Текстовая подсказка для взаимодействия")]
    public GameObject interactionPrompt;
    [Tooltip("Расстояние, с которого можно взаимодействовать")]
    public float interactionDistance = 3f;
    
    [Tooltip("Клавиша для входа в терминал")]
    public KeyCode interactKey = KeyCode.E;
    
    [Tooltip("Клавиша для выхода из терминала")]
    public KeyCode exitKey = KeyCode.Escape;
    
    // Приватные переменные
    private Vector3 originalCameraPos;
    private Quaternion originalCameraRot;
    private Vector3 originalPlayerPos;
    private Quaternion originalPlayerRot;
    private bool isPlayerLooking = false;
    private bool isTransitioning = false;
    
    void Start()
    {
        // Получаем камеру
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Получаем игрока (родительский объект камеры)
        if (playerTransform == null && mainCamera != null)
        {
            // Предполагаем, что камера - дочерний объект игрока
            playerTransform = mainCamera.transform.parent;
            if (playerTransform == null)
            {
                Debug.LogWarning("Player Transform не найден! Назначьте вручную.");
            }
        }
        
        // Проверка на ошибки
        if (terminalCameraPosition == null)
        {
            Debug.LogError("TerminalCameraPosition не назначен! Назначьте пустой объект перед монитором.");
        }
    }
    
    void Update()
    {
        // Проверяем только если не в процессе перехода
        if (!isTransitioning)
        {
            CheckPlayerLook();
            HandleInput();
        }
    }
    
    // Проверяем, смотрит ли игрок на монитор
   // Проверяем, смотрит ли игрок на монитор
    void CheckPlayerLook()
    {
        // Показываем подсказку только в режиме Roaming
        if (PlayerStateManager.Instance.CurrentState != PlayerState.Roaming)
        {
            if (interactionPrompt != null)
                interactionPrompt.SetActive(false);
            isPlayerLooking = false;
            return;
        }
        
        // Луч из центра экрана
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        // Пускаем луч
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // Проверяем, попал ли луч в монитор
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                isPlayerLooking = true;
                
                // Показываем подсказку
                if (interactionPrompt != null)
                    interactionPrompt.SetActive(true);
                return;
            }
        }
        
        isPlayerLooking = false;
        
        // Скрываем подсказку
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
    // Обработка нажатий клавиш
    void HandleInput()
    {
        // Вход в терминал (только если игрок смотрит И находится в режиме Roaming)
        if (isPlayerLooking && 
            Input.GetKeyDown(interactKey) && 
            PlayerStateManager.Instance.CurrentState == PlayerState.Roaming)
        {
            StartCoroutine(EnterTerminal());
        }
        
        // Выход из терминала
        if (Input.GetKeyDown(exitKey) && 
            PlayerStateManager.Instance.CurrentState == PlayerState.TerminalFocus)
        {
            StartCoroutine(ExitTerminal());
        }
    }
    
    // Корутина для плавного входа в терминал
    IEnumerator EnterTerminal()
    {
        isTransitioning = true;
        
        // ВАЖНО: Сохраняем текущую позицию ПЕРЕД переходом
        if (playerTransform != null)
        {
            originalPlayerPos = playerTransform.position;
            originalPlayerRot = playerTransform.rotation;
        }
        originalCameraPos = mainCamera.transform.position;
        originalCameraRot = mainCamera.transform.rotation;
        
        // Меняем состояние игрока (это заблокирует движение)
        PlayerStateManager.Instance.SetState(PlayerState.TerminalFocus);
        
        float elapsed = 0f;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        
        // Плавное перемещение камеры
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * transitionSpeed;
            
            // Lerp = линейная интерполяция (плавный переход)
            mainCamera.transform.position = Vector3.Lerp(
                startPos, 
                terminalCameraPosition.position, 
                elapsed
            );
            
            mainCamera.transform.rotation = Quaternion.Lerp(
                startRot, 
                terminalCameraPosition.rotation, 
                elapsed
            );
            
            yield return null; // Ждём следующий кадр
        }
        
        // Точно устанавливаем финальную позицию
        mainCamera.transform.position = terminalCameraPosition.position;
        mainCamera.transform.rotation = terminalCameraPosition.rotation;
        
        isTransitioning = false;
    }
    
    // Корутина для выхода из терминала
    IEnumerator ExitTerminal()
    {
        isTransitioning = true;
        
        // Меняем состояние обратно (это разблокирует движение)
        PlayerStateManager.Instance.SetState(PlayerState.Roaming);
        
        float elapsed = 0f;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        
        // Плавное возвращение камеры
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * transitionSpeed;
            
            mainCamera.transform.position = Vector3.Lerp(
                startPos, 
                originalCameraPos, 
                elapsed
            );
            
            mainCamera.transform.rotation = Quaternion.Lerp(
                startRot, 
                originalCameraRot, 
                elapsed
            );
            
            yield return null;
        }
        
        // Точно устанавливаем исходную позицию камеры
        mainCamera.transform.position = originalCameraPos;
        mainCamera.transform.rotation = originalCameraRot;
        
        // Также восстанавливаем позицию игрока (если сохранили)
        if (playerTransform != null)
        {
            playerTransform.position = originalPlayerPos;
            playerTransform.rotation = originalPlayerRot;
        }
        
        isTransitioning = false;
    }
    
    // Отрисовка Gizmo в редакторе (для удобства настройки)
    void OnDrawGizmosSelected()
    {
        if (mainCamera != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * interactionDistance);
        }
    }
}