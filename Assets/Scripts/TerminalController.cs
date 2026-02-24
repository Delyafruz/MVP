using UnityEngine;
using TMPro;
using System.Text;
using AbaiLib;

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
    private TerminalEmulator emulator;
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

        // Инициализация эмулятора терминала
        emulator = new TerminalEmulator();
        
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
        // Вставка из буфера обмена (Ctrl+V)
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.V))
        {
            PasteFromClipboard();
            // не возвращаем — позволяем обрабатывать другие символы в этом кадре
        }
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

    // Вставить текст из системного буфера обмена
    void PasteFromClipboard()
    {
        string clip = GUIUtility.systemCopyBuffer;
        if (string.IsNullOrEmpty(clip))
            return;

        // Убираем возвраты каретки, заменяем переводы строк пробелом
        clip = clip.Replace("\r", "").Replace("\n", " ");

        currentLine.Append(clip);
        UpdateDisplay();

        // Проигрываем звук один раз (если назначен)
        PlayTypingSound();
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
        // Передаём обработку эмулятору (взято из перенесённого Program.cs)
        string output = emulator.HandleCommand(command);

        if (!string.IsNullOrEmpty(output))
        {
            // Разбиваем по строкам и добавляем в терминал
            var lines = output.Split('\n');
            foreach (var l in lines)
            {
                allText.AppendLine(l.TrimEnd('\r'));
            }
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