using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Health healthComponent;
    [SerializeField] private Canvas healthCanvas;

    private Transform cameraTransform;

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

        if (healthCanvas != null)
            healthCanvas.enabled = true;

        if (Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (cameraTransform == null || healthCanvas == null)
            return;

        Vector3 directionToCamera = healthCanvas.transform.position - cameraTransform.position;
        directionToCamera.y = 0f;

        if (directionToCamera.sqrMagnitude > 0.0001f)
            healthCanvas.transform.rotation = Quaternion.LookRotation(directionToCamera);
    }

    public void UpdateHealthBar()
    {
        if (healthSlider != null && healthComponent != null)
        {
            healthSlider.value = healthComponent.CurrentHealth;

            if (healthComponent.CurrentHealth <= 0 && healthCanvas != null)
                healthCanvas.enabled = false;
        }
    }

    public void OnUnitDeath()
    {
        if (healthCanvas != null)
            healthCanvas.enabled = false;
    }
}