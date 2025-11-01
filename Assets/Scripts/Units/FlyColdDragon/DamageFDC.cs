using UnityEngine;

public class DamageFDC : MonoBehaviour
{
    private bool hasDealtDamageThisAttack = false;
    private StateManagerFlyColdDragon manager;

    public int damageAmount = 50;

    // 0 = синие, 1 = красные
    [SerializeField] private int teamID;

    private void Awake()
    {
        manager = GetComponentInParent<StateManagerFlyColdDragon>();
    }
    public void SetTeam(int team)
    {
        teamID = team;
    }
    public int GetTeam()
    {
        return teamID;
    }
    public void DFC_ResetDamageFromAnimation()
    {
        hasDealtDamageThisAttack = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasDealtDamageThisAttack) return;
        if (manager == null || manager.dragFlyCold_target == null) return;
        if (other.gameObject != manager.dragFlyCold_target.gameObject) return;
        if (other.TryGetComponent<HealthTower>(out HealthTower tower))
        {
            if (tower.GetTeam() != teamID)
            {
                tower.OnDamageDetected(damageAmount);
                hasDealtDamageThisAttack = true;
            }
        }
        if (other.TryGetComponent<Health>(out Health enemyHealth))
        {
            if (enemyHealth.GetTeam() != teamID)
            {
                enemyHealth.ApplyDamage(damageAmount, "враг-дракон");
                hasDealtDamageThisAttack = true;
            }
        }
    }
}
