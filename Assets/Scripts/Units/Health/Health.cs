using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int CurrentHealth;

    [SerializeField] private UnitStateManager stateManager;
    [SerializeField] private int teamID;

    private HealthBar healthBar;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        healthBar = GetComponentInChildren<HealthBar>();
    }

    public int GetTeam() => teamID;
    public void SetTeam(int id) => teamID = id;

    public void ApplyDamage(int damage, string damageSource)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth -= damage;
        Debug.Log($"{gameObject.name} получил {damage} урона от {damageSource}. Здоровье: {CurrentHealth}");

        if (healthBar != null)
            healthBar.UpdateHealthBar();

        if (CurrentHealth <= 0)
        {
            if (healthBar != null)
                healthBar.OnUnitDeath();

            if (stateManager != null)
                stateManager.OnUnitDie();
        }
    }

    public int GetMaxHealth() => maxHealth;
}