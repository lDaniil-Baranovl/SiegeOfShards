# Шпаргалка - Arena Placement System

## Быстрый старт (3 минуты)

### 1. Добавьте в сцену:
```
GameObject → Create Empty → "ArenaPlacementSystem"
Add Component:
  - ArenaPlacement
  - ManualArenaPlacement
  - HybridArenaPlacementManager
```

### 2. Настройте параметры:
```
ArenaPlacement:
  ✓ Arena Prefab: [ваш префаб]
  ✓ Bounding Box Manager: [ARBoundingBoxManager]
  ✓ Anchor Manager: [ARAnchorManager]
  ✓ Confirm Action: XRI RightHand/Trigger

ManualArenaPlacement:
  ✓ Arena Prefab: [тот же префаб]
  ✓ Right Controller: [XR RightHand Controller]
  ✓ Place Action: XRI RightHand/Trigger

HybridManager:
  ✓ Auto Placement: [ArenaPlacement компонент]
  ✓ Manual Placement: [ManualArenaPlacement компонент]
```

### 3. Готово! 🎉

---

## Параметры настройки

### Высота стола
```csharp
// ArenaPlacement Inspector:
Preferred Height: 0.9f   // 90см - стандартный стол
Height Tolerance: 0.2f   // ±20см допуск
```

**Типичные значения:**
- Обеденный стол: `0.75`
- Рабочий стол: `0.75`
- Барная стойка: `1.0-1.1`
- Кофейный столик: `0.45`

### Размер стола
```csharp
// ArenaPlacement Inspector:
Min Arena Size: (0.8, 0.1, 1.2)
// X=80см (ширина), Z=120см (глубина)
```

### Таймаут
```csharp
// HybridManager Inspector:
Auto Mode Timeout: 10f  // Секунд для автопоиска
```

---

## Код - Основные операции

### Проверить размещение
```csharp
var placement = FindObjectOfType<HybridArenaPlacementManager>();

if (placement.IsPlaced)
{
    Debug.Log("Arena placed!");
}
```

### Получить позицию
```csharp
Vector3 pos = placement.GetArenaPosition();
```

### Сбросить (для отладки)
```csharp
placement.ResetPlacement();
```

### Принудительно выбрать режим
```csharp
placement.ForceMode(PlacementMode.Manual);
```

### Подождать размещения
```csharp
IEnumerator WaitForPlacement()
{
    while (!placement.IsPlaced)
        yield return null;

    StartGame();
}
```

---

## Интеграция с NavMesh

### ❌ Старый код (ПЛОХО):
```csharp
// NavMeshBuildOnline.cs
void Update()
{
    navMeshSurface.BuildNavMesh(); // КРИТИЧЕСКАЯ ОШИБКА!
}
```

### ✅ Новый код (ХОРОШО):
```csharp
// NavMeshBuildOnline.cs
public void BuildNavMeshOnce()
{
    navMeshSurface.BuildNavMesh();
}

// HybridManager вызовет автоматически при размещении
```

---

## Permissions (Quest 3)

### AndroidManifest.xml:
```xml
<uses-permission android:name="com.oculus.permission.USE_SCENE" />
<uses-feature android:name="com.oculus.feature.PASSTHROUGH" required="true" />
```

### Unity Settings:
```
Edit → Project Settings → XR Plug-in Management → Oculus:
  ✓ Scene Understanding
  ✓ Passthrough
```

---

## Debug команды

### В коде:
```csharp
// Включить Debug режим
#if UNITY_EDITOR
placement.ForceMode(PlacementMode.Manual); // Проще для тестов
#endif
```

### В Inspector:
```
HybridManager → OnGUI Debug:
  - Показывает текущий режим
  - Кнопка Reset
  - Кнопки переключения режима
```

### В Console:
Ищите префикс `[ArenaPlacement]` или `[HybridPlacement]`

---

## Troubleshooting (1 минута)

### Проблема → Решение

**Столы не находятся:**
1. Settings → Apps → Permissions → Scene ✅
2. Пересоздайте Guardian

**Арена плывет:**
- Проверьте: Anchor Manager есть в сцене?
- Улучшите освещение

**Ничего не работает:**
- Console → ищите ошибки
- Проверьте все ссылки в Inspector

**Quest 2 не работает:**
- Quest 2 НЕ поддерживает Scene Understanding
- Используйте только Manual режим

---

## Input Actions (напоминание)

### Подтверждение размещения:
```
XRI RightHand Interaction → Trigger Press
или
XRI LeftHand Interaction → Trigger Press
```

### Как добавить:
```
Project → XRI Default Input Actions → XRI RightHand
→ Add Action → "Confirm Placement"
→ Add Binding → <XRController>{RightHand}/triggerPressed
```

---

## Производительность

### Метрики:
- CPU: <1%
- Update calls: ~2 Hz (не 90 FPS!)
- NavMesh build: 1 раз (не каждый кадр!)

### Профилирование:
```
Window → Analysis → Profiler
  → CPU Usage → Scripts
    → Ищите ArenaPlacement.Update
```

**Должно быть:** <0.1ms

---

## UI Integration (опционально)

### Canvas с инструкциями:
```csharp
[SerializeField] TextMeshProUGUI instructionText;

void Update()
{
    if (placement.IsPlaced)
        instructionText.text = "";
    else
        instructionText.text = "Look for green indicator";
}
```

### Прогресс-бар сканирования:
```csharp
[SerializeField] Slider progressSlider;

void Update()
{
    float progress = Time.time / autoModeTimeout;
    progressSlider.value = Mathf.Clamp01(progress);
}
```

---

## События (расширенное)

### Создайте Event System:
```csharp
public static class GameEvents
{
    public static event Action<Vector3> OnArenaPlaced;
}

// В ArenaPlacement:
GameEvents.OnArenaPlaced?.Invoke(position);

// В других скриптах:
void OnEnable()
{
    GameEvents.OnArenaPlaced += HandlePlacement;
}
```

---

## Режимы работы

| Режим | Когда использовать |
|-------|-------------------|
| **Auto** | Production (Quest 3) |
| **Manual** | Fallback, Quest 2 |
| **Hybrid** | Рекомендуется (авто+фоллбек) |

---

## Материалы для индикатора

### Создание:
```
Create → Material → "ValidPlacement"
  Rendering Mode: Transparent
  Color: Green (0, 255, 0, 128)

Create → Material → "InvalidPlacement"
  Rendering Mode: Transparent
  Color: Red (255, 0, 0, 128)
```

### Применение:
```
ArenaPlacement Inspector:
  Valid Placement Material: ValidPlacement
  Invalid Placement Material: InvalidPlacement
```

---

## Layers (если нужно)

### Для Manual режима:
```
Edit → Project Settings → Tags and Layers:
  Layer 10: SpatialMesh
```

```csharp
ManualArenaPlacement Inspector:
  Placement Layers: SpatialMesh
```

---

## Полезные ссылки

📖 [Полная документация](README_ARENA_PLACEMENT.md)
🏗️ [Архитектура](ARCHITECTURE.md)
🚀 [Быстрый старт](QUICKSTART.md)
🔍 [Code Review проекта](../../../CODE_REVIEW.md)

---

## Контрольный чеклист перед билдом

- [ ] Arena Prefab назначен
- [ ] AR Managers в сцене
- [ ] Input Actions настроены
- [ ] AndroidManifest permissions
- [ ] Scene Understanding включен
- [ ] Протестировано на устройстве
- [ ] NavMesh НЕ в Update
- [ ] Console без ошибок

---

## Горячие клавиши (Editor)

```
Ctrl+Shift+C  → Console
Ctrl+7        → Profiler
F             → Focus на объект
```

---

## Типичные ошибки

### ❌ НЕ делайте:
```csharp
// Поиск каждый кадр
void Update() {
    var placement = FindObjectOfType<...>();
}

// NavMesh в Update
void Update() {
    navMesh.BuildNavMesh();
}

// Создание без уничтожения
Instantiate(indicator); // Memory leak!
```

### ✅ Делайте:
```csharp
// Кеширование
private HybridManager placement;
void Awake() {
    placement = FindObjectOfType<...>();
}

// NavMesh один раз
void OnArenaPlaced() {
    navMesh.BuildNavMeshOnce();
}

// Pooling или Destroy
if (indicator != null)
    Destroy(indicator);
```

---

**Все готово! Удачи! 🎮**

_При проблемах → смотрите полную документацию_
