using UnityEngine;

public class HealthCen : MonoBehaviour
{
    public int cen_health = 100;
    public CentaurStateManager cen_stateManager;

    public void Centaur_ApplyDamage(int damage)
    {
        if (cen_health <= 0) return;

        cen_health -= damage;
        if (cen_health <= 0 && cen_stateManager != null)
        {
            cen_stateManager.Centaur_Die();
        }
    }
}