using System.Collections.Generic;
using UnityEngine;

public class DamageFDC : MonoBehaviour
{
    private HashSet<GameObject> damagedTargets = new HashSet<GameObject>();
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
        damagedTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (manager == null) return;
        GameObject target = other.gameObject; 
        if(damagedTargets.Contains(target)) return;
        if (other.TryGetComponent<HealthTower>(out HealthTower tower))
        {
            if (tower.GetTeam() != teamID)
            {
                tower.OnDamageDetected(damageAmount);
                damagedTargets.Add(target);
            }
        }
        if (other.TryGetComponent<Health>(out Health enemyHealth))
        {
            if (enemyHealth.GetTeam() != teamID)
            {
                enemyHealth.ApplyDamage(damageAmount, "враг-дракон");
                damagedTargets.Add(target);
            }
        }
    }
}
