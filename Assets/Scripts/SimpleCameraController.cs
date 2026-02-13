using UnityEngine;

public class SimpleCameraController : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;
    
    private float rotationX = 0f;
    private float rotationY = 0f;
    private bool controlEnabled = true;
    
    void Start()
    {
        // Скрываем и блокируем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Подписываемся на изменение состояния игрока
        PlayerStateManager.Instance.OnStateChanged += OnStateChanged;
    }
    
    void Update()
    {
        if (!controlEnabled)
            return;
        
        // Получаем движение мыши
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        // Вращение по горизонтали (вокруг оси Y)
        rotationY += mouseX;
        
        // Вращение по вертикали (вокруг оси X) с ограничением
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxLookAngle, maxLookAngle);
        
        // Применяем вращение к камере
        transform.localRotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }
    
    // Обработчик изменения состояния
    void OnStateChanged(PlayerState newState)
    {
        if (newState == PlayerState.TerminalFocus)
        {
            // Отключаем управление при работе с терминалом
            controlEnabled = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (newState == PlayerState.Roaming)
        {
            // Включаем управление
            controlEnabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    
    void OnDestroy()
    {
        // Отписываемся от события
        if (PlayerStateManager.Instance != null)
        {
            PlayerStateManager.Instance.OnStateChanged -= OnStateChanged;
        }
    }
}