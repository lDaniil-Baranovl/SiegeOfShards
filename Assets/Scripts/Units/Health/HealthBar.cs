using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Health healthComponent;
    [SerializeField] private Canvas healthCanvas;
    [SerializeField] private bool alwaysVisible = false;

    private bool hasTakenDamage = false;
    private bool isDead = false;

    private void Start()
    {
        if (healthComponent == null)
            healthComponent = GetComponentInParent<Health>();

        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>();

        if (healthSlider != null && healthComponent != null)
        {
            healthSlider.maxValue = healthComponent.GetMaxHealth();
            healthSlider.value = healthComponent.CurrentHealth;
        }

        if (!alwaysVisible && healthCanvas != null)
            healthCanvas.enabled = false;
    }

    public void UpdateHealthBar()
    {
        if (healthSlider != null && healthComponent != null)
        {
            healthSlider.value = healthComponent.CurrentHealth;

            if (!hasTakenDamage && healthComponent.CurrentHealth < healthSlider.maxValue)
                hasTakenDamage = true;

            if (hasTakenDamage && !isDead && healthCanvas != null)
                healthCanvas.enabled = true;

            if (healthComponent.CurrentHealth <= 0)
            {
                isDead = true;
                if (healthCanvas != null)
                    healthCanvas.enabled = false;
            }
        }
    }

    public void OnUnitDeath()
    {
        isDead = true;
        if (healthCanvas != null)
            healthCanvas.enabled = false;
    }
}