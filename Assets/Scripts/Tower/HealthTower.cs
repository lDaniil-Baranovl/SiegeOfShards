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

    // 0 = синие, 1 = красные
    [SerializeField] private int teamID;
    void Start()
    {
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
        if (tower_currentHealth <= 0)
        {
            Tower_HandleDestruction();
        }
    }
    private void Tower_HandleDestruction()
    {
        if (tower_ruines != null)
        {
            Destroy(tower);
            tower_ruines.SetActive(true);
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