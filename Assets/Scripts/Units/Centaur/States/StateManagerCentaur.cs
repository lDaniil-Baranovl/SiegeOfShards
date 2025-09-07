using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CentaurStateManager : MonoBehaviour
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

    [HideInInspector] public float centaur_runTime;

    CentaurBaseState currentState;
    public RunCentaurState runCentaurState = new RunCentaurState();
    public AttackCentaurState attackCentaurState = new AttackCentaurState();
    public DeathCentaur deathCentaurState = new DeathCentaur();

    private void Start()
    {
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
        string targetTag = gameObject.CompareTag("EnemyUnit") ? "PeacefulUnit" : "EnemyUnit";

        GameObject[] enemies = GameObject.FindGameObjectsWithTag(targetTag);

        if (enemies == null || enemies.Length == 0)
            return null;

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || enemy == this.gameObject) continue; 

            Transform enemyTransform = enemy.transform;
            if (enemyTransform == null) continue;

            float dist = Vector3.Distance(transform.position, enemyTransform.position);

            if (dist <= centaur_agroDistance && dist < minDist)
            {
                minDist = dist;
                closest = enemyTransform;
            }
        }

        return closest;
    }

    private Transform GetClosestTower()
    {
        if (centaur_tower_enemy.Count == 0) return null;
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var tower in centaur_tower_enemy)
        {
            if(tower == null) continue;
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
}