using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AICardSelector : MonoBehaviour
{
    [Header("AI Deck")]
    [SerializeField] private List<UnitCost> aiDeck = new List<UnitCost>();

    [Header("Selection Weights")]
    [SerializeField] private float defenseUrgencyMultiplier = 2f;
    [SerializeField] private float spellEfficiencyThreshold = 3;
    [SerializeField] private float antiAirPriorityBonus = 30f;

    private BattlefieldAnalyzer analyzer;
    private int currentElixir;

    public void Initialize(BattlefieldAnalyzer battlefieldAnalyzer)
    {
        analyzer = battlefieldAnalyzer;
    }

    public void SetElixir(int elixir)
    {
        currentElixir = elixir;
    }

    public enum AIStrategy
    {
        Defend,
        CounterAttack,
        Aggressive,
        EconomyBuild
    }

    public class CardChoice
    {
        public UnitCost card;
        public Vector3 spawnPosition;
        public float priority;
        public string reason;
    }

    public CardChoice SelectBestCard(BattlefieldAnalyzer.BattlefieldState battleState, AIStrategy strategy)
    {
        List<CardChoice> availableChoices = new List<CardChoice>();

        foreach (var card in aiDeck)
        {
            if (card.elixirCost > currentElixir)
                continue;

            CardChoice choice = EvaluateCard(card, battleState, strategy);
            if (choice != null)
            {
                availableChoices.Add(choice);
            }
        }

        if (availableChoices.Count == 0)
            return null;

        availableChoices = availableChoices.OrderByDescending(c => c.priority).ToList();

        return availableChoices[0];
    }

    private CardChoice EvaluateCard(UnitCost card, BattlefieldAnalyzer.BattlefieldState state, AIStrategy strategy)
    {
        CardChoice choice = new CardChoice
        {
            card = card,
            priority = 0f,
            reason = ""
        };

        if (card.IsSpell())
        {
            return EvaluateSpell(card, state, strategy);
        }
        else
        {
            return EvaluateUnit(card, state, strategy);
        }
    }

    private CardChoice EvaluateSpell(UnitCost spell, BattlefieldAnalyzer.BattlefieldState state, AIStrategy strategy)
    {
        CardChoice choice = new CardChoice { card = spell };

        int targetCount = 0;

        switch (spell.spellType)
        {
            case SpellType.Freeze:
                targetCount = CountUnitsInRadius(analyzer.GetPlayerUnits(), analyzer.GetBestSpellPosition(SpellType.Freeze), 5f);

                if (targetCount >= spellEfficiencyThreshold)
                {
                    choice.priority = 80f + (targetCount * 10f);
                    choice.reason = $"Freeze {targetCount} enemy units";
                    choice.spawnPosition = analyzer.GetBestSpellPosition(SpellType.Freeze);
                }
                else
                {
                    return null;
                }
                break;

            case SpellType.Damage:
                targetCount = CountUnitsInRadius(analyzer.GetPlayerUnits(), analyzer.GetBestSpellPosition(SpellType.Damage), 6f);

                if (targetCount >= spellEfficiencyThreshold)
                {
                    choice.priority = 75f + (targetCount * 8f);
                    choice.reason = $"Damage {targetCount} enemy units";
                    choice.spawnPosition = analyzer.GetBestSpellPosition(SpellType.Damage);
                }
                else
                {
                    return null;
                }
                break;

            case SpellType.Heal:
                targetCount = CountUnitsInRadius(analyzer.GetAIUnits(), analyzer.GetBestSpellPosition(SpellType.Heal), 5f);

                if (targetCount >= 2 && strategy == AIStrategy.Defend)
                {
                    choice.priority = 60f + (targetCount * 5f);
                    choice.reason = $"Heal {targetCount} defending units";
                    choice.spawnPosition = analyzer.GetBestSpellPosition(SpellType.Heal);
                }
                else
                {
                    return null;
                }
                break;

            default:
                return null;
        }

        if (state.threatLevel == BattlefieldAnalyzer.ThreatLevel.High ||
            state.threatLevel == BattlefieldAnalyzer.ThreatLevel.Critical)
        {
            choice.priority *= defenseUrgencyMultiplier;
        }

        return choice;
    }

    private CardChoice EvaluateUnit(UnitCost unit, BattlefieldAnalyzer.BattlefieldState state, AIStrategy strategy)
    {
        CardChoice choice = new CardChoice
        {
            card = unit,
            priority = 50f,
            reason = "Deploy unit"
        };

        if (state.hasFlyingThreat && unit.canTargetAir)
        {
            choice.priority += antiAirPriorityBonus;
            choice.reason = "Anti-air unit vs flying threat";
        }

        switch (strategy)
        {
            case AIStrategy.Defend:
                choice.spawnPosition = analyzer.GetBestDefensiveSpawnPoint();
                choice.reason = "Defensive deployment";

                if (state.hasFlyingThreat && unit.canTargetAir)
                {
                    choice.priority += 40f;
                    choice.reason = "URGENT: Anti-air defense";
                }
                else if (unit.elixirCost <= 3)
                {
                    choice.priority = 70f;
                    choice.reason = "Quick defense with cheap unit";
                }
                else if (unit.elixirCost <= 5)
                {
                    choice.priority = 60f;
                }
                else
                {
                    choice.priority = 40f;
                }

                if (state.threatLevel == BattlefieldAnalyzer.ThreatLevel.Critical)
                {
                    choice.priority *= 1.5f;
                }
                break;

            case AIStrategy.CounterAttack:
                choice.spawnPosition = analyzer.GetBestDefensiveSpawnPoint();
                choice.reason = "Counter-attack deployment";

                if (unit.elixirCost >= 4 && unit.elixirCost <= 6)
                {
                    choice.priority = 75f;
                    choice.reason = "Medium-cost counter unit";
                }
                else if (unit.elixirCost > 6)
                {
                    choice.priority = 65f;
                    choice.reason = "Heavy counter unit";
                }
                else
                {
                    choice.priority = 50f;
                }
                break;

            case AIStrategy.Aggressive:
                choice.spawnPosition = analyzer.GetBestOffensiveSpawnPoint();
                choice.reason = "Aggressive push";

                if (unit.elixirCost >= 5)
                {
                    choice.priority = 80f;
                    choice.reason = "Heavy offensive unit";
                }
                else if (unit.elixirCost >= 3)
                {
                    choice.priority = 70f;
                    choice.reason = "Medium offensive unit";
                }
                else
                {
                    choice.priority = 60f;
                    choice.reason = "Light offensive unit";
                }
                break;

            case AIStrategy.EconomyBuild:
                if (unit.elixirCost <= 4)
                {
                    choice.spawnPosition = analyzer.GetBestOffensiveSpawnPoint();
                    choice.priority = 55f;
                    choice.reason = "Economy pressure with cheap unit";
                }
                else
                {
                    return null;
                }
                break;
        }

        return choice;
    }

    private int CountUnitsInRadius(List<Health> units, Vector3 center, float radius)
    {
        int count = 0;
        foreach (var unit in units)
        {
            if (unit != null && Vector3.Distance(unit.transform.position, center) <= radius)
            {
                count++;
            }
        }
        return count;
    }

    public List<UnitCost> GetAvailableCards(int elixir)
    {
        return aiDeck.Where(card => card.elixirCost <= elixir).ToList();
    }

    public void SetDeck(List<UnitCost> deck)
    {
        aiDeck = new List<UnitCost>(deck);
    }
}
