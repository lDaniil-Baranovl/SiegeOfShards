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

    [Header("AI Deck (8 cards total)")]
    [SerializeField] private List<UnitCost> aiDeck = new List<UnitCost>();

    [Header("Card Cycle System (like Clash Royale)")]
    [SerializeField] private int handSize = 4;

    [Header("Spawn Zone (like RandomSpawner)")]
    [SerializeField] private Vector3 spawnAreaCenter = Vector3.zero;
    [SerializeField] private Vector3 spawnAreaSize = new Vector3(10f, 0f, 10f);
    [SerializeField] private bool useSpawnArea = true;

    private int currentElixir;
    private bool isRunning = true;
    private AICardSelector.AIStrategy currentStrategy;

    private Queue<UnitCost> deckQueue = new Queue<UnitCost>();
    private List<UnitCost> currentHand = new List<UnitCost>();

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

        InitializeDeck();

        currentElixir = startingElixir;

        StartCoroutine(ElixirRegeneration());
        StartCoroutine(AIThinkLoop());
    }

    private void InitializeDeck()
    {
        if (aiDeck == null || aiDeck.Count == 0)
        {
            Debug.LogError("SmartAIOpponent: AI Deck is empty!");
            return;
        }

        deckQueue.Clear();
        currentHand.Clear();

        HashSet<string> usedNames = new HashSet<string>();
        foreach (var card in aiDeck)
        {
            if (card == null) continue;

            if (!usedNames.Contains(card.unitName))
            {
                usedNames.Add(card.unitName);
                deckQueue.Enqueue(card);
            }
        }

        for (int i = 0; i < handSize && deckQueue.Count > 0; i++)
        {
            DrawCard();
        }

        Debug.Log($"[AI] Deck initialized: {currentHand.Count} cards in hand, {deckQueue.Count} in queue");
        LogCurrentHand();
    }

    private void DrawCard()
    {
        if (deckQueue.Count == 0)
        {
            foreach (var card in aiDeck)
            {
                if (card != null)
                    deckQueue.Enqueue(card);
            }
        }

        if (deckQueue.Count > 0)
        {
            UnitCost drawnCard = deckQueue.Dequeue();
            currentHand.Add(drawnCard);
            Debug.Log($"[AI] Drew card: {drawnCard.unitName} (Hand: {currentHand.Count}/{handSize})");
        }
    }

    private void OnCardPlayed(UnitCost playedCard)
    {
        currentHand.Remove(playedCard);
        deckQueue.Enqueue(playedCard);
        DrawCard();

        Debug.Log($"[AI] Played: {playedCard.unitName}, Drew next card");
        LogCurrentHand();
    }

    private void LogCurrentHand()
    {
        string handInfo = "[AI Hand] ";
        foreach (var card in currentHand)
        {
            handInfo += $"{card.unitName}({card.elixirCost}), ";
        }
        Debug.Log(handInfo);
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
        if (currentHand.Count == 0)
        {
            Debug.LogWarning("[AI] Hand is empty! Cannot play card.");
            return null;
        }

        int availableElixir = currentElixir;

        if (currentStrategy != AICardSelector.AIStrategy.Defend &&
            state.threatLevel != BattlefieldAnalyzer.ThreatLevel.Critical)
        {
            availableElixir = Mathf.Max(0, currentElixir - elixirReserve);
        }

        if (availableElixir <= 0)
            return null;

        cardSelector.SetElixir(availableElixir);

        AICardSelector.CardChoice choice = cardSelector.SelectBestCard(state, currentStrategy, currentHand);

        if (choice != null && choice.card.elixirCost <= currentElixir)
        {
            SpendElixir(choice.card.elixirCost);
            SpawnCard(choice.card, choice.spawnPosition);
            OnCardPlayed(choice.card);
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

        Vector3 finalPosition = position;

        if (useSpawnArea)
        {
            finalPosition = ClampToSpawnArea(position);
        }

        for (int i = 0; i < card.prefabs.Length; i++)
        {
            if (card.prefabs[i] == null)
                continue;

            Vector3 spawnPos = finalPosition;

            if (card.spawnOffsets != null && i < card.spawnOffsets.Length)
            {
                spawnPos += card.spawnOffsets[i];
            }
            else if (card.prefabs.Length > 1)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 1.5f;
                spawnPos += new Vector3(randomOffset.x, 0, randomOffset.y);
            }

            if (useSpawnArea)
            {
                spawnPos = ClampToSpawnArea(spawnPos);
            }

            GameObject spawnedObject = Instantiate(card.prefabs[i], spawnPos, Quaternion.identity);

            SetupSpawnedObject(spawnedObject);
        }
    }

    private Vector3 ClampToSpawnArea(Vector3 position)
    {
        Vector3 clamped = position;

        float halfSizeX = spawnAreaSize.x / 2f;
        float halfSizeZ = spawnAreaSize.z / 2f;

        clamped.x = Mathf.Clamp(position.x,
            spawnAreaCenter.x - halfSizeX,
            spawnAreaCenter.x + halfSizeX);

        clamped.z = Mathf.Clamp(position.z,
            spawnAreaCenter.z - halfSizeZ,
            spawnAreaCenter.z + halfSizeZ);

        clamped.y = spawnAreaCenter.y;

        return clamped;
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

    public List<UnitCost> GetCurrentHand() => currentHand;

    public int GetHandSize() => handSize;

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

    private void OnDrawGizmosSelected()
    {
        if (useSpawnArea)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
        }
    }
}
