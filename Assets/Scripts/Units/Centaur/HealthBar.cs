using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private HealthCen healthComponent;
    [SerializeField] private Canvas healthCanvas;
    [SerializeField] private bool alwaysVisible = false;
    private bool hasTakenDamage = false;
    private bool isDead = false;
    private void Start()
    {
        if (healthComponent == null)
            healthComponent = GetComponentInParent<HealthCen>();

        if (healthSlider == null)
            healthSlider = GetComponentInChildren<Slider>();
        if (healthSlider != null && healthComponent != null)
        {
            healthSlider.maxValue = healthComponent.cen_health;
            healthSlider.value = healthComponent.cen_health;
        }
        if (!alwaysVisible && healthCanvas != null)
            healthCanvas.enabled = false;
    }

    public void UpdateHealthBar()
    {
        if (healthSlider != null && healthComponent != null)
        {
            healthSlider.value = healthComponent.cen_health;
            if (!hasTakenDamage && healthComponent.cen_health < healthSlider.maxValue)
            {
                hasTakenDamage = true;
            }
            if (hasTakenDamage && !isDead && healthCanvas != null)
            {
                healthCanvas.enabled = true;
            }
            if (healthComponent.cen_health <= 0)
            {
                isDead = true;
                if (healthCanvas != null)
                    healthCanvas.enabled = false;
            }
        }
    }

    public void HideHealthBar()
    {
        if (healthCanvas != null)
            healthCanvas.enabled = false;
    }
    public void OnUnitDeath()
    {
        isDead = true;
        if (healthCanvas != null)
            healthCanvas.enabled = false;
    }
}