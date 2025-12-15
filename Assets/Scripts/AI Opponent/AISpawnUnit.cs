using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<UnitCost> prefabsToSpawn = new List<UnitCost>(); // ������� ��� ������
    public Vector3 spawnAreaCenter;     // ����� ���� ������
    public Vector3 spawnAreaSize;       // ������� ����
    public float minSpawnDelay = 1f;    // ����������� ��������
    public float maxSpawnDelay = 5f;    // ������������ ��������
    public int maxObjects = 50;         // ����� ��������
    public int teamID = 0; // 0 = �����, 1 = �������

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

            // 1. ����� ���������� UnitCost
            UnitCost unitData = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Count)];

            if (unitData.prefabs == null || unitData.prefabs.Length == 0)
            {
                Debug.LogError($"UnitData '{unitData.unitName}' has no prefabs!");
                continue;
            }

            // 1. ���� ����� ����� ������ ��� ���� ������
            Vector3 groupCenter = GetRandomPosition();

            // 2. ������� ���� ������ �� UnitCost, �� ����� ���� � ������
            foreach (var prefab in unitData.prefabs)
            {
                if (prefab == null) continue;

                // ��������� �������� (0.5�1.5 �����)
                Vector3 offset = Random.insideUnitSphere * 1.2f;
                offset.y = 0f; // ����� �� ������� �����/����

                Vector3 spawnPos = groupCenter + offset;

                // Для летающих юнитов добавляем вертикальное смещение вверх
                if (unitData.isFlying)
                    spawnPos += Vector3.up * 3f;

                GameObject newUnit = Instantiate(prefab, spawnPos, Quaternion.identity);

                SetupNewUnit(newUnit);
                spawnedObjects.Add(newUnit);
            }


        }
    }
    private void SetupNewUnit(GameObject unit)
    {
        unit.SetActive(true);

        // ====== ���������� ������� � Health ======
        if (unit.TryGetComponent<Health>(out Health health))
        {
            health.SetTeam(teamID);
        }

        // ====== ���������� ������� �� ���� DamageCentaur (��� ������ ���������) ======
        var damagers = unit.GetComponentsInChildren<DamageCentaur>();
        foreach (var dmg in damagers)
        {
            dmg.SetTeam(teamID);
        }

        // ====== ���������� ������� � UnitStateManager (���� �����) ======
        if (unit.TryGetComponent<UnitStateManager>(out var stateManager))
        {
            stateManager.teamID = teamID; // ���� � ��� ���� ����� ����������
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