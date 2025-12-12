# 🚀 Быстрый старт: Умный AI оппонент

## ✅ Что было сделано

Создана полностью новая умная AI система, которая:
- ✅ Анализирует действия игрока
- ✅ Управляет элексиром разумно
- ✅ Выбирает стратегию (защита/атака/контр-атака)
- ✅ Умно использует зелья (freeze/meteor/heal)
- ✅ Размещает юнитов тактически
- ✅ Реагирует на пассивность игрока

## 📁 Новые файлы

```
Assets/Scripts/Cards/
  └─ UnitCost.cs (обновлён - добавлены типы карт)

Assets/Scripts/AI Opponent/
  ├─ SmartAIOpponent.cs (главный контроллер)
  ├─ BattlefieldAnalyzer.cs (анализ поля боя)
  ├─ AICardSelector.cs (выбор карт)
  ├─ AIDebugUI.cs (отладка)
  └─ README_AI_SETUP.md (подробная документация)
```

## ⚡ 5 шагов для запуска

### 1️⃣ Настройте типы карт

Для каждой карты (UnitCost ScriptableObject):

**Юниты:** `Card Type = Unit`, `Spell Type = None`

**Зелья:**
- Freeze: `Card Type = Spell`, `Spell Type = Freeze`
- Meteor: `Card Type = Spell`, `Spell Type = Damage`
- Heal: `Card Type = Spell`, `Spell Type = Heal`

### 2️⃣ Создайте AI GameObject

1. Hierarchy → Create Empty → назовите "SmartAI"
2. Add Component:
   - `SmartAIOpponent`
   - `BattlefieldAnalyzer`
   - `AICardSelector`
   - `AIDebugUI` (опционально)

### 3️⃣ Настройте BattlefieldAnalyzer

```
AI Tower: [ваша AI башня]
Player Tower: [башня игрока]
AI Team ID: 1
Player Team ID: 0
```

### 4️⃣ Настройте SmartAIOpponent

```
AI Team ID: 1
AI Deck: [добавьте 6-8 карт]
```

**Рекомендуемая колода:**
- 3 дешёвых юнита (1-3 элексира)
- 2 средних юнита (4-6 элексира)
- 1 дорогой юнит (7+)
- 1-2 зелья

### 5️⃣ Отключите старый AI

Найдите объект со старым `RandomSpawner` (AISpawnUnit) и отключите его.

## 🎮 Тестирование

1. Запустите игру
2. Если включили AIDebugUI - увидите инфо в левом верхнем углу
3. Проверьте консоль - AI будет писать свои действия:
   ```
   [AI] Strategy: Defend | Playing: Skeleton | Reason: Quick defense
   ```

## 🎯 Типичные сценарии

| Действие игрока | Реакция AI |
|-----------------|-----------|
| Массовая атака | Защита дешёвыми юнитами + freeze |
| Медленный push | Контр-атака средними юнитами |
| Пассивность 8+ сек | Агрессивная атака |
| Мало юнитов | Экономит элексир |
| Много врагов | Использует meteor/freeze |

## ⚙️ Настройка сложности

**Лёгкий AI:**
```
Min Think Delay: 2
Max Think Delay: 4
Elixir Reserve: 4
```

**Средний AI (по умолчанию):**
```
Min Think Delay: 1
Max Think Delay: 3
Elixir Reserve: 3
```

**Сложный AI:**
```
Min Think Delay: 0.5
Max Think Delay: 2
Elixir Reserve: 2
```

## 🐛 Решение проблем

**AI ничего не делает:**
- Проверьте что AI Deck заполнен
- Проверьте что башни назначены
- Посмотрите консоль на ошибки

**AI слишком пассивный:**
- Уменьшите `Elixir Reserve` (3 → 2)
- Уменьшите `Passivity Threshold` (8 → 5)

**AI тратит весь элексир:**
- Увеличьте `Elixir Reserve` (3 → 4)

## 📖 Подробная документация

Полное руководство: `Assets/Scripts/AI Opponent/README_AI_SETUP.md`

---

**Готово!** Теперь у вас умный AI оппонент! 🎉
