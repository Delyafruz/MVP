# Cable Physics System - Оригинальная версия

Полная система физики кабелей с взаимодействием для Unity (перенесена из проекта Cable-physics-main).

## Структура проекта

```
MVP/Assets/
├── NaughtyAttributes-2.1.4/     # Система расширенных атрибутов для инспектора
├── Interactions/                # Базовая система взаимодействий
│   ├── IObjectHolder.cs
│   ├── Interactable.cs
│   └── Liftable.cs
├── PhysicCalbes/               # Система кабелей
│   ├── Scripts/
│   │   ├── Connector.cs        # Логика соединения коннекторов
│   │   ├── PhysicCable.cs      # Физическая цепочка кабеля
│   │   └── PhysicCableCon.cs   # Взаимодействие с игроком
│   ├── Materials/              # Материалы кабелей
│   ├── Models/                 # 3D модели коннекторов
│   └── Prefabs/                # Готовые префабы кабелей
└── SamplePlayerController/     # Пример контроллера игрока
    ├── Scripts/
    │   ├── PlayerController.cs      # FPS контроллер
    │   ├── PlayerInteractions.cs    # Взаимодействие с объектами
    │   └── PlayerCursor.cs          # UI курсор
    └── Prefabs/                     # Префабы игрока
```

## Зависимости

### NaughtyAttributes
Пакет для улучшения Unity Inspector. Предоставляет атрибуты:
- `[Required]` - Обязательное поле (красное предупреждение если пусто)
- `[Button]` - Кнопка в инспекторе для вызова метода
- `[Min]` - Минимальное значение
- `[ShowIf]` - Условное отображение поля
- `[ReadOnly]` - Поле только для чтения
- `[OnValueChanged]` - Callback при изменении значения

**Уже скопирован в проект** из папки `NaughtyAttributes-2.1.4/`.

## Компоненты системы кабелей

### Connector (HPhysic namespace)

Управляет соединением двух коннекторов через FixedJoint.

**Настройки:**
- `Connection Type` - Male/Female (тип коннектора)
- `Connector Type` - HDMI, TypeC, LAN (тип кабеля)
- `Make Connection Kinematic` - Делать ли подключённый объект кинематическим
- `Hide Interactable When Is Connected` - Скрывать коллайдер при соединении
- `Connection Point` - Transform точки соединения (Required)
- `Connector Mesh` - GameObject меша коннектора

**Правила соединения:**
- Male соединяется только с Female
- Типы кабелей должны совпадать (HDMI с HDMI и т.д.)
- Оба коннектора должны быть свободны

**API:**
```csharp
bool CanConnect(Connector secondConnector)
void Connect(Connector secondConnector)
void Disconnect()
bool IsConnected
bool IsConnectedRight  // Типы совпадают
```

### PhysicCable (HPhysic namespace)

Создаёт и управляет физической цепочкой из SpringJoint.

**Параметры Look:**
- `Number Of Points` (1-100) - Количество сегментов
- `Space` (0.01-10) - Расстояние между точками
- `Size` (0.01-5) - Размер визуальных секций

**Параметры Behaviour:**
- `Spring Force` (1-1000) - Сила пружин
- `Brake Length Multiplier` (1-10) - Множитель максимальной длины
- `Min Brake Time` (0.1-10) - Время до автоотключения

**Требуемые объекты:**
- `Start` - GameObject начального коннектора (Required)
- `End` - GameObject конечного коннектора (Required)
- `Connector0` - Префаб визуального сегмента (Required)
- `Point0` - Префаб физической точки с Rigidbody + SpringJoint (Required)

**Кнопки редактора (NaughtyAttributes):**
- `Reset points` - Пересоздать все точки
- `Add point` - Добавить сегмент
- `Remove point` - Удалить сегмент

### PhysicCableCon (HPhysic namespace)

Наследует `Liftable` для интеграции с системой взаимодействий.

**Поведение:**
- PickUp - разрывает текущее соединение
- Drop - пытается автоподключиться к выделенному объекту
- Если типы не подходят - выравнивает позицию рядом

**Требует:** `Connector` на том же GameObject

## Система взаимодействий (HInteractions namespace)

### Interactable
Базовый класс для всех интерактивных объектов.

**Свойства:**
- `ShowPointerOnInterract` - Показывать ли курсор
- `IsSelected` - Выбран ли объект (ReadOnly)

### Liftable
Для объектов, которые можно поднять.

**Наследует:** `Interactable`
**Требует:** `Rigidbody`

**Свойства:**
- `IsLift` - Поднят ли объект
- `LiftDirectionOffset` - Смещение вращения при удержании

### IObjectHolder
Интерфейс для объектов, которые могут держать Liftable.

```csharp
Interactable SelectedObject { get; }
```

## Player контроллер (HPlayer namespace)

### PlayerController

First-person контроллер с физикой через CharacterController.

**Требует:** `CharacterController`

**Управление:**
- WASD - движение
- Мышь - камера
- Shift - бег
- Space - прыжок
- Ctrl - присесть (резерв)

**Параметры Camera:**
- `Player Camera` - Camera компонент (Required)
- `Mouse Sensitivity` (1-15) - Чувствительность мыши
- `Min/Max Pitch` - Ограничение вертикального угла

**Параметры Movement:**
- `Default Speed` / `Sprint Speed` - Скорости
- `Default Height` - Высота контроллера
- `Ground Mask` - Слой земли для проверки
- `Can Jump` / `Can Sprint` - Включить прыжки/бег

**События:**
```csharp
static Action OnPlayerEnterPortal
```

### PlayerInteractions

Реализует `IObjectHolder` для взаимодействия с миром.

**Требует:** Добавить на GameObject с `PlayerController`

**Параметры Select:**
- `Player Camera` - Transform камеры (Required)
- `Select Range` - Дистанция выбора (1-50)
- `Select Layer` - Слой интерактивных объектов

**Параметры Hold:**
- `Hand Transform` - Позиция удержания (Required)
- `Holding Force` (1-100) - Сила притяжения к руке
- `Held Object Layer` - Слой удерживаемых объектов

**Управление:**
- ЛКМ - Поднять/Бросить

**События:**
```csharp
event Action OnSelect;
event Action OnDeselect;
event Action OnInteractionStart;
event Action OnInteractionEnd;
```

### PlayerCursor

UI курсор при наведении на интерактивные объекты.

**Требует:** `PlayerInteractions`

**Параметры:**
- `Cursor Canvas` - UI GameObject с курсором
- `Min Show Distance` - Минимальная дистанция показа

## Быстрый старт

### 1. Настройка Player

1. Создать GameObject "Player"
2. Добавить компоненты:
   - `CharacterController`
   - `PlayerController`
   - `PlayerInteractions`
   - `PlayerCursor` (опционально)

3. Создать дочерние объекты:
   - `Camera` (позиция: 0, 1.6, 0) с компонентом `Camera`
   - `HandTransform` (позиция: 0.3, 1.4, 0.5)

4. Настроить `PlayerController`:
   - Player Camera → Camera
   - Ground Mask → слой земли

5. Настроить `PlayerInteractions`:
   - Player Camera → Camera Transform
   - Hand Transform → HandTransform
   - Select Layer → слой интерактивных объектов (например, "Interactable")
   - Held Object Layer → отдельный слой (например, "HeldObject")

6. Настроить слои в Project Settings → Tags and Layers

### 2. Настройка кабеля

Использовать готовые префабы из `PhysicCalbes/Prefabs/`:
- `PhysicCableMM.prefab` - Male-Male
- `PhysicCableMF.prefab` - Male-Female
- `PhysicCableFF.prefab` - Female-Female

Или создать свой:
1. Создать GameObject "Cable"
2. Добавить компонент `PhysicCable`
3. Создать два GameObject для концов (Start/End):
   - Добавить `Rigidbody`, `Connector`, `PhysicCableCon`
   - Настроить Connector Type и Connection Type
4. Создать префабы Point и Connector (или использовать из Base/)
5. Назначить все ссылки в инспекторе PhysicCable
6. Нажать кнопку "Reset points"

### 3. Тестирование

1. Убедиться, что коннекторы на слое Select Layer
2. Запустить сцену (Play Mode)
3. Навести на коннектор - должен загореться курсор
4. ЛКМ - поднять коннектор
5. Навести на другой коннектор того же типа
6. ЛКМ - бросить, должно автоподключиться

## Известные особенности

- Кнопки редактора (Reset/Add/Remove points) работают только в Edit Mode
- При растяжении кабеля свыше `brakeLength` он автоматически разъединяется
- Для корректной работы нужны отдельные Physics Layers для удерживаемых объектов
- Префабы могут потребовать переназначения ссылок после импорта

## Структура префабов

`PhysicCalbes/Prefabs/Base/` содержит базовые элементы:
- Точки кабеля (с Rigidbody + SpringJoint)
- Визуальные секции (Cylinders)
- Коннекторы Male/Female

## Материалы и модели

- **Materials/** - 4 материала (основной, коннектор, пины, цвет)
- **Models/** - FBX модели коннекторов:
  - `male.fbx` / `female.fbx` - базовые
  - `LanMale.fbx` / `LanFemale.fbx` - LAN коннекторы

## Поддержка

Система полностью функциональна и протестирована в исходном проекте Cable-physics-main.

Все зависимости (NaughtyAttributes) включены в копию проекта.
