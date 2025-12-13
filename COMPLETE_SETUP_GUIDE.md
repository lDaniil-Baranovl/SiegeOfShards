# 🎯 Полное руководство по настройке умного AI

## ✨ Обзор системы

Ваш AI теперь - полноценный противник как в Clash Royale:

### ✅ Основные возможности
- 🎴 **4 карты в руке** - циклическая система как в CR
- 🏰 **Автопоиск 3 башен** - по слоям Layer
- 🟩 **Зона спавна** - ограничение области появления
- 🚁 **Летающие юниты** - умная анти-воздушная защита
- 🧠 **4 стратегии** - Defend, Counter, Aggressive, Economy
- ⚡ **Управление элексиром** - резерв и регенерация
- 🎯 **Умные зелья** - на скопления врагов

---

## 🚀 Быстрая настройка (7 шагов)

### Шаг 1: Настройте карты (UnitCost)

Для каждой карты установите:

**Юниты наземные (не атакуют воздух):**
```
Card Type: Unit
Spell Type: None
Is Flying: ☐
Can Target Air: ☐
```

**Юниты наземные (атакуют воздух):**
```
Card Type: Unit
Spell Type: None
Is Flying: ☐
Can Target Air: ☑  ← ВАЖНО!
```

**Юниты летающие:**
```
Card Type: Unit
Spell Type: None
Is Flying: ☑
Can Target Air: ☑
```

**Зелья:**
```
Card Type: Spell
Spell Type: [Freeze/Damage/Heal]
Is Flying: ☐
Can Target Air: ☐
```

---

### Шаг 2: Создайте слои для башен

1. **Edit → Project Settings → Tags and Layers**
2. Добавьте:
   - `TowerPlayer`
   - `TowerEnemy`
3. Назначьте башням:
   - Все 3 башни игрока → Layer: `TowerPlayer`
   - Все 3 башни AI → Layer: `TowerEnemy`

---

### Шаг 3: Создайте SmartAI GameObject

1. Hierarchy → Create Empty → "SmartAI"
2. Add Component (4 компонента):
   - `SmartAIOpponent`
   - `BattlefieldAnalyzer`
   - `AICardSelector`
   - `AIDebugUI` (опционально)

---

### Шаг 4: Настройте BattlefieldAnalyzer

```
AI Team ID: 1
Player Team ID: 0

Auto-Find Towers by Layer:
  Player Tower Layer Name: TowerPlayer
  AI Tower Layer Name: TowerEnemy

Zone Settings:
  Danger Zone Radius: 15
  Mid Field Radius: 25
```

**Башни находятся автоматически!**

---

### Шаг 5: Настройте SmartAIOpponent

```
AI Settings:
  AI Team ID: 1
  Max Elixir: 10
  Starting Elixir: 5
  Elixir Regen Rate: 2
  Elixir Regen Amount: 1

AI Behavior:
  Min Think Delay: 1
  Max Think Delay: 3
  Aggressive Think Delay: 0.8
  Elixir Reserve: 3
  Passivity Threshold: 8

AI Deck (8 cards total):
  ⚠️ 8 уникальных карт
  ⚠️ Минимум 2 с canTargetAir = true

Card Cycle System:
  Hand Size: 4  (как в Clash Royale)

Spawn Zone:
  Use Spawn Area: ☑
  Spawn Area Center: [X, Y, Z]
  Spawn Area Size: [10, 0, 10]
```

**Настройте зону спавна в Scene View - увидите зелёный куб!**

---

### Шаг 6: Составьте колоду AI (8 карт)

**Рекомендуемый состав:**

| № | Название | Стоимость | Can Target Air | Тип |
|---|----------|-----------|----------------|-----|
| 1 | Skeleton | 1-2 | ☐ | Дешёвый |
| 2 | Goblin | 2 | ☐ | Дешёвый |
| 3 | Archer | 3 | **☑** | Анти-воздух |
| 4 | Knight | 4 | ☐ | Средний |
| 5 | Wizard | 5 | **☑** | Анти-воздух |
| 6 | Giant | 6 | ☐ | Танк |
| 7 | Dragon | 7 | ☑ | Летающий |
| 8 | Freeze | 4 | - | Зелье |

**Критично:**
- ✅ Ровно 8 уникальных карт
- ✅ Минимум 2 с `canTargetAir = true`
- ✅ Разброс стоимости 1-7 элексира
- ✅ 1-2 зелья для тактики

---

### Шаг 7: Запустите и проверьте

**В Console должно появиться:**
```
[BattlefieldAnalyzer] Found Player Tower: Tower1
[BattlefieldAnalyzer] Found Player Tower: Tower2
[BattlefieldAnalyzer] Found Player Tower: Tower3
[BattlefieldAnalyzer] Found AI Tower: Tower1
[BattlefieldAnalyzer] Found AI Tower: Tower2
[BattlefieldAnalyzer] Found AI Tower: Tower3
[BattlefieldAnalyzer] Total: 3 AI towers, 3 Player towers

[AI] Drew card: Skeleton (Hand: 1/4)
[AI] Drew card: Archer (Hand: 2/4)
[AI] Drew card: Knight (Hand: 3/4)
[AI] Drew card: Dragon (Hand: 4/4)
[AI] Deck initialized: 4 cards in hand, 4 in queue
[AI Hand] Skeleton(1), Archer(3), Knight(4), Dragon(7),
```

**Если увидели эти логи - всё работает!** ✅

---

## 📊 Debug UI - Что вы увидите

```
=== AI DEBUG ===
Elixir: 7/10
Strategy: Defend
Threat: High

Hand (4/4):
  Skeleton(1)    ← Зелёный = можно играть
  Archer(3)      ← Зелёный
  Knight(4)      ← Зелёный
  Dragon(7)      ← Зелёный

Units:
  Player: 3
  - Flying: 1    ← AI видит летающих!
  - Ground: 2
  AI: 2
  Near Tower: 2

Player State:
  Attacking: YES
  Flying Threat: YES  ← AI готовит анти-воздух
  Passive: NO
  Idle Time: 2.3s
```

---

## 🎮 Как AI принимает решения

### Пример игры:

```
Ход 1: Игрок размещает 3 летающих дракона

AI анализирует:
  - playerFlyingCount = 3
  - hasFlyingThreat = true
  - threatLevel = High

AI смотрит в руку (4/4):
  [Skeleton(1), Archer(3), Knight(4), Dragon(7)]

AI оценивает:
  - Skeleton: приоритет 70 (дешёвый, но не анти-воздух)
  - Archer: приоритет 140 (70 + 70 бонус анти-воздух!)
  - Knight: приоритет 60 (не анти-воздух)
  - Dragon: приоритет 65 (дорогой)

AI выбирает: Archer (приоритет 140)

AI играет Archer:
  - Рука: [Skeleton, Knight, Dragon, Freeze] ← Freeze с очереди
  - Archer вернулся в конец очереди

Console:
[AI] Strategy: Defend | Playing: Archer |
     Reason: URGENT: Anti-air defense | Priority: 140.0
[AI] Played: Archer, Drew next card
[AI Hand] Skeleton(1), Knight(4), Dragon(7), Freeze(4),
```

**AI не читерит** - использует только 4 карты в руке!

---

## 🔧 Тонкая настройка

### Сложность AI

**Лёгкий:**
```
Min Think Delay: 2
Max Think Delay: 4
Elixir Reserve: 4
Anti Air Priority Bonus: 20
```

**Средний (по умолчанию):**
```
Min Think Delay: 1
Max Think Delay: 3
Elixir Reserve: 3
Anti Air Priority Bonus: 30
```

**Сложный:**
```
Min Think Delay: 0.5
Max Think Delay: 2
Elixir Reserve: 2
Anti Air Priority Bonus: 50
```

---

## ✅ Финальный чеклист

Перед игрой убедитесь:

### Карты:
- [ ] Все юниты настроены (`isFlying`, `canTargetAir`)
- [ ] Все зелья настроены (`Card Type: Spell`, `Spell Type`)
- [ ] Минимум 2 анти-воздушных карты

### Башни:
- [ ] Созданы слои `TowerPlayer` и `TowerEnemy`
- [ ] 3 башни игрока → Layer: `TowerPlayer`
- [ ] 3 башни AI → Layer: `TowerEnemy`

### AI GameObject:
- [ ] SmartAI создан с 4 компонентами
- [ ] BattlefieldAnalyzer настроен
- [ ] SmartAIOpponent настроен
- [ ] Зона спавна видна (зелёный куб)

### Колода AI:
- [ ] 8 уникальных карт в AI Deck
- [ ] Минимум 2 с `canTargetAir = true`
- [ ] Разброс стоимости (дешёвые/средние/дорогие)
- [ ] 1-2 зелья

### Проверка:
- [ ] Console показывает найденные башни
- [ ] Console показывает руку AI (4/4)
- [ ] AI размещает юнитов
- [ ] Юниты AI в зелёной зоне
- [ ] AI реагирует на летающих
- [ ] Debug UI работает

---

## 🐛 Решение проблем

### ❌ "Layer 'TowerPlayer' not found!"
**Решение:** Создайте слой в Project Settings → Tags and Layers

### ❌ "No AI towers found!"
**Решение:** Назначьте Layer = `TowerEnemy` всем башням AI

### ❌ "Hand is empty!"
**Решение:** Заполните AI Deck (минимум 8 карт)

### ❌ AI не реагирует на летающих
**Решение:**
1. Проверьте `Health.CanFly = true` у летающих
2. Убедитесь что в колоде есть `canTargetAir = true`

### ❌ Юниты спавнятся вне зоны
**Решение:** Включите `Use Spawn Area = true`

---

## 📖 Дополнительная документация

- **[CARD_CYCLE_SYSTEM.md](CARD_CYCLE_SYSTEM.md)** - Система циклической колоды
- **[AUTO_SETUP_GUIDE.md](AUTO_SETUP_GUIDE.md)** - Автопоиск башен и зона спавна
- **[AI_FLYING_UNITS_UPDATE.md](AI_FLYING_UNITS_UPDATE.md)** - Летающие юниты
- **[FINAL_CHECKLIST.md](FINAL_CHECKLIST.md)** - Полный чеклист
- **[UPGRADE_GUIDE.md](UPGRADE_GUIDE.md)** - Быстрый гайд

---

## 🎉 Готово!

**Ваш AI теперь:**
- ✅ Работает как в Clash Royale (4 карты в руке)
- ✅ Автоматически находит башни
- ✅ Спавнит в ограниченной зоне
- ✅ Умеет контрить летающих
- ✅ Использует 4 стратегии
- ✅ Умно применяет зелья
- ✅ Управляет элексиром

**Полноценный AI противник готов к игре!** 🎮🚀
