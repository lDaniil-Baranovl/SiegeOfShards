using UnityEngine;

public class DamageCentaur : MonoBehaviour
{
    private bool isSpecialAttack = false;
    private bool hasDealtDamageThisAttack = false;
    private float lastSpecialAttackTime = -999f;
    private CentaurStateManager manager;

    public int specialAttackDamage = 400;
    public int normalAttackDamage = 200;

    // 0 = синие, 1 = красные
    [SerializeField] private int teamID;

    private void Awake()
    {
        manager = GetComponentInParent<CentaurStateManager>();
    }
    public void SetTeam(int team)
    {
        teamID = team;
    }
    public int GetTeam()
    {
        return teamID;
    }
    public void SetSpecialAttack(bool value)
    {
        if (value && CanUseSpecialAttack())
        {
            isSpecialAttack = true;
            lastSpecialAttackTime = manager != null ? manager.centaur_runTime : Time.time;
        }
    }
    private bool CanUseSpecialAttack()
    {
        if (manager == null) return true;
        return manager.centaur_runTime - lastSpecialAttackTime >= 3f;
    }
    public void CEN_ResetDamageFromAnimation()
    {
        hasDealtDamageThisAttack = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasDealtDamageThisAttack) return;

        int damageAmount = isSpecialAttack ? specialAttackDamage : normalAttackDamage;

        if (other.TryGetComponent<HealthTower>(out HealthTower tower))
        {
            if (tower.GetTeam() != teamID)
            {
                tower.OnDamageDetected(damageAmount);
                hasDealtDamageThisAttack = true;
            }
            return;
        }
        if (other.TryGetComponent<HealthCen>(out HealthCen enemyHealth))
        {
            if (enemyHealth.GetTeam() != teamID)
            {
                enemyHealth.Centaur_ApplyDamage(damageAmount, "враг-кентавр");
                hasDealtDamageThisAttack = true;
            }
        }
        if (isSpecialAttack && hasDealtDamageThisAttack)
        {
            isSpecialAttack = false;
        }
    }
}