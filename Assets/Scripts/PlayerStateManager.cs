using UnityEngine;

// Перечисление состояний игрока
public enum PlayerState
{
    Roaming,        // Свободное перемещение
    TerminalFocus   // Работа за монитором
}

public class PlayerStateManager : MonoBehaviour
{
    // Singleton (единственный экземпляр в игре)
    public static PlayerStateManager Instance;
    
    // Текущее состояние игрока
    [SerializeField] private PlayerState currentState;
public PlayerState CurrentState 
{ 
    get => currentState;
    private set => currentState = value;
}
    
    // Event (событие) для оповещения других скриптов об изменении состояния
    public event System.Action<PlayerState> OnStateChanged;
    
    void Awake()
    {
        // Создаём Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Не уничтожать при загрузке новой сцены
        }
        else
        {
            Destroy(gameObject); // Если уже есть другой менеджер - удаляем этот
        }
        
        // Устанавливаем начальное состояние
        currentState = PlayerState.Roaming;
    }
    
    // Метод для изменения состояния
    public void SetState(PlayerState newState)
    {
        if (CurrentState == newState)
            return; // Если состояние не изменилось - ничего не делаем
        
        CurrentState = newState;
        
        // Оповещаем всех подписчиков о смене состояния
        OnStateChanged?.Invoke(newState);
        
        Debug.Log($"State changed to: {newState}");
    }
}