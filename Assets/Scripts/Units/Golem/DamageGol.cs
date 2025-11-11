using UnityEngine;

public class DamageGol : MonoBehaviour
{
    private bool hasDealtDamageThisAttack = false;
    private GolStateMan manager;
    public int damageAmount = 15;

    // 0 = синие, 1 = красные
    [SerializeField] private int teamID;

    private void Awake()
    {
        manager = GetComponentInParent<GolStateMan>();
    }
    public void SetTeam(int team)
    {
        teamID = team;
    }
    public int GetTeam()
    {
        return teamID;
    }
    public void Gol_ResetDamageFromAnimation()
    {
        hasDealtDamageThisAttack = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (manager == null || manager.isDead) return;
        if (hasDealtDamageThisAttack) return;
        if (manager == null || manager.target == null) return;
        if (other.gameObject != manager.target.gameObject) return;

        if (other.TryGetComponent<HealthTower>(out HealthTower tower))
        {
            if (tower.GetTeam() != teamID)
            {
                tower.OnDamageDetected(damageAmount);
                hasDealtDamageThisAttack = true;
            }
        }
    }
}
