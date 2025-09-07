using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] prefabsToSpawn; // Префабы для спавна
    public Vector3 spawnAreaCenter;     // Центр зоны спавна
    public Vector3 spawnAreaSize;       // Размеры зоны
    public float minSpawnDelay = 1f;    // Минимальная задержка
    public float maxSpawnDelay = 5f;    // Максимальная задержка
    public int maxObjects = 50;         // Лимит объектов
    //public string unitTag = "EnemyUnit"; // Тег для юнитов

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

            if (prefabsToSpawn.Length == 0)
            {
                Debug.LogError("No prefabs assigned!");
                yield break;
            }

            GameObject randomPrefab = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
            Vector3 randomPosition = GetRandomPosition();

            GameObject newUnit = Instantiate(randomPrefab, randomPosition, Quaternion.identity);
            SetupNewUnit(newUnit); // Настройка юнита
            spawnedObjects.Add(newUnit);

            CleanupDestroyedObjects();
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

    // Настройка юнита: тег + активация
    private void SetupNewUnit(GameObject unit)
    {
        //unit.tag = unitTag; // Присваиваем тег
        unit.SetActive(true); // Активируем (если префаб был неактивен)

        // Дополнительно: можно добавить компоненты, например:
        // unit.AddComponent<EnemyAI>();
    }

    private void CleanupDestroyedObjects()
    {
        spawnedObjects.RemoveAll(obj => obj == null);
    }

    // Визуализация зоны спавна (Gizmos)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }
}