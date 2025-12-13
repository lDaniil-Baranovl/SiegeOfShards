using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattlefieldAnalyzer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private int aiTeamID = 1;
    [SerializeField] private int playerTeamID = 0;

    [Header("Auto-Find Towers by Layer")]
    [SerializeField] private string playerTowerLayerName = "TowerPlayer";
    [SerializeField] private string aiTowerLayerName = "TowerEnemy";

    [Header("Zone Settings")]
    [SerializeField] private float dangerZoneRadius = 15f;
    [SerializeField] private float midFieldRadius = 25f;

    private List<Transform> aiTowers = new List<Transform>();
    private List<Transform> playerTowers = new List<Transform>();
    private List<Health> playerUnits = new List<Health>();
    private List<Health> aiUnits = new List<Health>();
    private List<Health> playerFlyingUnits = new List<Health>();
    private List<Health> playerGroundUnits = new List<Health>();

    public enum ThreatLevel
    {
        None,
        Low,
        Medium,
        High,
        Critical
    }

    public class BattlefieldState
    {
        public ThreatLevel threatLevel;
        public int playerUnitsCount;
        public int aiUnitsCount;
        public int playerFlyingCount;
        public int playerGroundCount;
        public Vector3 playerAttackCenter;
        public bool playerIsAttacking;
        public bool playerIsPassive;
        public int playerUnitsNearTower;
        public float timeSinceLastPlayerAction;
        public Transform mostThreatenedTower;
        public bool hasFlyingThreat;
    }

    private float lastPlayerActionTime;
    private int lastPlayerUnitCount;

    private void Start()
    {
        FindTowersByLayer();
        lastPlayerActionTime = Time.time;
    }

    private void FindTowersByLayer()
    {
        aiTowers.Clear();
        playerTowers.Clear();

        int playerLayer = LayerMask.NameToLayer(playerTowerLayerName);
        int aiLayer = LayerMask.NameToLayer(aiTowerLayerName);

        if (playerLayer == -1)
        {
            Debug.LogError($"BattlefieldAnalyzer: Layer '{playerTowerLayerName}' not found! Create it in Unity.");
            return;
        }

        if (aiLayer == -1)
        {
            Debug.LogError($"BattlefieldAnalyzer: Layer '{aiTowerLayerName}' not found! Create it in Unity.");
            return;
        }

        HealthTower[] allTowers = FindObjectsByType<HealthTower>(FindObjectsSortMode.None);

        foreach (var tower in allTowers)
        {
            if (tower.gameObject.layer == playerLayer)
            {
                playerTowers.Add(tower.transform);
                Debug.Log($"[BattlefieldAnalyzer] Found Player Tower: {tower.name}");
            }
            else if (tower.gameObject.layer == aiLayer)
            {
                aiTowers.Add(tower.transform);
                Debug.Log($"[BattlefieldAnalyzer] Found AI Tower: {tower.name}");
            }
        }

        if (aiTowers.Count == 0)
        {
            Debug.LogWarning($"BattlefieldAnalyzer: No AI towers found on layer '{aiTowerLayerName}'!");
        }

        if (playerTowers.Count == 0)
        {
            Debug.LogWarning($"BattlefieldAnalyzer: No player towers found on layer '{playerTowerLayerName}'!");
        }

        Debug.Log($"[BattlefieldAnalyzer] Total: {aiTowers.Count} AI towers, {playerTowers.Count} Player towers");
    }

    public BattlefieldState AnalyzeBattlefield()
    {
        UpdateUnitLists();

        BattlefieldState state = new BattlefieldState
        {
            playerUnitsCount = playerUnits.Count,
            aiUnitsCount = aiUnits.Count,
            playerFlyingCount = playerFlyingUnits.Count,
            playerGroundCount = playerGroundUnits.Count,
            playerIsAttacking = false,
            playerIsPassive = false,
            playerUnitsNearTower = 0,
            timeSinceLastPlayerAction = Time.time - lastPlayerActionTime,
            hasFlyingThreat = playerFlyingUnits.Count > 0
        };

        if (playerUnits.Count > lastPlayerUnitCount)
        {
            lastPlayerActionTime = Time.time;
        }
        lastPlayerUnitCount = playerUnits.Count;

        if (playerUnits.Count == 0)
        {
            state.playerIsPassive = true;
            state.threatLevel = ThreatLevel.None;
            return state;
        }

        state.playerAttackCenter = CalculateAveragePosition(playerUnits);

        state.mostThreatenedTower = GetMostThreatenedTower();
        if (state.mostThreatenedTower != null)
        {
            state.playerUnitsNearTower = CountUnitsNearPosition(playerUnits, state.mostThreatenedTower.position, dangerZoneRadius);
        }

        if (state.playerUnitsNearTower > 0)
        {
            state.playerIsAttacking = true;
        }

        state.threatLevel = CalculateThreatLevel(state);

        return state;
    }

    private void UpdateUnitLists()
    {
        playerUnits.Clear();
        aiUnits.Clear();
        playerFlyingUnits.Clear();
        playerGroundUnits.Clear();

        Health[] allUnits = FindObjectsByType<Health>(FindObjectsSortMode.None);

        foreach (Health unit in allUnits)
        {
            if (unit == null || unit.IsDead) continue;

            if (unit.GetTeam() == playerTeamID)
            {
                playerUnits.Add(unit);

                if (unit.CanFly)
                {
                    playerFlyingUnits.Add(unit);
                }
                else
                {
                    playerGroundUnits.Add(unit);
                }
            }
            else if (unit.GetTeam() == aiTeamID)
            {
                aiUnits.Add(unit);
            }
        }
    }

    private ThreatLevel CalculateThreatLevel(BattlefieldState state)
    {
        if (state.playerUnitsNearTower >= 5)
            return ThreatLevel.Critical;

        if (state.playerUnitsNearTower >= 3)
            return ThreatLevel.High;

        if (state.playerUnitsNearTower >= 1)
            return ThreatLevel.Medium;

        Transform closestTower = GetClosestAliveTower();
        if (closestTower != null)
        {
            float distanceToTower = Vector3.Distance(state.playerAttackCenter, closestTower.position);

            if (distanceToTower < midFieldRadius && state.playerUnitsCount >= 3)
                return ThreatLevel.Medium;
        }

        if (state.playerUnitsCount >= 2)
            return ThreatLevel.Low;

        return ThreatLevel.None;
    }

    private Vector3 CalculateAveragePosition(List<Health> units)
    {
        if (units.Count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;
        foreach (var unit in units)
        {
            if (unit != null)
                sum += unit.transform.position;
        }

        return sum / units.Count;
    }

    private int CountUnitsNearPosition(List<Health> units, Vector3 position, float radius)
    {
        int count = 0;
        foreach (var unit in units)
        {
            if (unit != null && Vector3.Distance(unit.transform.position, position) <= radius)
            {
                count++;
            }
        }
        return count;
    }

    private Transform GetMostThreatenedTower()
    {
        if (aiTowers.Count == 0) return null;

        Transform mostThreatened = aiTowers[0];
        int maxThreat = 0;

        foreach (var tower in aiTowers)
        {
            if (tower == null) continue;

            int nearbyEnemies = CountUnitsNearPosition(playerUnits, tower.position, dangerZoneRadius);

            if (nearbyEnemies > maxThreat)
            {
                maxThreat = nearbyEnemies;
                mostThreatened = tower;
            }
        }

        return mostThreatened;
    }

    private Transform GetClosestAliveTower()
    {
        if (aiTowers.Count == 0) return null;

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var tower in aiTowers)
        {
            if (tower == null) continue;

            HealthTower towerHealth = tower.GetComponent<HealthTower>();
            if (towerHealth != null && towerHealth.tower_currentHealth <= 0)
                continue;

            if (closest == null)
            {
                closest = tower;
                minDist = 0;
            }
        }

        return closest != null ? closest : (aiTowers.Count > 0 ? aiTowers[0] : null);
    }

    public Vector3 GetBestDefensiveSpawnPoint()
    {
        Transform threatenedTower = GetMostThreatenedTower();
        if (threatenedTower == null)
            threatenedTower = GetClosestAliveTower();

        if (threatenedTower == null)
            return Vector3.zero;

        if (playerUnits.Count == 0)
            return GetRandomPointNearTower(threatenedTower.position, 5f);

        Vector3 playerCenter = CalculateAveragePosition(playerUnits);
        Vector3 directionToPlayer = (playerCenter - threatenedTower.position).normalized;

        Vector3 defensivePoint = threatenedTower.position + directionToPlayer * 8f;
        defensivePoint.y = threatenedTower.position.y;

        return defensivePoint;
    }

    public Vector3 GetBestOffensiveSpawnPoint()
    {
        Transform myTower = GetClosestAliveTower();
        if (myTower == null || playerTowers.Count == 0)
            return Vector3.zero;

        Transform targetTower = playerTowers[0];
        Vector3 directionToEnemy = (targetTower.position - myTower.position).normalized;

        Vector3 offensivePoint = myTower.position + directionToEnemy * 12f;
        offensivePoint.y = myTower.position.y;

        return offensivePoint;
    }

    public Vector3 GetBestSpellPosition(SpellType spellType)
    {
        if (playerUnits.Count == 0)
            return Vector3.zero;

        switch (spellType)
        {
            case SpellType.Damage:
            case SpellType.Freeze:
                return FindBestClusterPosition(playerUnits);

            case SpellType.Heal:
                return FindBestClusterPosition(aiUnits);

            default:
                return CalculateAveragePosition(playerUnits);
        }
    }

    private Vector3 FindBestClusterPosition(List<Health> units)
    {
        if (units.Count == 0)
            return Vector3.zero;

        Vector3 bestPos = units[0].transform.position;
        int maxNearby = 0;

        foreach (var unit in units)
        {
            if (unit == null) continue;

            int nearbyCount = CountUnitsNearPosition(units, unit.transform.position, 5f);

            if (nearbyCount > maxNearby)
            {
                maxNearby = nearbyCount;
                bestPos = unit.transform.position;
            }
        }

        return bestPos;
    }

    private Vector3 GetRandomPointNearTower(Vector3 towerPos, float radius)
    {
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        return new Vector3(towerPos.x + randomCircle.x, towerPos.y, towerPos.z + randomCircle.y);
    }

    public List<Health> GetPlayerUnits() => playerUnits;
    public List<Health> GetAIUnits() => aiUnits;
    public List<Health> GetPlayerFlyingUnits() => playerFlyingUnits;
    public List<Health> GetPlayerGroundUnits() => playerGroundUnits;

    private void OnDrawGizmosSelected()
    {
        Transform mainTower = GetClosestAliveTower();
        if (mainTower != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(mainTower.position, dangerZoneRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(mainTower.position, midFieldRadius);
        }

        foreach (var tower in aiTowers)
        {
            if (tower != null)
            {
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
                Gizmos.DrawWireSphere(tower.position, dangerZoneRadius * 0.5f);
            }
        }
    }
}
