using UnityEngine;

public class DamageCentaur : MonoBehaviour
{
    private bool isSpecialAttack = false;
    private bool hasDealtDamageThisAttack = false;

    public int specialAttackDamage = 400;
    public int normalAttackDamage = 200;

    public void SetSpecialAttack(bool value)
    {
        isSpecialAttack = value;
    }
    public void CEN_ResetDamageFromAnimation()
    {
        hasDealtDamageThisAttack = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasDealtDamageThisAttack) return;

        int damageAmount = isSpecialAttack ? specialAttackDamage : normalAttackDamage;

        if (other.TryGetComponent<TowerDamageDetector>(out TowerDamageDetector detector))
        {
            detector.OnDamageDetected(damageAmount);
            hasDealtDamageThisAttack = true;
        }
        else if (other.TryGetComponent<HealthCen>(out HealthCen enemyHealth))
        {
            enemyHealth.Centaur_ApplyDamage(damageAmount);
            hasDealtDamageThisAttack = true;
        }
    }
}