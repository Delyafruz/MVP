using UnityEngine;

// ============================================================================
// УСТАРЕЛО (DEPRECATED)
// ============================================================================
// Этот скрипт сохранён для обратной совместимости.
// 
// Рекомендуется использовать PlayerController из папки SamplePlayerController,
// который включает:
// - Управление FPS камерой мышью
// - Плавное ускорение движения
// - Интеграцию с PlayerStateManager
// - Поддержку взаимодействия с кабелями (через PlayerInteractions)
// 
// Этот скрипт можно оставить для простых проектов или удалить,
// если используется новый PlayerController.
// ============================================================================

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Скорость ходьбы")]
    public float walkSpeed = 5f;
    
    [Tooltip("Скорость бега (при зажатом Shift)")]
    public float runSpeed = 8f;
    
    [Tooltip("Сила гравитации")]
    public float gravity = -9.81f;
    
    [Tooltip("Высота прыжка")]
    public float jumpHeight = 1.5f;
    
    [Header("Ground Check")]
    [Tooltip("Трансформ для проверки земли (создай пустой объект под ногами)")]
    public Transform groundCheck;
    
    [Tooltip("Радиус проверки земли")]
    public float groundDistance = 0.4f;
    
    [Tooltip("Слой земли")]
    public LayerMask groundMask;
    
    // Приватные переменные
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool canMove = true;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Подписываемся на изменение состояния
        PlayerStateManager.Instance.OnStateChanged += OnStateChanged;
        
        // Проверка наличия groundCheck
        if (groundCheck == null)
        {
            Debug.LogWarning("GroundCheck не назначен! Создайте пустой объект под персонажем.");
        }
    }
    
    void Update()
    {
        // Если управление заблокировано - не двигаемся
        if (!canMove)
        {
            return;
        }
        
        // Проверка на земле ли персонаж
        CheckGround();
        
        // Движение WASD
        HandleMovement();
        
        // Прыжок
        HandleJump();
        
        // Применение гравитации
        ApplyGravity();
    }
    
    // Проверка земли
    void CheckGround()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }
        else
        {
            // Если groundCheck не назначен, используем встроенную проверку
            isGrounded = controller.isGrounded;
        }
        
        // Сброс вертикальной скорости если на земле
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Небольшое отрицательное значение для "прилипания" к земле
        }
    }
    
    // Обработка движения WASD
    void HandleMovement()
    {
        // Получаем ввод
        float x = Input.GetAxis("Horizontal"); // A/D или стрелки влево/вправо
        float z = Input.GetAxis("Vertical");   // W/S или стрелки вверх/вниз
        
        // Вычисляем направление движения относительно камеры
        Vector3 move = transform.right * x + transform.forward * z;
        
        // Определяем текущую скорость (бег или ходьба)
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        
        // Применяем движение
        controller.Move(move * currentSpeed * Time.deltaTime);
    }
    
    // Обработка прыжка
    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    
    // Применение гравитации
    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    // Обработчик изменения состояния
    void OnStateChanged(PlayerState newState)
    {
        if (newState == PlayerState.TerminalFocus)
        {
            // Блокируем движение при работе с терминалом
            canMove = false;
        }
        else if (newState == PlayerState.Roaming)
        {
            // Разрешаем движение
            canMove = true;
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
    
    // Отрисовка Gizmo для ground check (для удобства в редакторе)
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}