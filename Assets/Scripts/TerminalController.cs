using UnityEngine;
using TMPro;
using System.Text;

public class TerminalController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Текстовое поле терминала (TextMeshPro)")]
    public TextMeshProUGUI terminalText;
    
    [Header("Settings")]
    [Tooltip("Максимальное количество строк на экране")]
    public int maxLines = 20;
    
    [Tooltip("Скорость мигания курсора (в секундах)")]
    public float caretBlinkSpeed = 0.5f;
    
    [Tooltip("Символ курсора")]
    public string caretSymbol = "_";
    
    [Tooltip("Символ приглашения командной строки")]
    public string promptSymbol = "> ";
    
    [Header("Audio (Optional)")]
    [Tooltip("Звук печати (опционально - можно оставить пустым)")]
    public AudioClip typingSoundClip;
    
    [Tooltip("Громкость звука печати (0.0 - 1.0)")]
    [Range(0f, 1f)]
    public float typingVolume = 0.5f;
    
    // Приватные переменные
    private StringBuilder currentLine;      // Текущая вводимая строка
    private StringBuilder allText;          // Весь текст терминала
    private bool showCaret = true;          // Показывать ли курсор
    private float caretTimer = 0f;          // Таймер для мигания
    private bool isActive = false;          // Активен ли терминал
    private AudioSource audioSource;        // Компонент для воспроизведения звука
    
    void Start()
    {
        currentLine = new StringBuilder();
        allText = new StringBuilder();
        
        // Приветственное сообщение
        allText.AppendLine("SYSTEM READY");
        allText.AppendLine("Type 'help' for available commands");
        allText.AppendLine();
        allText.Append(promptSymbol);
        
        UpdateDisplay();
        
        // Настройка аудио
        SetupAudio();
        
        // Подписываемся на изменение состояния игрока
        PlayerStateManager.Instance.OnStateChanged += OnStateChanged;
    }
    
    // Настройка AudioSource
    void SetupAudio()
    {
        // Получаем существующий AudioSource или создаём новый
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Настройки AudioSource (ГАРАНТИРОВАННО отключаем автоплей)
        audioSource.clip = null;              // ВАЖНО: не назначаем клип напрямую
        audioSource.playOnAwake = false;      // НЕ воспроизводить при старте
        audioSource.loop = false;              // НЕ зациклено
        audioSource.volume = typingVolume;
        audioSource.spatialBlend = 0f;         // 2D звук
        audioSource.Stop();                    // Останавливаем если что-то играет
        
        Debug.Log("Audio setup complete. PlayOnAwake: " + audioSource.playOnAwake);
    }
    
    void Update()
    {
        if (!isActive)
            return;
        
        // Обновляем мигание курсора
        UpdateCaret();
        
        // Обрабатываем ввод
        HandleInput();
    }
    
    // Обработчик изменения состояния
    void OnStateChanged(PlayerState newState)
    {
        isActive = (newState == PlayerState.TerminalFocus);
        
        if (isActive)
        {
            // Сбрасываем таймер курсора
            caretTimer = 0f;
            showCaret = true;
        }
    }
    
    // Мигание курсора
    void UpdateCaret()
    {
        caretTimer += Time.deltaTime;
        
        if (caretTimer >= caretBlinkSpeed)
        {
            showCaret = !showCaret;
            caretTimer = 0f;
            UpdateDisplay();
        }
    }
    
    // Обработка ввода с клавиатуры
    void HandleInput()
    {
        // Input.inputString содержит все символы, введённые за этот кадр
        foreach (char c in Input.inputString)
        {
            if (c == '\b') // Backspace
            {
                HandleBackspace();
            }
            else if (c == '\n' || c == '\r') // Enter
            {
                HandleEnter();
            }
            else if (c >= 32 && c <= 126) // Печатаемые ASCII символы
            {
                HandleCharacter(c);
            }
        }
    }
    
    // Обработка обычного символа
    void HandleCharacter(char c)
    {
        currentLine.Append(c);
        UpdateDisplay();
        
        // Воспроизводим звук печати (если назначен)
        PlayTypingSound();
    }
    
    // Воспроизведение звука печати
    void PlayTypingSound()
    {
        if (typingSoundClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(typingSoundClip, typingVolume);
        }
    }
    
    // Обработка Backspace
    void HandleBackspace()
    {
        if (currentLine.Length > 0)
        {
            currentLine.Length--; // Удаляем последний символ
            UpdateDisplay();
            
            // Можно добавить другой звук для Backspace (опционально)
            PlayTypingSound();
        }
    }
    
    // Обработка Enter
    void HandleEnter()
    {
        string command = currentLine.ToString().Trim();
        
        // Добавляем введённую команду в историю
        allText.AppendLine(currentLine.ToString());
        
        // Обрабатываем команду
        ProcessCommand(command);
        
        // Новая строка с приглашением
        allText.Append(promptSymbol);
        
        // Очищаем текущую строку
        currentLine.Clear();
        
        // Ограничиваем количество строк
        LimitLines();
        
        UpdateDisplay();
    }
    
    // Обработка команд
    void ProcessCommand(string command)
    {
        if (string.IsNullOrEmpty(command))
        {
            // Пустая команда - ничего не делаем
            return;
        }
        
        string lowerCommand = command.ToLower();
        
        switch (lowerCommand)
        {
            case "help":
                allText.AppendLine("Available commands:");
                allText.AppendLine("  help   - Show this help message");
                allText.AppendLine("  clear  - Clear the terminal");
                allText.AppendLine("  hello  - Greet the user");
                allText.AppendLine("  time   - Show current time");
                allText.AppendLine("  exit   - Close terminal (use ESC)");
                break;
                
            case "clear":
                allText.Clear();
                break;
                
            case "hello":
                allText.AppendLine("Hello, User! Welcome to the terminal.");
                break;
                
            case "time":
                allText.AppendLine("Current time: " + System.DateTime.Now.ToString("HH:mm:ss"));
                break;
                
            case "exit":
                allText.AppendLine("Use ESC key to exit terminal.");
                break;
                
            default:
                allText.AppendLine($"Unknown command: {command}");
                allText.AppendLine("Type 'help' for available commands.");
                break;
        }
    }
    
    // Ограничение количества строк
    void LimitLines()
    {
        string[] lines = allText.ToString().Split('\n');
        
        if (lines.Length > maxLines)
        {
            allText.Clear();
            
            // Оставляем только последние maxLines строк
            int startIndex = lines.Length - maxLines;
            for (int i = startIndex; i < lines.Length; i++)
            {
                allText.AppendLine(lines[i]);
            }
        }
    }
    
    // Обновление отображения текста
    void UpdateDisplay()
    {
        if (terminalText == null)
            return;
        
        // Собираем финальный текст
        string displayText = allText.ToString() + currentLine.ToString();
        
        // Добавляем курсор если нужно
        if (showCaret && isActive)
        {
            displayText += caretSymbol;
        }
        
        terminalText.text = displayText;
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