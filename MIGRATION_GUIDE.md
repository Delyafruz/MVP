# Быстрая миграция на новую систему Player

## 🔄 Если у вас уже есть старый Player с PlayerMovement

### Шаг 1: Создайте дочерние объекты

На объекте Player создайте:
```
Player/
├── Camera (Transform + Camera component, позиция: 0, 1.6, 0)
└── HandTransform (пустой Transform, позиция: 0.3, 1.4, 0.5)
```

### Шаг 2: Замените компоненты

**Удалите (или отключите):**
- ~~PlayerMovement~~ → оставьте для обратной совместимости или удалите

**Добавьте:**
- PlayerController (из SamplePlayerController/)
- PlayerInteractions (из SamplePlayerController/)
- PlayerCursor (опционально)

### Шаг 3: Настройте ссылки

**PlayerController:**
- Player Camera → `Player/Camera`
- Ground Mask → ваш слой земли

**PlayerInteractions:**
- Player Camera → `Player/Camera` Transform
- Hand Transform → `Player/HandTransform`
- Select Layer → `Interactable` (создайте в Project Settings)
- Held Object Layer → `HeldObject` (создайте в Project Settings)

### Шаг 4: Готово!

Теперь у вас:
✅ FPS управление с мышью  
✅ Взаимодействие с объектами (кабели, etc)  
✅ Автоматическая интеграция с терминалами через PlayerStateManager  

## 🆕 Если создаёте нового Player с нуля

Следуйте инструкции в [PLAYER_SYSTEM_INTEGRATION.md](PLAYER_SYSTEM_INTEGRATION.md), раздел "Настройка → Вариант 1: PlayerController".

## ⚙️ Настройка слоёв (Layers)

Project Settings → Tags and Layers:

| Layer | Назначение |
|-------|-----------|
| `Interactable` | Объекты, с которыми можно взаимодействовать |
| `HeldObject` | Удерживаемые объекты (чтобы не сталкивались с игроком) |

Назначьте на префабах кабелей слой `Interactable`.

## 📖 Полная документация

- [PLAYER_SYSTEM_INTEGRATION.md](PLAYER_SYSTEM_INTEGRATION.md) — детали объединения систем
- [CABLE_SYSTEM_SETUP.md](CABLE_SYSTEM_SETUP.md) — настройка системы кабелей
- [Assets/README_CableSystem.md](Assets/README_CableSystem.md) — документация Cable Physics System
