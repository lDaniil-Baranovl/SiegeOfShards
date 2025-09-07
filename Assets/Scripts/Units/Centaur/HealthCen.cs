using UnityEngine;

public class HealthCen : MonoBehaviour
{
    [Header("Настройки здоровья юнита")]
    public int centaur_maxHeath = 100;
    public int centaur_currentHeath = 0;
    private bool centaur_isDead = false;
    private CentaurStateManager centaurStateManager;
    void Start()
    {
        centaur_currentHeath = centaur_maxHeath;
        centaurStateManager = GetComponent<CentaurStateManager>();
    }
    void Update(){}
    public void Centaur_ApplyDamage(int damageAmount)
    {
        if (centaur_isDead) return;

        centaur_currentHeath -= damageAmount;
        centaur_currentHeath = Mathf.Clamp(centaur_currentHeath, 0, centaur_maxHeath);

        if (centaur_currentHeath <= 0)
        {
            centaur_isDead=true;
        }
    }
    private void Centaur_Die()
    {
        if (centaur_isDead) return;

        centaur_isDead = true;

        if (centaurStateManager != null)
        {
            DeathCentaur deathState = new DeathCentaur();
            centaurStateManager.SwitchState(deathState);
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }
}
