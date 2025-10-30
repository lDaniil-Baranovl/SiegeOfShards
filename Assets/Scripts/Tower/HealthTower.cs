using UnityEngine;

public class HealthTower : MonoBehaviour
{
    [Header("Настройки здоровья башни")]
    public int tower_maxHealth = 1000;
    public int tower_currentHealth;

    [Header("Объект Башни")]
    public GameObject tower;
    [Header("Пост - Башни")]
    public GameObject tower_ruines;
    [SerializeField] public GameObject effectBroken1;
    [SerializeField] public GameObject effectBroken2;

    // 0 = синие, 1 = красные
    [SerializeField] private int teamID;
    void Start()
    {
        if (effectBroken1 != null && effectBroken2 != null)
        {
            effectBroken1.SetActive(false);
            effectBroken2.SetActive(false);
        }
        tower_currentHealth = tower_maxHealth;
        tower_ruines.SetActive(false);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Tower_Destroy();
        }
    }
    public void Tower_ApplyDamage(int damageAmount)
    {
        tower_currentHealth -= damageAmount;
        tower_currentHealth = Mathf.Clamp(tower_currentHealth, 0, tower_maxHealth);
        Debug.Log($"Башня получила урон {damageAmount} Текущее здороье {tower_currentHealth}");
        TowerBarHealth healthBar = GetComponentInChildren<TowerBarHealth>();
        if (healthBar != null)
            healthBar.UpdateHealthBarTower();
        if (tower_currentHealth <= 0)
        {
            Tower_HandleDestruction();
        }
    }
    private void Tower_HandleDestruction()
    {
        if (tower_ruines != null && effectBroken1 != null && effectBroken2 != null)
        {
            Destroy(tower);
            tower_ruines.SetActive(true);
            effectBroken1.SetActive(true);
            effectBroken2.SetActive(true);
        }
    }
    public void Tower_Destroy()
    {
        tower_currentHealth = 0;
        Tower_HandleDestruction();
    }
    public void OnDamageDetected(int damageAmount)
    {
        Debug.Log($"[DamageDetector] Получен урон: {damageAmount}");
        Tower_ApplyDamage(damageAmount);
    }
    public int GetTeam()
    {
        return teamID;
    }
    public void SetTeam(int id)
    {
        teamID = id;
    }
}