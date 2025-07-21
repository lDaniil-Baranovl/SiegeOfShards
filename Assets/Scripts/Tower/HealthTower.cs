using UnityEngine;

public class TowerHealth : MonoBehaviour
{
    [Header("Настройки здоровья башни")]
    public int tower_maxHealth = 1000;
    private int tower_currentHealth;

    [Header("Объект Башни")]
    public GameObject tower;
    [Header("Пост - Башни")]
    public GameObject tower_ruines;

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

        //Debug.Log($"Башня {gameObject.name} получила {damageAmount} урона. Осталось здоровья: {tower_currentHealth}");

        if (tower_currentHealth <= 0)
        {
            Tower_HandleDestruction();
        }
    }
    private void Tower_HandleDestruction()
    {
        //Debug.Log($"Башня {gameObject.name} уничтожена.");

        if (tower_ruines != null)
        {
            Destroy(tower);
            tower_ruines.SetActive(true);
        }
    }
    public int Tower_GetCurrentHealth()
    {
        return tower_currentHealth;
    }
    public int Tower_GetMaxHealth()
    {
        return tower_maxHealth;
    }
    public void Tower_Destroy()
    {
        tower_currentHealth = 0;
        Tower_HandleDestruction();
    }
}