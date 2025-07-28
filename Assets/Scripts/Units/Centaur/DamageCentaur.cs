using UnityEngine;

public class DamageCentaur : MonoBehaviour
{
    private bool isSpecialAttack = false;

    public int specialAttackDamage = 400;
    public int normalAttackDamage = 200;
    public void SetSpecialAttack(bool value)
    {
        isSpecialAttack = value;
    }
    private void OnTriggerEnter(Collider other)
    {

        if (other.TryGetComponent<TowerDamageDetector>(out TowerDamageDetector detector))
        {
            if (isSpecialAttack)
            {
                detector.OnDamageDetected(specialAttackDamage);
            }
            detector.OnDamageDetected(normalAttackDamage);
        }
    }
}
