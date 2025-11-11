using UnityEngine;
using UnityEngine.UI;

public class TowerBarHealth : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private HealthTower healthComponent;
    [SerializeField] private Canvas healthCanvas;
    [SerializeField] private bool alwaysVisible = true;
    //private bool hasTakenDamage = false;
    private bool isBroke = false;
    private void Start()
    {
        if (healthComponent == null)
            healthComponent = GetComponentInParent<HealthTower>();

        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>();

        if (healthCanvas == null)
            healthCanvas = GetComponent<Canvas>();

        if (healthSlider != null && healthComponent != null)
        {
            healthSlider.maxValue = healthComponent.tower_maxHealth;
            healthSlider.value = healthComponent.tower_currentHealth;
        }
        if (healthCanvas != null)
            healthCanvas.enabled = true;
    }
    private void Update()
    {
        UpdateHealthBarTower();
    }
    public void UpdateHealthBarTower()
    {
        if (healthSlider != null && healthComponent != null)
        {
            healthSlider.value = healthComponent.tower_currentHealth;
            if (healthCanvas != null)
                healthCanvas.enabled = true;
            if (healthComponent.tower_currentHealth <= 0 && healthCanvas != null)
            {
                healthCanvas.enabled = false;
            }
        }
    }
    public void HideHealthBar()
    {
        if (!alwaysVisible && healthCanvas != null)
            healthCanvas.enabled = false;
    }
    public void OnUnitDeath()
    {
        isBroke = true;
        if (healthCanvas != null)
            healthCanvas.enabled = false;
    }
}