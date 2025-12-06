# Система размещения арены для VR Tower Defense

## Обзор

Для вашей игры предлагается **два подхода** размещения поля битвы:

### Вариант 1: Автоматическое размещение (Рекомендуется)
**Скрипт:** `ArenaPlacement.cs`

**Как работает:**
- Использует AR Foundation Scene Understanding (Meta Quest 3)
- Автоматически сканирует комнату и находит столы
- Фильтрует столы по высоте (уровень пояса ~90см)
- Игрок видит зеленый индикатор на ближайшем подходящем столе
- Нажатие Trigger подтверждает размещение
- Арена закрепляется через AR Anchor (остается на месте)

**Преимущества:**
- ✅ Наиболее удобный UX
- ✅ Точное позиционирование на реальной мебели
- ✅ Стабильное размещение благодаря Spatial Anchors
- ✅ Автоматический выбор оптимальной высоты

### Вариант 2: Ручное размещение (Fallback)
**Скрипт:** `ManualArenaPlacement.cs`

**Как работает:**
- Игрок наводит контроллер на поверхность
- Видит превью арены в точке прицеливания
- Цвет индикатора: зеленый (можно) / красный (нельзя)
- Нажатие подтверждает размещение

**Преимущества:**
- ✅ Работает без Scene Understanding
- ✅ Больше свободы выбора места
- ✅ Проще для отладки

---

## Настройка в Unity

### Шаг 1: Настройка сцены

1. **Добавьте AR Session Origin:**
   - GameObject → XR → AR Session Origin
   - GameObject → XR → AR Session

2. **Добавьте необходимые AR компоненты:**
   ```
   AR Session Origin
   ├── AR Camera
   ├── AR Bounding Box Manager  (для автоматического режима)
   ├── AR Anchor Manager
   └── AR Plane Manager (опционально)
   ```

3. **Настройте AR Bounding Box Manager:**
   - Inspector → Bounding Box Prefab: создайте префаб с компонентом `ARBoundingBoxColorizer`
   - Убедитесь, что Scene Understanding включен в OVR Manager

### Шаг 2: Настройка автоматического размещения

1. **Создайте GameObject для системы размещения:**
   ```
   GameObject → Create Empty → "ArenaPlacementSystem"
   ```

2. **Добавьте компонент ArenaPlacement:**
   - Add Component → ArenaPlacement

3. **Настройка параметров в Inspector:**

   **Arena Settings:**
   - `Arena Prefab`: префаб вашей арены (поле битвы)
   - `Preferred Height`: 0.9 (метры, уровень пояса)
   - `Height Tolerance`: 0.2 (допуск ±20см)
   - `Min Arena Size`: (0.8, 0.1, 1.2) - минимальный размер стола

   **AR Components:**
   - `Bounding Box Manager`: перетащите AR Bounding Box Manager
   - `Anchor Manager`: перетащите AR Anchor Manager

   **Visual Feedback:**
   - `Placement Indicator Prefab`: префаб индикатора (можно оставить пустым - создастся автоматически)
   - `Valid Placement Material`: зеленый полупрозрачный материал
   - `Invalid Placement Material`: красный полупрозрачный материал

   **XR Input:**
   - `Confirm Placement Action`: XRI Right Hand/Trigger Press
   - `Player Camera`: XR Origin/Camera Offset/Main Camera

### Шаг 3: Настройка Input Actions

1. **Откройте XR Interaction Manager Input Actions:**
   - Assets → XRI Default Input Actions (или ваш Input Actions asset)

2. **Добавьте действие для размещения:**
   ```
   Action Map: XRI RightHand Interaction
   Action: Confirm Placement
   Binding: <XRController>{RightHand}/triggerPressed
   Action Type: Button
   ```

3. **В ArenaPlacement привяжите:**
   - Confirm Placement Action → XRI RightHand Interaction/Confirm Placement

### Шаг 4: Настройка Quest 3 Permissions

В **Assets/Plugins/Android/AndroidManifest.xml** добавьте:

```xml
<uses-permission android:name="com.oculus.permission.USE_SCENE" />
<uses-feature android:name="com.oculus.feature.PASSTHROUGH" android:required="true" />
```

В **XR Plug-in Management → Oculus:**
- ✅ Enable Scene Understanding
- ✅ Enable Passthrough

---

## Использование в коде

### Автоматическое размещение

```csharp
// Получить ссылку на систему размещения
ArenaPlacement placement = FindObjectOfType<ArenaPlacement>();

// Проверить, размещена ли арена
if (placement.IsPlaced)
{
    Vector3 arenaPos = placement.GetArenaPosition();
    Debug.Log($"Arena at {arenaPos}");
}

// Сбросить размещение (для отладки)
placement.ResetPlacement();
```

### Ручное размещение

```csharp
ManualArenaPlacement manual = FindObjectOfType<ManualArenaPlacement>();

if (manual.IsPlaced)
{
    // Арена размещена
}

manual.ResetPlacement(); // Переразместить
```

---

## Интеграция с NavMesh

**ВАЖНО:** Как указано в Code Review, ваш `NavMeshBuildOnline.cs` вызывает `BuildNavMesh()` в Update - это критическая проблема!

### Правильная интеграция:

1. **Измените NavMeshBuildOnline.cs:**

```csharp
public class NavMeshBuildOnline : MonoBehaviour
{
    [SerializeField] private NavMeshSurface navMeshSurface;

    // ❌ УДАЛИТЕ Update метод полностью!
    // void Update() { navMeshhSur.BuildNavMesh(); }

    // ✅ Добавьте публичный метод
    public void BuildNavMeshOnce()
    {
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("[NavMesh] Built successfully");
        }
    }
}
```

2. **В ArenaPlacement.cs (строка 344-349) раскомментируйте:**

```csharp
private void OnArenaPlaced()
{
    var navMeshBuilder = FindObjectOfType<NavMeshBuildOnline>();
    if (navMeshBuilder != null)
    {
        navMeshBuilder.BuildNavMeshOnce(); // ✅ Вызов ОДИН РАЗ после размещения
    }
}
```

---

## Альтернативные подходы

### Подход 3: Фиксированная позиция перед игроком

Если Scene Understanding недоступен, самый простой вариант:

```csharp
public class SimpleArenaPlacement : MonoBehaviour
{
    [SerializeField] private GameObject arenaPrefab;
    [SerializeField] private float distanceFromPlayer = 1.5f;
    [SerializeField] private float heightOffset = -0.3f; // Ниже уровня глаз

    void Start()
    {
        Transform camera = Camera.main.transform;

        // Размещаем арену прямо перед игроком
        Vector3 position = camera.position + camera.forward * distanceFromPlayer;
        position.y = camera.position.y + heightOffset;

        Instantiate(arenaPrefab, position, Quaternion.identity);
    }
}
```

**Минусы:** Не привязано к реальному столу, может "висеть в воздухе"

### Подход 4: Hybrid - Auto + Manual Fallback

Рекомендую объединить оба подхода:

```csharp
void Start()
{
    // Пытаемся автоматически найти стол
    StartCoroutine(TryAutoPlacement());
}

IEnumerator TryAutoPlacement()
{
    yield return new WaitForSeconds(5f); // Даем время на сканирование

    if (availableTables.Count == 0)
    {
        Debug.Log("No tables found, switching to manual mode");
        EnableManualPlacement();
    }
}
```

---

## Рекомендации по UX

### Визуальная обратная связь

1. **При поиске столов:**
   - Показывайте текст: "Scanning room... Look around to find tables"
   - Индикатор прогресса сканирования

2. **Когда стол найден:**
   - Зеленый контур вокруг стола
   - Текст: "Table found! Press Trigger to place arena"
   - Звуковой сигнал

3. **После размещения:**
   - Эффект частиц
   - Анимация появления арены
   - Звук подтверждения

### Пример UI кода:

```csharp
[SerializeField] private TextMeshProUGUI instructionText;

void UpdateInstructions()
{
    if (isArenaPlaced)
        instructionText.text = "";
    else if (selectedTable != null)
        instructionText.text = "Press Trigger to confirm";
    else
        instructionText.text = "Scanning for tables...";
}
```

---

## Тестирование

### В Unity Editor

1. **Используйте Device Simulator:**
   - Window → Device Simulator
   - Симулирует Meta Quest 3 input

2. **Mock режим:**
   ```csharp
   #if UNITY_EDITOR
   // Автоматически размещаем для тестирования
   void Start()
   {
       if (Application.isEditor)
       {
           PlaceArenaAtPosition(new Vector3(0, 0.9f, 1.5f));
       }
   }
   #endif
   ```

### На устройстве (Quest 3)

1. **Включите Debug mode:**
   - Показывает Gizmos для доступных столов
   - Логи в консоль

2. **Проверьте permissions:**
   - Settings → Apps → [Your App] → Permissions → Scene

3. **Сканирование комнаты:**
   - Медленно поворачивайтесь, глядя на стол
   - Scene Understanding требует времени на обработку

---

## Troubleshooting

### Проблема: Столы не обнаруживаются

**Решения:**
1. Убедитесь, что Scene Understanding включен в Oculus settings
2. Проверьте AndroidManifest permissions
3. Попробуйте пересоздать Scene Model: Settings → Guardian → Clear Guardian History
4. Улучшите освещение в комнате
5. Убедитесь, что стол достаточно большой (min 80x120см)

### Проблема: Арена "плывет" или смещается

**Решения:**
1. Проверьте, что AR Anchor создается корректно
2. Убедитесь, что arena привязана к anchor через `SetParent`
3. Используйте Spatial Anchors вместо простых AR Anchors
4. Убедитесь, что Quest Tracking качественный (хорошее освещение)

### Проблема: Высота арены неправильная

**Решения:**
1. Отрегулируйте `preferredHeight` (стандарт: 0.9м для столов)
2. Увеличьте `heightTolerance` для большей гибкости
3. Проверьте, правильно ли Quest определил пол (Guardian setup)

---

## Оптимизация производительности

### Рекомендации:

1. **Ограничьте частоту поиска:**
   ```csharp
   private const float TABLE_SEARCH_INTERVAL = 0.5f; // ✅ Уже реализовано
   ```

2. **Кешируйте компоненты:**
   ```csharp
   // ✅ Все компоненты кешируются в Awake()
   ```

3. **Отключайте Update после размещения:**
   ```csharp
   if (isArenaPlaced)
   {
       enabled = false; // Отключаем скрипт полностью
   }
   ```

4. **Используйте Object Pooling для индикаторов:**
   - Вместо создания/уничтожения - переиспользуйте

---

## Дополнительные фичи (опционально)

### 1. Масштабирование арены

```csharp
[SerializeField] private Vector2 scaleRange = new Vector2(0.5f, 1.5f);

void Update()
{
    if (isArenaPlaced && Input.GetAxis("Vertical") != 0)
    {
        float scale = arenaInstance.transform.localScale.x;
        scale += Input.GetAxis("Vertical") * Time.deltaTime * 0.5f;
        scale = Mathf.Clamp(scale, scaleRange.x, scaleRange.y);
        arenaInstance.transform.localScale = Vector3.one * scale;
    }
}
```

### 2. Поворот арены

```csharp
[SerializeField] private InputActionReference rotateAction;

void Update()
{
    if (isArenaPlaced && rotateAction.action.IsPressed())
    {
        float rotation = rotateAction.action.ReadValue<Vector2>().x;
        arenaInstance.transform.Rotate(Vector3.up, rotation * 90f * Time.deltaTime);
    }
}
```

### 3. Сохранение позиции

```csharp
// Сохранение позиции арены между сессиями
public void SavePlacement()
{
    PlayerPrefs.SetFloat("ArenaX", arenaInstance.transform.position.x);
    PlayerPrefs.SetFloat("ArenaY", arenaInstance.transform.position.y);
    PlayerPrefs.SetFloat("ArenaZ", arenaInstance.transform.position.z);
}

public void LoadPlacement()
{
    if (PlayerPrefs.HasKey("ArenaX"))
    {
        Vector3 savedPos = new Vector3(
            PlayerPrefs.GetFloat("ArenaX"),
            PlayerPrefs.GetFloat("ArenaY"),
            PlayerPrefs.GetFloat("ArenaZ")
        );
        PlaceArenaAtPosition(savedPos);
    }
}
```

---

## Заключение

**Рекомендация:**

Для **лучшего пользовательского опыта** используйте **автоматическое размещение** ([ArenaPlacement.cs](ArenaPlacement.cs)) с **ручным fallback** ([ManualArenaPlacement.cs](ManualArenaPlacement.cs)).

**Workflow:**
1. Игрок запускает игру
2. Система автоматически ищет столы (5 секунд)
3. Если стол найден → показываем зеленый индикатор, игрок подтверждает
4. Если стол не найден → переключаемся на ручной режим
5. После размещения → строим NavMesh **ОДИН РАЗ**

Это обеспечивает:
- ✅ Удобство (автоматика)
- ✅ Надежность (fallback)
- ✅ Стабильность (AR Anchors)
- ✅ Производительность (нет постоянного пересчета NavMesh)

---

**Удачи в разработке!** 🚀

Если возникнут вопросы - обращайтесь.
