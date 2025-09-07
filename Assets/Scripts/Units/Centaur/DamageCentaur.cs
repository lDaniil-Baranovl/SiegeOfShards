using UnityEngine;
using System.Collections;

public class DamageCentaur : MonoBehaviour
{
    private bool isSpecialAttack = false;
    private float lastDamageTime = 0f; 
    private float damageCooldown = 0.5f; 

    public int specialAttackDamage = 400;
    public int normalAttackDamage = 200;

    public void SetSpecialAttack(bool value)
    {
        isSpecialAttack = value;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (Time.time - lastDamageTime < damageCooldown) return;

        int damageAmount = isSpecialAttack ? specialAttackDamage : normalAttackDamage;

        if (other.TryGetComponent<TowerDamageDetector>(out TowerDamageDetector detector))
        {
            detector.OnDamageDetected(damageAmount);
            lastDamageTime = Time.time; 
        }

        if (other.TryGetComponent<HealthCen>(out HealthCen enemyHealth))
        {
            enemyHealth.Centaur_ApplyDamage(damageAmount);
            lastDamageTime = Time.time;
        }
    }

    // Для отладки в инспекторе
    public float GetTimeSinceLastDamage() => Time.time - lastDamageTime;
}