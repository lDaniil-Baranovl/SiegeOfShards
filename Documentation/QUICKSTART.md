# Быстрый старт - Размещение арены в VR

## Какой скрипт использовать?

| Скрипт | Когда использовать | Сложность | UX качество |
|--------|-------------------|-----------|-------------|
| **ArenaPlacement.cs** | Quest 3 с Scene Understanding | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **ManualArenaPlacement.cs** | Простой fallback или отладка | ⭐ | ⭐⭐⭐ |
| **HybridArenaPlacementManager.cs** | Production-ready решение | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |

---

## Рекомендация: HybridArenaPlacementManager

### Что он делает:
1. **Автоматически ищет стол** (10 секунд)
2. Если не нашел → **переключается на ручной режим**
3. Игрок видит индикатор и подтверждает размещение
4. Арена **закрепляется AR Anchor** (не двигается)

### Минимальная настройка (5 минут):

#### 1. Создайте GameObject:
```
Hierarchy → Create Empty → "ArenaPlacementSystem"
```

#### 2. Добавьте компоненты:
- ArenaPlacement
- ManualArenaPlacement
- HybridArenaPlacementManager

#### 3. Настройте в Inspector:

**ArenaPlacement:**
- Arena Prefab: [ваш префаб арены]
- Preferred Height: 0.9
- Bounding Box Manager: [AR Bounding Box Manager из сцены]
- Anchor Manager: [AR Anchor Manager из сцены]
- Confirm Placement Action: XRI RightHand/Trigger Press
- Player Camera: Main Camera

**ManualArenaPlacement:**
- Arena Prefab: [тот же префаб]
- Right Controller: XR Origin/Camera Offset/RightHand Controller
- Place Action: XRI RightHand/Trigger Press
- Anchor Manager: [AR Anchor Manager]

**HybridArenaPlacementManager:**
- Auto Placement: [перетащите ArenaPlacement компонент]
- Manual Placement: [перетащите ManualArenaPlacement компонент]
- Auto Mode Timeout: 10

#### 4. Готово!

Запустите игру. Система автоматически:
1. Ищет стол на уровне пояса
2. Показывает зеленый индикатор
3. Игрок нажимает Trigger → арена размещена

---

## Проблемы? Debug чеклист:

### ❌ Столы не находятся

**Решение:**
1. Проверьте: Settings → Apps → [Your App] → Permissions → Scene ✅
2. XR Plug-in Management → Oculus → Scene Understanding ✅
3. AndroidManifest.xml содержит:
   ```xml
   <uses-permission android:name="com.oculus.permission.USE_SCENE" />
   ```

### ❌ Арена "плывет"

**Решение:**
- Убедитесь, что AR Anchor Manager есть в сцене
- Проверьте хорошее освещение в комнате
- Пройдите Guardian setup заново

### ❌ Ничего не работает

**Решение:**
1. Откройте Console (Ctrl+Shift+C)
2. Ищите ошибки с префиксом `[ArenaPlacement]`
3. Проверьте все ссылки в Inspector (не должно быть "None")

---

## Интеграция с вашей игрой

### Получить позицию арены:

```csharp
HybridArenaPlacementManager placement = FindObjectOfType<HybridArenaPlacementManager>();

if (placement.IsPlaced)
{
    Vector3 arenaPos = placement.GetArenaPosition();
    // Используйте позицию для спавна юнитов и т.д.
}
```

### Подождать размещения перед началом игры:

```csharp
IEnumerator WaitForArenaPlacement()
{
    var placement = FindObjectOfType<HybridArenaPlacementManager>();

    while (!placement.IsPlaced)
    {
        yield return null;
    }

    // Арена размещена, начинаем игру
    StartGame();
}
```

### Перестроить NavMesh после размещения:

```csharp
// В HybridArenaPlacementManager.NotifyGameSystems() уже есть вызов:
var navMeshBuilder = FindObjectOfType<NavMeshBuildOnline>();
if (navMeshBuilder != null)
{
    navMeshBuilder.BuildNavMeshOnce(); // Один раз, не в Update!
}
```

---

## Настройка высоты

По умолчанию система ищет столы на высоте **0.9м** (уровень пояса).

**Если хотите изменить:**
- `ArenaPlacement` → Preferred Height: 0.7 (ниже)
- `ArenaPlacement` → Height Tolerance: 0.3 (больше допуск)

**Типичные высоты:**
- Обеденный стол: 0.75м
- Рабочий стол: 0.75м
- Барная стойка: 1.0-1.1м
- Кофейный столик: 0.45м

---

## Кастомизация индикатора

### Создайте свой индикатор:

1. Create → 3D Object → Quad (или импортируйте модель)
2. Добавьте полупрозрачный материал (зеленый для валидного)
3. Сохраните как префаб
4. ArenaPlacement → Placement Indicator Prefab: [ваш префаб]

### Или используйте автоматический (уже работает)

Система создаст простой зеленый круг автоматически.

---

## Полная документация

Смотрите [README_ARENA_PLACEMENT.md](README_ARENA_PLACEMENT.md) для:
- Детальных объяснений всех параметров
- Дополнительных функций (масштабирование, поворот)
- Troubleshooting
- Оптимизация производительности
- Альтернативные подходы

---

## Частые вопросы

**Q: Можно ли размещать на полу?**
A: Да, измените Preferred Height на 0.0

**Q: Как разместить на любой горизонтальной поверхности?**
A: Используйте только ManualArenaPlacement (отключите Auto)

**Q: Можно ли переместить арену после размещения?**
A: Да, вызовите `placement.ResetPlacement()`

**Q: Работает ли на Quest 2?**
A: Quest 2 не поддерживает Scene Understanding. Используйте только ManualArenaPlacement.

**Q: Как сохранить позицию между сессиями?**
A: Смотрите раздел "Сохранение позиции" в полной документации

---

**Все готово!** 🎮

Система автоматически найдет стол и разместит арену на уровне пояса.

_Если что-то не работает - проверьте Debug чеклист выше._
