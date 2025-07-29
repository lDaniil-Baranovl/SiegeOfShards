using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] prefabsToSpawn; // Массив префабов для спавна
    public Vector3 spawnAreaCenter;     // Центр зоны спавна
    public Vector3 spawnAreaSize;       // Размеры зоны спавна
    public float minSpawnDelay = 1f;    // Минимальная задержка между спавнами
    public float maxSpawnDelay = 5f;    // Максимальная задержка между спавнами
    public int maxObjects = 50;         // Максимальное количество объектов

    private List<GameObject> spawnedObjects = new List<GameObject>(); // Список активных объектов
    private bool isSpawning = true;     // Флаг для остановки спавна

    private void Start()
    {
        StartCoroutine(SpawnObjects());
    }

    private IEnumerator SpawnObjects()
    {
        while (isSpawning) // Бесконечный цикл (можно остановить через isSpawning = false)
        {
            // Если достигнут лимит объектов — ждем
            if (spawnedObjects.Count >= maxObjects)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Случайная задержка перед спавном
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            yield return new WaitForSeconds(delay);

            // Выбираем случайный префаб
            if (prefabsToSpawn.Length == 0)
            {
                Debug.LogError("No prefabs assigned!");
                yield break;
            }

            GameObject randomPrefab = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];

            // Генерируем случайную позицию
            Vector3 randomPosition = new Vector3(
                Random.Range(spawnAreaCenter.x - spawnAreaSize.x / 2, spawnAreaCenter.x + spawnAreaSize.x / 2),
                Random.Range(spawnAreaCenter.y - spawnAreaSize.y / 2, spawnAreaCenter.y + spawnAreaSize.y / 2),
                Random.Range(spawnAreaCenter.z - spawnAreaSize.z / 2, spawnAreaCenter.z + spawnAreaSize.z / 2)
            );

            // Создаем объект и добавляем в список
            GameObject newObj = Instantiate(randomPrefab, randomPosition, Quaternion.identity);
            spawnedObjects.Add(newObj);

            // Удаляем уничтоженные объекты из списка
            CleanupDestroyedObjects();
        }
    }

    // Очистка списка от уничтоженных объектов
    private void CleanupDestroyedObjects()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAt(i);
            }
        }
    }

    // Визуализация зоны спавна в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(spawnAreaCenter, spawnAreaSize);
    }

    // Остановить спавн (опционально)
    public void StopSpawning()
    {
        isSpawning = false;
    }

    // Удалить все созданные объекты (опционально)
    public void ClearAllObjects()
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();
    }
}