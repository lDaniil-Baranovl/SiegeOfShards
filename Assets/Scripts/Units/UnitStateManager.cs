using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class UnitStateManager : MonoBehaviour
{
    [Header("Common Components")]
    [SerializeField] public Animator unitAnimator;
    [SerializeField] public NavMeshAgent navMeshAgent;
    [SerializeField] public Collider damageCollider;

    [Header("Combat Settings")]
    [SerializeField] public float walkSpeed;
    [SerializeField] public float agroDistance;
    [SerializeField] public float attackDistance;

    [Header("Targets & Teams")]
    [SerializeField] public List<Transform> enemyTowers = new List<Transform>();
    [SerializeField] public GameObject deathEffect;
    [SerializeField] public int teamID = 0; // 0 = синие, 1 = красные

    [SerializeField] public Transform target;
    [SerializeField] public bool canMove = false;
    [SerializeField] public bool canAttackFlyTarget = false;
    public bool isDead = false;

    public bool isFrozen = false;
    private float defaultSpeed;
    private float defaultAnimatorSpeed = 1f;
    protected virtual void Start()
    {
        if (deathEffect != null)
            deathEffect.SetActive(false);
    }

    protected virtual void Update()
    {
        enemyTowers.RemoveAll(tower => tower == null);
        if (GamePause.paused) return;
    }
    // ================= FREEZE SYSTEM =================
    private float savedSpeed;
    private float savedAnimatorSpeed;

    public virtual void Freeze()
    {
        if (isFrozen) return;
        isFrozen = true;
        if (navMeshAgent != null)
        {
            savedSpeed = navMeshAgent.speed;
            navMeshAgent.speed = 0f;
            navMeshAgent.isStopped = true;
        }

        if (unitAnimator != null)
        {
            savedAnimatorSpeed = unitAnimator.speed;
            unitAnimator.speed = 0f;
        }
        if (damageCollider != null)
            damageCollider.enabled = false;

        if (this is GolStateMan gol)
        {
            gol.attackEffect.SetActive(false);
            gol.isAttackEffectActive = false;
        }
    }

    public virtual void Unfreeze()
    {
        if (!isFrozen) return;
        isFrozen = false;
        if (navMeshAgent != null)
        {
            navMeshAgent.speed = savedSpeed;
            navMeshAgent.isStopped = false;
        }
        if (unitAnimator != null)
            unitAnimator.speed = savedAnimatorSpeed;
    }
    // ========== COMMON METHODS ==========
    public virtual void SetSpeed(float newSpeed)
    {
        if (navMeshAgent != null)
            navMeshAgent.speed = newSpeed;
    }
    public virtual void SetDestination(Transform newDestination)
    {
        if (navMeshAgent == null || newDestination == null) return;
        target = newDestination;

        // »спользуем умную точку дл€ движени€ Ч ближайша€ поверхность цели
        Vector3 targetPoint = GetAttackPosition(newDestination);
        navMeshAgent.SetDestination(targetPoint);
    }
    public virtual Vector3 GetAttackPosition(Transform targetTransform)
    {
        if (targetTransform == null)
            return transform.position;

        Collider targetCollider = targetTransform.GetComponentInChildren<Collider>();
        if (targetCollider == null)
            return targetTransform.position;

        Vector3 closest = targetCollider.ClosestPoint(transform.position);

        Vector3 dir = (transform.position - closest).normalized;
        closest += dir * 0.3f;

        return closest;
    }
    public virtual bool HasReachedTarget()
    {
        if (target == null) return false;
        return Vector3.Distance(transform.position, target.position) <= attackDistance;
    }

    public virtual Transform GetTarget()
    {
        Transform closestEnemy = GetClosestEnemy();
        Transform closestTower = GetClosestTower();

        float enemyDist = closestEnemy != null ? Vector3.Distance(transform.position, closestEnemy.position) : Mathf.Infinity;
        float towerDist = closestTower != null ? Vector3.Distance(transform.position, closestTower.position) : Mathf.Infinity;

        if (enemyDist <= agroDistance && enemyDist < towerDist)
            return closestEnemy;

        return closestTower;
    }

    protected virtual Transform GetClosestEnemy()
    {
        Health[] allUnits = FindObjectsByType<Health>(FindObjectsSortMode.None);
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (Health unit in allUnits)
        {
            if (unit == null || unit.IsDead) continue;
            if (unit == null || unit.gameObject == this.gameObject) continue;
            if (unit.GetTeam() == teamID) continue;
            if (!canAttackFlyTarget && unit.CanFly) continue;

            float dist = Vector3.Distance(transform.position, unit.transform.position);
            if (dist <= agroDistance && dist < minDist)
            {
                minDist = dist;
                closest = unit.transform;
            }
        }
        return closest;
    }

    protected virtual Transform GetClosestTower()
    {
        if (enemyTowers.Count == 0)
        {
            HealthTower[] allTowers = FindObjectsByType<HealthTower>(FindObjectsSortMode.None);
            foreach (HealthTower tower in allTowers)
            {
                if (tower != null && tower.GetTeam() != teamID)
                    enemyTowers.Add(tower.transform);
            }

            if (enemyTowers.Count == 0) return null;
        }

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var tower in enemyTowers)
        {
            if (tower == null) continue;
            float dist = Vector3.Distance(transform.position, tower.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = tower;
            }
        }
        return closest;
    }

    public virtual void SetTeam(int id)
    {
        teamID = id;
    }

    public virtual int GetTeam()
    {
        return teamID;
    }

    public virtual void ToggleDamageCollider(bool state)
    {
        if (damageCollider != null)
            damageCollider.enabled = state;
    }

    public abstract void OnUnitDie();
}