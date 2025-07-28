using UnityEngine;

public class TowerDamageDetector : MonoBehaviour
{
    private HealthTower healthTower;

    private void Awake()
    {
        healthTower = GetComponentInParent<HealthTower>();
        if (healthTower == null)
        {
            Debug.LogError("[DamageDetector] Не найден PlayerHealth в родителе!");
        }
    }

    public void OnDamageDetected(int damageAmount)
    {
        Debug.Log($"[DamageDetector] Получен урон: {damageAmount}");
        healthTower?.Tower_ApplyDamage(damageAmount);
    }
}
