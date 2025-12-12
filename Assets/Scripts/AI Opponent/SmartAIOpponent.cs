using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartAIOpponent : MonoBehaviour
{
    [Header("AI Components")]
    [SerializeField] private BattlefieldAnalyzer battlefieldAnalyzer;
    [SerializeField] private AICardSelector cardSelector;

    [Header("AI Settings")]
    [SerializeField] private int aiTeamID = 1;
    [SerializeField] private int maxElixir = 10;
    [SerializeField] private int startingElixir = 5;
    [SerializeField] private float elixirRegenRate = 2f;
    [SerializeField] private int elixirRegenAmount = 1;

    [Header("AI Behavior")]
    [SerializeField] private float minThinkDelay = 1f;
    [SerializeField] private float maxThinkDelay = 3f;
    [SerializeField] private float aggressiveThinkDelay = 0.8f;
    [SerializeField] private int elixirReserve = 3;
    [SerializeField] private float passivityThreshold = 8f;

    [Header("AI Deck")]
    [SerializeField] private List<UnitCost> aiDeck = new List<UnitCost>();

    private int currentElixir;
    private bool isRunning = true;
    private AICardSelector.AIStrategy currentStrategy;

    private void Start()
    {
        if (battlefieldAnalyzer == null)
        {
            battlefieldAnalyzer = GetComponent<BattlefieldAnalyzer>();
        }

        if (cardSelector == null)
        {
            cardSelector = GetComponent<AICardSelector>();
        }

        if (battlefieldAnalyzer == null || cardSelector == null)
        {
            Debug.LogError("SmartAIOpponent: Missing required components!");
            enabled = false;
            return;
        }

        cardSelector.Initialize(battlefieldAnalyzer);
        cardSelector.SetDeck(aiDeck);

        currentElixir = startingElixir;

        StartCoroutine(ElixirRegeneration());
        StartCoroutine(AIThinkLoop());
    }

    private IEnumerator ElixirRegeneration()
    {
        while (isRunning)
        {
            yield return new WaitForSeconds(elixirRegenRate);

            if (currentElixir < maxElixir)
            {
                currentElixir = Mathf.Min(currentElixir + elixirRegenAmount, maxElixir);
            }
        }
    }

    private IEnumerator AIThinkLoop()
    {
        yield return new WaitForSeconds(2f);

        while (isRunning)
        {
            BattlefieldAnalyzer.BattlefieldState battleState = battlefieldAnalyzer.AnalyzeBattlefield();

            currentStrategy = DetermineStrategy(battleState);

            AICardSelector.CardChoice bestChoice = SelectAndPlayCard(battleState);

            if (bestChoice != null)
            {
                Debug.Log($"[AI] Strategy: {currentStrategy} | Playing: {bestChoice.card.unitName} | Reason: {bestChoice.reason} | Priority: {bestChoice.priority:F1}");
            }

            float thinkDelay = CalculateThinkDelay(battleState);
            yield return new WaitForSeconds(thinkDelay);
        }
    }

    private AICardSelector.AIStrategy DetermineStrategy(BattlefieldAnalyzer.BattlefieldState state)
    {
        switch (state.threatLevel)
        {
            case BattlefieldAnalyzer.ThreatLevel.Critical:
            case BattlefieldAnalyzer.ThreatLevel.High:
                return AICardSelector.AIStrategy.Defend;

            case BattlefieldAnalyzer.ThreatLevel.Medium:
                if (state.aiUnitsCount >= state.playerUnitsCount)
                {
                    return AICardSelector.AIStrategy.CounterAttack;
                }
                else
                {
                    return AICardSelector.AIStrategy.Defend;
                }

            case BattlefieldAnalyzer.ThreatLevel.Low:
                if (currentElixir >= 7)
                {
                    return AICardSelector.AIStrategy.CounterAttack;
                }
                else
                {
                    return AICardSelector.AIStrategy.EconomyBuild;
                }

            case BattlefieldAnalyzer.ThreatLevel.None:
                if (state.timeSinceLastPlayerAction > passivityThreshold)
                {
                    return AICardSelector.AIStrategy.Aggressive;
                }
                else if (currentElixir >= 8)
                {
                    return AICardSelector.AIStrategy.Aggressive;
                }
                else
                {
                    return AICardSelector.AIStrategy.EconomyBuild;
                }

            default:
                return AICardSelector.AIStrategy.EconomyBuild;
        }
    }

    private AICardSelector.CardChoice SelectAndPlayCard(BattlefieldAnalyzer.BattlefieldState state)
    {
        int availableElixir = currentElixir;

        if (currentStrategy != AICardSelector.AIStrategy.Defend &&
            state.threatLevel != BattlefieldAnalyzer.ThreatLevel.Critical)
        {
            availableElixir = Mathf.Max(0, currentElixir - elixirReserve);
        }

        if (availableElixir <= 0)
            return null;

        cardSelector.SetElixir(availableElixir);

        AICardSelector.CardChoice choice = cardSelector.SelectBestCard(state, currentStrategy);

        if (choice != null && choice.card.elixirCost <= currentElixir)
        {
            SpendElixir(choice.card.elixirCost);
            SpawnCard(choice.card, choice.spawnPosition);
            return choice;
        }

        return null;
    }

    private void SpawnCard(UnitCost card, Vector3 position)
    {
        if (card.prefabs == null || card.prefabs.Length == 0)
        {
            Debug.LogError($"AI: Card '{card.unitName}' has no prefabs!");
            return;
        }

        for (int i = 0; i < card.prefabs.Length; i++)
        {
            if (card.prefabs[i] == null)
                continue;

            Vector3 spawnPos = position;

            if (card.spawnOffsets != null && i < card.spawnOffsets.Length)
            {
                spawnPos += card.spawnOffsets[i];
            }
            else if (card.prefabs.Length > 1)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 1.5f;
                spawnPos += new Vector3(randomOffset.x, 0, randomOffset.y);
            }

            GameObject spawnedObject = Instantiate(card.prefabs[i], spawnPos, Quaternion.identity);

            SetupSpawnedObject(spawnedObject);
        }
    }

    private void SetupSpawnedObject(GameObject obj)
    {
        if (obj.TryGetComponent<Health>(out Health health))
        {
            health.SetTeam(aiTeamID);
        }

        var damagers = obj.GetComponentsInChildren<DamageCentaur>();
        foreach (var dmg in damagers)
        {
            dmg.SetTeam(aiTeamID);
        }

        if (obj.TryGetComponent<UnitStateManager>(out var stateManager))
        {
            stateManager.teamID = aiTeamID;
        }

        var freezeZone = obj.GetComponent<FreezeZone>();
        if (freezeZone != null)
        {
            SetFreezeZoneTeam(freezeZone, 0);
        }

        var healZone = obj.GetComponent<HealingZone>();
        if (healZone != null)
        {
            healZone.zoneTeamID = aiTeamID;
        }

        var meteorZone = obj.GetComponent<MeteorDamageZone>();
        if (meteorZone != null)
        {
            meteorZone.teamID = 0;
        }

        var laserZone = obj.GetComponent<LaserAOE>();
        if (laserZone != null)
        {
            SetLaserAOETeam(laserZone, 0);
        }

        var explosionZone = obj.GetComponent<DelayedExplosionDamage>();
        if (explosionZone != null)
        {
            SetExplosionTeam(explosionZone, 0);
        }
    }

    private void SetFreezeZoneTeam(FreezeZone zone, int team)
    {
        var field = zone.GetType().GetField("teamID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(zone, team);
        }
    }

    private void SetLaserAOETeam(LaserAOE laser, int team)
    {
        var field = laser.GetType().GetField("teamID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(laser, team);
        }
    }

    private void SetExplosionTeam(DelayedExplosionDamage explosion, int team)
    {
        var field = explosion.GetType().GetField("teamID", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(explosion, team);
        }
    }

    private float CalculateThinkDelay(BattlefieldAnalyzer.BattlefieldState state)
    {
        switch (state.threatLevel)
        {
            case BattlefieldAnalyzer.ThreatLevel.Critical:
                return aggressiveThinkDelay;

            case BattlefieldAnalyzer.ThreatLevel.High:
                return minThinkDelay;

            case BattlefieldAnalyzer.ThreatLevel.Medium:
                return (minThinkDelay + maxThinkDelay) * 0.5f;

            default:
                return Random.Range(minThinkDelay, maxThinkDelay);
        }
    }

    private void SpendElixir(int amount)
    {
        currentElixir = Mathf.Max(0, currentElixir - amount);
    }

    public int GetCurrentElixir() => currentElixir;

    public AICardSelector.AIStrategy GetCurrentStrategy() => currentStrategy;

    private void OnDestroy()
    {
        isRunning = false;
    }

    private void OnValidate()
    {
        if (aiDeck.Count == 0)
        {
            Debug.LogWarning("SmartAIOpponent: AI Deck is empty! Add cards to the deck.");
        }
    }
}
