using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
public class StateManagerFlyColdDragon : UnitStateManager
{
    [SerializeField] public Animator dragFlyCold_animator;
    [SerializeField] public NavMeshAgent dragFlyCold_navMeshAgent;
    [SerializeField] public List<Transform> dragFlyCold_tower_enemy = new List<Transform>();
    [SerializeField] public Collider dragFlyCold_damageCollider;

    [SerializeField] public float dragFlyCold_walkSpeed;
    [SerializeField] public float dragFlyCold_agroDistance;
    [SerializeField] public float attackDistance;

    public bool canMove = false;
    public Transform dragFlyCold_target;
    private bool dragFlyCold_isDead = false;

    public bool CanAttackFlyTarget = false;

    FlyColdDragonBaseState currentState;
    public DragFlyColdRunState dragFlyColdRunState = new DragFlyColdRunState();
    public DragFlyColdAttackState dragFlyColdAttackState = new DragFlyColdAttackState();
    public DragFlyColdDeathState dragFlyColdDeathState = new DragFlyColdDeathState();

    [SerializeField] public GameObject effectDieth;
    public int teamID = 0; // 0 = синие, 1 = красные


    //-----------------------------
    [SerializeField] public GameObject AttackEffect;
    private bool isAttackEff = false;

    private void Start()
    {
        StartCoroutine(EnableNavMeshAfterDeath());
        effectDieth.SetActive(false);
        SwitchState(dragFlyColdRunState);
        //------------------------
        AttackEffect.SetActive(false);
    }

    public void SwitchState(FlyColdDragonBaseState newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    private void Update()
    {
        dragFlyCold_tower_enemy.RemoveAll(tower => tower == null);
        currentState?.UpdateState(this);
    }

    public void SetSpeed(float newSpeed)
    {
        dragFlyCold_navMeshAgent.speed = newSpeed;
    }

    public void SetDestination(Transform newDestination)
    {
        dragFlyCold_target = newDestination;
        dragFlyCold_navMeshAgent.SetDestination(newDestination.position);
    }

    public bool HasReachedTarget()
    {
        if (dragFlyCold_target == null) return false;
        return Vector3.Distance(transform.position, dragFlyCold_target.position) <= attackDistance;
    }

    public Transform GetTarget()
    {
        Transform closestEnemy = GetClosestEnemy();
        Transform closestTower = GetClosestTower();

        float enemyDist = closestEnemy != null ? Vector3.Distance(transform.position, closestEnemy.position) : Mathf.Infinity;
        float towerDist = closestTower != null ? Vector3.Distance(transform.position, closestTower.position) : Mathf.Infinity;

        // Если враг ближе башни и в пределах агро-радиуса — атакуем врага
        if (enemyDist <= dragFlyCold_agroDistance && enemyDist < towerDist)
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
            if (dist <= dragFlyCold_agroDistance && dist < minDist)
            {
                minDist = dist;
                closest = unit.transform;
            }
        }
        return closest;
    }

    private Transform GetClosestTower()
    {
        if (dragFlyCold_tower_enemy.Count == 0)
        {
            HealthTower[] allTowers = FindObjectsByType<HealthTower>(FindObjectsSortMode.None);
            foreach (HealthTower tower in allTowers)
            {
                if (tower != null && tower.GetTeam() != teamID)
                {
                    dragFlyCold_tower_enemy.Add(tower.transform);
                }
            }

            if (dragFlyCold_tower_enemy.Count == 0) return null;
        }

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var tower in dragFlyCold_tower_enemy)
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

    void OnOffDamagerFDC(int isOff)
    {
        if (isOff == 0 || isAttackEff == true)
        {
            dragFlyCold_damageCollider.enabled = false;
            AttackEffect.SetActive(false);
            isAttackEff = false;
        }
        else
        {
            dragFlyCold_damageCollider.enabled = true;
            AttackEffect.SetActive(true);
            isAttackEff = true;
        }
    }
    public override void OnUnitDie()
    {
        SwitchState(dragFlyColdDeathState);
    }

    public void FLCAnimationEvent_ResetDamage()
    {
        if (dragFlyCold_damageCollider != null)
        {
            DamageFDC damageScript = dragFlyCold_damageCollider.GetComponent<DamageFDC>();
            if (damageScript != null)
            {
                damageScript.DFC_ResetDamageFromAnimation();
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
    public IEnumerator EnableNavMeshAfterDeath()
    {
        yield return new WaitForSeconds(1.2f);
        if (dragFlyCold_navMeshAgent != null)
        {
            dragFlyCold_navMeshAgent.enabled = true;
        }
    }
}
