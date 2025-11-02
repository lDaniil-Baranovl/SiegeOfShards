using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.AI;

public class CentaurStateManager : UnitStateManager
{
    [SerializeField] public Animator centaur_animator;
    [SerializeField] public NavMeshAgent centaur_navMeshAgent;
    [SerializeField] public List<Transform> centaur_tower_enemy = new List<Transform>();
    [SerializeField] public Collider centaur_damageCollider;

    [SerializeField] public float centaur_walkSpeed;
    [SerializeField] public float centaur_agroDistance;
    [SerializeField] public float attackDistance;

    public bool canMove = false;
    public Transform centaur_target;
    private bool cen_isDead = false;

    public bool CanAttackFlyTarget = false; 

    [HideInInspector] public float centaur_runTime;

    CentaurBaseState currentState;
    public RunCentaurState runCentaurState = new RunCentaurState();
    public AttackCentaurState attackCentaurState = new AttackCentaurState();
    public DeathCentaur deathCentaurState = new DeathCentaur();

    [SerializeField] public GameObject effectDieth;
    public int teamID = 0; // 0 = синие, 1 = красные
    private void Start()
    {
        effectDieth.SetActive(false);
        SwitchState(runCentaurState);
    }

    public void SwitchState(CentaurBaseState newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    private void Update()
    {
        centaur_tower_enemy.RemoveAll(tower => tower == null);
        currentState?.UpdateState(this);
    }

    public void SetSpeed(float newSpeed)
    {
        centaur_navMeshAgent.speed = newSpeed;
    }

    public void SetDestination(Transform newDestination)
    {
        centaur_target = newDestination;
        centaur_navMeshAgent.SetDestination(newDestination.position);
    }

    public bool HasReachedTarget()
    {
        if (centaur_target == null) return false;
        return Vector3.Distance(transform.position, centaur_target.position) <= attackDistance;
    }

    public Transform GetTarget()
    {
        Transform closestEnemy = GetClosestEnemy();
        Transform closestTower = GetClosestTower();

        float enemyDist = closestEnemy != null ? Vector3.Distance(transform.position, closestEnemy.position) : Mathf.Infinity;
        float towerDist = closestTower != null ? Vector3.Distance(transform.position, closestTower.position) : Mathf.Infinity;

        // Если враг ближе башни и в пределах агро-радиуса — атакуем врага
        if (enemyDist <= centaur_agroDistance && enemyDist < towerDist)
            return closestEnemy;
        return closestTower;
    }

    private Transform GetClosestEnemy()
    {
        Health[] allUnits = FindObjectsByType<Health>(FindObjectsSortMode.None);

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (Health unit in allUnits)
        {
            if (unit == null) continue;
            if (unit.gameObject == this.gameObject) continue;
            if (unit.GetTeam() == teamID) continue;

            if (!CanAttackFlyTarget && unit.CanFly) continue;

            float dist = Vector3.Distance(transform.position, unit.transform.position);
            if (dist <= centaur_agroDistance && dist < minDist)
            {
                minDist = dist;
                closest = unit.transform;
            }
        }
        return closest;
    }

    private Transform GetClosestTower()
    {
        if (centaur_tower_enemy.Count == 0)
        {
            HealthTower[] allTowers = FindObjectsByType<HealthTower>(FindObjectsSortMode.None);
            foreach (HealthTower tower in allTowers)
            {
                if (tower != null && tower.GetTeam() != teamID)
                {
                    centaur_tower_enemy.Add(tower.transform);
                }
            }

            if (centaur_tower_enemy.Count == 0) return null;
        }

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var tower in centaur_tower_enemy)
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

    void OnOffDamagerCen(int isOff)
    {
        if (isOff == 0)
        {
            centaur_damageCollider.enabled = false;
        }
        else
        {
            centaur_damageCollider.enabled = true;
        }
    }
    public override void OnUnitDie()
    {
        SwitchState(deathCentaurState);
    }

    public void CenAnimationEvent_ResetDamage()
    {
        if (centaur_damageCollider != null)
        {
            DamageCentaur damageScript = centaur_damageCollider.GetComponent<DamageCentaur>();
            if (damageScript != null)
            {
                damageScript.CEN_ResetDamageFromAnimation();
            }
        }
    }
    public void SetTeam(int id)
    {
        teamID = id;
    }
    public int GetTeam()
    {
        return teamID;
    }
}