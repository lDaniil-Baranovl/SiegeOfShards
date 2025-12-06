# Руководство по настройке сцены для MR (Quest 3)

## Текущий статус проекта

### ✅ Что УЖЕ ГОТОВО:
- Meta XR SDK установлен (v81.0.1)
- MR Utility Kit установлен
- Скрипты размещения арены созданы
- AndroidManifest.xml обновлён с нужными разрешениями
- XR Interaction Toolkit настроен

### ❌ Что НУЖНО НАСТРОИТЬ:

---

## Шаг 1: Настройка AndroidManifest.xml

✅ **УЖЕ СДЕЛАНО** - файл обновлён автоматически

Проверьте, что [AndroidManifest.xml](../../../Assets/Plugins/Android/AndroidManifest.xml) содержит:

```xml
<uses-permission android:name="com.oculus.permission.USE_SCENE" />
<uses-feature android:name="com.oculus.feature.PASSTHROUGH" android:required="true" />
```

---

## Шаг 2: Настройка Unity Project Settings

### 2.1 XR Plug-in Management

1. Откройте: `Edit → Project Settings → XR Plug-in Management`
2. Выберите вкладку **Android** (иконка Android)
3. Включите:
   - ✅ **Oculus**

### 2.2 Oculus Settings (под XR Plug-in Management)

1. Разверните **Oculus** в списке слева
2. В секции **Quest Features** включите:
   - ✅ **Scene Understanding** ← КРИТИЧНО для обнаружения столов
   - ✅ **Passthrough** ← Для MR режима
   - ✅ **Boundary** (опционально)

### 2.3 Build Settings

1. `File → Build Settings`
2. Platform: **Android**
3. Нажмите **Switch Platform** (если ещё не на Android)
4. **Minimum API Level**: Android 10.0 (API level 29) или выше

---

## Шаг 3: Создание префаба Arena (Поле битвы)

### Вариант А: Если у вас УЖЕ есть объект поля битвы в сцене

1. Найдите в Hierarchy ваш GameObject поля битвы
2. Перетащите его в папку `Assets/Prefabs/` → создастся префаб
3. Назовите его **"BattleArena"**

### Вариант Б: Создание нового префаба

1. `GameObject → Create Empty` → назовите "BattleArena"
2. Добавьте в него:
   - 3D модель вашей арены
   - Границы (Colliders)
   - NavMesh Surface (если используете)
3. Перетащите в `Assets/Prefabs/` для создания префаба
4. Удалите из Hierarchy (оставьте только префаб)

**ВАЖНО:** Префаб НЕ должен быть в сцене изначально - он будет создаваться динамически скриптом!

---

## Шаг 4: Настройка сцены testMechanic.unity

Откройте сцену [testMechanic.unity](../../../Assets/Scenes/testMechanic.unity)

### 4.1 Добавить MRUK (Scene Understanding)

**Если MRUK УЖЕ есть в сцене** - пропустите этот шаг.

1. В Hierarchy: `GameObject → Create Empty` → назовите **"MRUK"**
2. Add Component → **MRUK** (Meta MR Utility Kit)
3. В Inspector MRUK:
   - **Scene Loading Mode**: Auto Load
   - ✅ Load Scene on Startup

### 4.2 Создать ArenaPlacementSystem

1. `GameObject → Create Empty` → назовите **"ArenaPlacementSystem"**

2. **Add Component** → добавьте ВСЕ ТРИ скрипта:
   - `ClashRoyaleArenaPlacement`
   - `ManualArenaPlacement`
   - `HybridArenaPlacementManager`

### 4.3 Настройка HybridArenaPlacementManager

В Inspector компонента **HybridArenaPlacementManager**:

#### Placement Systems:
- **Auto Placement**: перетащите компонент `ClashRoyaleArenaPlacement` (с этого же объекта)
- **Manual Placement**: перетащите компонент `ManualArenaPlacement` (с этого же объекта)

#### Settings:
- **Auto Mode Timeout**: `10` секунд
- ✅ **Allow Mode Switch**: включено

#### UI Elements (опционально):
Если хотите показывать инструкции игроку:
- Создайте Canvas с TextMeshPro элементом
- Назначьте в **Instruction Text**

### 4.4 Настройка ClashRoyaleArenaPlacement

В Inspector компонента **ClashRoyaleArenaPlacement**:

- **Arena Prefab**: перетащите ваш префаб **BattleArena** из папки Prefabs
- **Min Table Size**: `0.8` (минимум 80см × 80см стол)
- **Preview Material**: создайте полупрозрачный материал (зелёный, alpha 0.5)

### 4.5 Настройка ManualArenaPlacement

В Inspector компонента **ManualArenaPlacement**:

- **Arena Prefab**: перетащите ваш префаб **BattleArena**
- **Right Controller**: найдите в Hierarchy `XR Origin → Camera Offset → RightHand Controller`
  - Если его нет - см. Шаг 4.7
- **Raycast Distance**: `5` метров
- **Placement Layers**: выберите слой для размещения (например, "Default" или создайте "SpatialMesh")

#### Input Actions:
Вам нужно настроить Input Actions для VR контроллеров:

1. Найдите в проекте: `Assets/Samples/XR Interaction Toolkit/.../XRI Default Input Actions`
2. Откройте его
3. В **ManualArenaPlacement**:
   - **Place Action**: выберите `XRI RightHand Interaction → Trigger Press`

### 4.6 Добавить OVRCameraRig или XR Origin

**КРИТИЧНО:** Без VR камеры игра не будет работать на Quest 3!

#### Вариант А: Использовать готовый префаб

1. Найдите в Project:
   ```
   Assets/Samples/XR Interaction Toolkit/3.0.9/Starter Assets/Prefabs/XR Origin (XR Rig).prefab
   ```
2. Перетащите его в Hierarchy вашей сцены
3. Если у вас уже есть Main Camera - удалите её

#### Вариант Б: Если используете Meta XR (рекомендуется для Quest 3)

1. В Hierarchy: `GameObject → XR → Room-Scale XR Rig`
2. Это создаст:
   - XR Origin
   - Camera
   - Controllers

### 4.7 Проверка иерархии сцены

Ваша Hierarchy должна выглядеть примерно так:

```
testMechanic
├── MRUK
├── ArenaPlacementSystem
│   ├── ClashRoyaleArenaPlacement (компонент)
│   ├── ManualArenaPlacement (компонент)
│   └── HybridArenaPlacementManager (компонент)
├── XR Origin (XR Rig)
│   ├── Camera Offset
│   │   ├── Main Camera
│   │   ├── LeftHand Controller
│   │   └── RightHand Controller
│   └── Locomotion System
├── Directional Light
└── (другие объекты сцены)
```

**ВАЖНО:** НЕ добавляйте префаб BattleArena в сцену - он создастся автоматически при размещении!

---

## Шаг 5: Настройка NavMesh (КРИТИЧНО!)

### 5.1 Исправить NavMeshBuildOnline.cs

В файле [NavMeshBuildOnline.cs](../../../Assets/Scripts/NavMesh/NavMeshBuildOnline.cs) есть КРИТИЧЕСКАЯ ОШИБКА:

❌ **НЕ ДЕЛАЙТЕ ТАК:**
```csharp
void Update()
{
    navMeshhSur.BuildNavMesh(); // Перестройка КАЖДЫЙ кадр = катастрофа!
}
```

✅ **ПРАВИЛЬНО:**
```csharp
// Удалите Update() полностью!

public void BuildNavMeshOnce()
{
    navMeshhSur.BuildNavMesh();
}
```

### 5.2 Вызов NavMesh после размещения арены

В [HybridArenaPlacementManager.cs](HybridArenaPlacementManager.cs:338-341) уже есть код для этого, но он закомментирован:

Раскомментируйте строку:
```csharp
// navMeshBuilder.BuildNavMeshOnce(); ← Уберите //
```

Или добавьте метод `BuildNavMeshOnce()` в класс NavMeshBuildOnline и вызовите его.

---

## Шаг 6: Настройка сцены MainMenu.unity

Сцена меню ([MainMenu.unity](../../../Assets/Scenes/MainMenu.unity)) НЕ требует MR компонентов!

**Только убедитесь:**
- У вас есть кнопка загрузки сцены `testMechanic`
- Кнопка использует `SceneManager.LoadScene("testMechanic")`

---

## Шаг 7: Build Settings - добавить сцены

1. `File → Build Settings`
2. Убедитесь, что обе сцены добавлены:
   - ✅ **MainMenu** (index 0)
   - ✅ **testMechanic** (index 1)

Если их нет - нажмите **Add Open Scenes** с открытой нужной сценой.

---

## Шаг 8: Тестирование на устройстве

### 8.1 На Quest 3 - настройка разрешений

**КРИТИЧНО:** Перед первым запуском игры на Quest 3:

1. Наденьте Quest 3
2. Откройте: **Settings → Apps → Permissions**
3. Найдите вашу игру
4. Включите: **✅ Scene Understanding**

**Без этого автоматическое обнаружение столов НЕ БУДЕТ РАБОТАТЬ!**

### 8.2 Рекомендуется пересоздать Guardian

Для лучшего распознавания мебели:
1. Settings → Guardian
2. Setup new room boundary
3. Quest 3 сканирует комнату и запомнит столы/мебель

### 8.3 Первый запуск

1. Подключите Quest 3 по USB
2. `File → Build And Run`
3. После запуска вы увидите:
   - **Сцена меню** → нажмите Play
   - **Сцена битвы** → начнётся автопоиск стола
   - Через ~10 сек появится зелёный индикатор на столе
   - Нажмите **Trigger** на правом контроллере
   - Арена разместится на столе!

---

## Шаг 9: Режимы работы

### Автоматический режим (Quest 3 только!)
- Система сама ищет подходящий стол
- Показывает зелёный индикатор
- Вы подтверждаете размещение триггером

### Ручной режим (Fallback)
- Активируется если:
  - Не найдено столов за 10 секунд
  - Пользователь нажал кнопку "Manual Mode"
- Вы наводите контроллером на любую поверхность
- Нажимаете триггер для размещения

### Совместимость
- **Quest 3 / 3S**: оба режима работают
- **Quest 2 / Pro**: только ручной режим (нет Scene Understanding)

---

## Troubleshooting (Решение проблем)

### Проблема: "Столы не находятся"

**Решения:**
1. ✅ Проверьте разрешения: Settings → Apps → YourGame → Scene ✅
2. Пересоздайте Guardian в Quest 3
3. Улучшите освещение комнаты
4. Убедитесь, что стол действительно горизонтальный

### Проблема: "Арена плывёт/исчезает"

**Причина:** Проблема с AR Anchor

**Решения:**
1. Убедитесь, что в сцене есть `ARAnchorManager`
2. Улучшите освещение
3. Не двигайте стол после размещения

### Проблема: "Низкий FPS / лаги"

**КРИТИЧНО:** Проверьте NavMeshBuildOnline.cs!

❌ Если `BuildNavMesh()` в `Update()` - немедленно исправьте (см. Шаг 5.1)

### Проблема: "Ничего не происходит"

**Чек-лист:**
1. ✅ MRUK в сцене?
2. ✅ ArenaPlacementSystem настроен?
3. ✅ Arena Prefab назначен?
4. ✅ XR Origin в сцене?
5. ✅ Разрешения на Quest 3 включены?

### Проблема: "Контроллеры не работают"

1. Убедитесь, что `RightHand Controller` найден в сцене
2. Проверьте Input Actions настроены
3. Перезапустите приложение на Quest

---

## Контрольный чеклист перед билдом

Перед финальным билдом проверьте:

### Unity Settings:
- [ ] XR Plug-in Management → Android → Oculus ✅
- [ ] Oculus Settings → Scene Understanding ✅
- [ ] Oculus Settings → Passthrough ✅
- [ ] Build Settings → Platform: Android
- [ ] Обе сцены добавлены в Build Settings

### Сцена testMechanic:
- [ ] MRUK в сцене
- [ ] ArenaPlacementSystem настроен
- [ ] Arena Prefab назначен
- [ ] XR Origin в сцене
- [ ] Input Actions настроены
- [ ] NavMesh НЕ в Update()

### Файлы:
- [ ] AndroidManifest.xml содержит разрешения
- [ ] Префаб Arena создан
- [ ] NavMeshBuildOnline исправлен

### На Quest 3:
- [ ] Разрешения включены
- [ ] Guardian настроен
- [ ] Стол в комнате

---

## Дополнительная информация

### Документация:
- [CHEATSHEET.md](CHEATSHEET.md) - шпаргалка по API
- [ARCHITECTURE.md](ARCHITECTURE.md) - архитектура системы
- [CODE_REVIEW.md](../../../CODE_REVIEW.md) - code review проекта

### Горячие клавиши Debug:
В билде с Debug mode (в Inspector HybridManager будут кнопки):
- **Reset Placement** - сбросить размещение
- **Switch to Manual** - переключить в ручной режим
- **Switch to Auto** - переключить в авто режим

---

## Что дальше?

После успешного размещения арены:

1. **NavMesh автоматически перестроится** (если настроили Шаг 5.2)
2. **Юниты смогут ходить** по арене
3. **Игра начнётся** в MR режиме

**Удачи с проектом!** 🎮

_Если возникли проблемы - проверьте Console в Unity на ошибки_
_Все логи системы имеют префикс `[ArenaPlacement]` или `[HybridPlacement]`_
