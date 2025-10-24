using UnityEngine;

public class HealthCen : MonoBehaviour
{
    public int cen_health = 100;
    public CentaurStateManager cen_stateManager;
    // 0 = синие, 1 = красные
    [SerializeField] private int teamID;
    public int GetTeam()
    {
        return teamID;
    }
    public void SetTeam(int id)
    {
        teamID = id;
    }
    public void Centaur_ApplyDamage(int damage, string damageSource)
    {
        if (cen_health <= 0) return;

        cen_health -= damage;
        Debug.Log($"{gameObject.name} получил {damage} урона от {damageSource}. Здоровье: {cen_health}");
        if (cen_health <= 0 && cen_stateManager != null)
        {
            cen_stateManager.Centaur_Die();
        }
    }
}