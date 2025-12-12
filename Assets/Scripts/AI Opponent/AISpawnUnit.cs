using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<UnitCost> prefabsToSpawn = new List<UnitCost>(); // Префабы для спавна
    public Vector3 spawnAreaCenter;     // Центр зоны спавна
    public Vector3 spawnAreaSize;       // Размеры зоны
    public float minSpawnDelay = 1f;    // Минимальная задержка
    public float maxSpawnDelay = 5f;    // Максимальная задержка
    public int maxObjects = 50;         // Лимит объектов
    public int teamID = 0; // 0 = синие, 1 = красные

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private bool isSpawning = true;

    private void Start()
    {
        StartCoroutine(SpawnObjects());
    }

    private IEnumerator SpawnObjects()
    {
        while (isSpawning)
        {
            if (spawnedObjects.Count >= maxObjects)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            if (prefabsToSpawn.Count == 0)
            {
                Debug.LogError("No UnitCost objects assigned!");
                yield break;
            }

            // 1. Выбор случайного UnitCost
            UnitCost unitData = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Count)];

            if (unitData.prefabs == null || unitData.prefabs.Length == 0)
            {
                Debug.LogError($"UnitData '{unitData.unitName}' has no prefabs!");
                continue;
            }

            // 1. Одна общая точка спавна для всей группы
            Vector3 groupCenter = GetRandomPosition();

            // 2. Спавним всех юнитов из UnitCost, но рядом друг с другом
            foreach (var prefab in unitData.prefabs)
            {
                if (prefab == null) continue;

                // Небольшое смещение (0.5–1.5 метра)
                Vector3 offset = Random.insideUnitSphere * 1.2f;
                offset.y = 0f; // чтобы не улетали вверх/вниз

                Vector3 spawnPos = groupCenter + offset;

                GameObject newUnit = Instantiate(prefab, spawnPos, Quaternion.identity);

                SetupNewUnit(newUnit);
                spawnedObjects.Add(newUnit);
            }


        }
    }
    private void SetupNewUnit(GameObject unit)
    {
        unit.SetActive(true);

        // ====== Установить команду в Health ======
        if (unit.TryGetComponent<Health>(out Health health))
        {
            health.SetTeam(teamID);
        }

        // ====== Установить команду во всех DamageCentaur (или других дамагерах) ======
        var damagers = unit.GetComponentsInChildren<DamageCentaur>();
        foreach (var dmg in damagers)
        {
            dmg.SetTeam(teamID);
        }

        // ====== Установить команду в UnitStateManager (если нужно) ======
        if (unit.TryGetComponent<UnitStateManager>(out var stateManager))
        {
            stateManager.teamID = teamID; // если у вас есть такая переменная
        }
    }


    private Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, spawnAreaCenter.x + spawnAreaSize.x / 2),
            Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2, spawnAreaCenter.y + spawnAreaSize.y / 2),
            Random.Range(spawnAreaCenter.z - spawnAreaSize.z / 2, spawnAreaCenter.z + spawnAreaSize.z / 2)
        );
    }

    private void CleanupDestroyedObjects()
    {
        spawnedObjects.RemoveAll(obj => obj == null);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }
}