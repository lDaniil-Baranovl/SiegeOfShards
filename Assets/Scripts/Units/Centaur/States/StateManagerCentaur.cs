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
        // Проверка на вражеских юнитов в радиусе агро
        Transform nearestEnemy = GetClosestEnemy();

        if (nearestEnemy != null)
        {
            return nearestEnemy;
        }

        // Если врагов нет — атакуем ближайшую башню
        return GetClosestTower();
    }

    private Transform GetClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("EnemyUnit");

        // Если врагов вообще нет — возвращаем null
        if (enemies == null || enemies.Length == 0)
            return null;

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

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