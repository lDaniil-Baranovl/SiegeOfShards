using UnityEngine;

public class DamageCentaur : MonoBehaviour
{
    public int normalAttackDamage = 200;
    public int specialAttackDamage = 400;
    public float damageCooldown = 0.5f;

    private bool canDamage = false;
    private bool damageApplied = false;
    private float damageTimer = 0f;

    private bool isSpecialAttack = false;

    [SerializeField] private Collider damageCollider; // <-- обязательно установить в инспекторе

    private void Awake()
    {
        if (damageCollider != null)
            damageCollider.enabled = false;
    }

    private void Update()
    {
        if (canDamage && damageApplied)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageCooldown)
            {
                damageApplied = false;
                damageTimer = 0f;
            }
        }
    }

    public void StartAttackWindowCen()
    {
        Debug.Log("StartAttackWindowCen");
        canDamage = true;
        damageApplied = false;
        if (damageCollider != null)
            damageCollider.enabled = true;
    }

    public void EndAttackWindowCen()
    {
        Debug.Log("EndAttackWindowCen");
        canDamage = false;
        damageApplied = false;
        damageTimer = 0f;
        isSpecialAttack = false;
        if (damageCollider != null)
            damageCollider.enabled = false;
    }

    public void SetSpecialAttack(bool value)
    {
        isSpecialAttack = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Триггер сработал с объектом");
        if (!canDamage || damageApplied) return;

        TowerHealth tower = other.GetComponent<TowerHealth>();
        if (tower != null)
        {
            Debug.Log($"Нанесен урон башне");
            int damage = isSpecialAttack ? specialAttackDamage : normalAttackDamage;
            tower.Tower_ApplyDamage(damage);
            damageApplied = true;
        }
    }
}