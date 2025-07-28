using UnityEngine;
using UnityEngine.AI;

public class SpawnerFromZone : MonoBehaviour
{
    [Header("Префаб врага")]
    public GameObject enemyPrefab;

    [Header("Зона спавна (невидимая плоскость)")]
    public Transform allowedZonePlane;

    [Header("Настройки высоты и задержки")]
    public float raycastHeight = 20f; // откуда делается луч вниз
    public float minSpawnInterval = 1f;
    public float maxSpawnInterval = 3f;

    private Vector3 zoneCenter;
    private Vector3 zoneSize;

    private void Start()
    {
        if (allowedZonePlane == null || enemyPrefab == null)
        {
            Debug.LogError(" Укажите зону и префаб!");
            return;
        }

        zoneCenter = allowedZonePlane.position;
        zoneSize = allowedZonePlane.localScale;

        StartCoroutine(SpawnLoop());
    }

    private System.Collections.IEnumerator SpawnLoop()
    {
        while (true)
        {
            TrySpawnEnemy();
            float delay = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(delay);
        }
    }

    private void TrySpawnEnemy()
    {
        Vector3 randomPos = GetRandomPointInZone();

        // Пускаем луч вниз с высоты
        Vector3 rayOrigin = new Vector3(randomPos.x, randomPos.y + raycastHeight, randomPos.z);

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastHeight * 2f))
        {
            // Проверяем, есть ли NavMesh в точке касания
            if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 1.0f, NavMesh.AllAreas))
            {
                Instantiate(enemyPrefab, navHit.position, Quaternion.identity);
                Debug.Log($" Враг заспавнен в точке: {navHit.position}");
            }
            else
            {
                Debug.Log("Поверхность не на NavMesh");
            }
        }
        else
        {
            Debug.Log(" Raycast не попал в поверхность");
        }
    }

    private Vector3 GetRandomPointInZone()
    {
        float zoneWidth = zoneSize.x * 10f; // потому что scale=1 == 10 Unity-единиц
        float zoneDepth = zoneSize.z * 10f;

        float x = Random.Range(-zoneWidth / 2f, zoneWidth / 2f);
        float z = Random.Range(-zoneDepth / 2f, zoneDepth / 2f);

        return new Vector3(zoneCenter.x + x, zoneCenter.y, zoneCenter.z + z);
    }

    private void OnDrawGizmosSelected()
    {
        if (allowedZonePlane == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Vector3 size = new Vector3(allowedZonePlane.localScale.x * 10f, 0.1f, allowedZonePlane.localScale.z * 10f);
        Gizmos.DrawCube(allowedZonePlane.position, size);
    }
}