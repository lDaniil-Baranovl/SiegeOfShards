using UnityEngine;

public class FireDrgRunState : UnitBaseState<StateManagerFireDragon>
{
    private float delayTimer = 0f;
    private float delayBeforeRun = 0.5f;
    private bool hasStartedRunning = false;

    public override void EnterState(StateManagerFireDragon manager)
    {
        delayTimer = 0f;
        hasStartedRunning = false;

        manager.SetSpeed(manager.walkSpeed);
        manager.navMeshAgent.isStopped = false;
        manager.unitAnimator.SetBool("IsRunning", false);
        manager.unitAnimator.SetBool("IsAttacking", false);
    }

    public override void ExitState(StateManagerFireDragon manager)
    {
        manager.unitAnimator.SetBool("IsRunning", false);
    }

    public override void UpdateState(StateManagerFireDragon manager)
    {
        if (!manager.canMove) return;

        if (!hasStartedRunning)
        {
            delayTimer += Time.deltaTime;

            if (delayTimer >= delayBeforeRun)
            {
                hasStartedRunning = true;

                Transform target = manager.GetTarget();
                if (target != null)
                {
                    manager.target = target;
                    manager.SetDestination(target);
                    manager.unitAnimator.SetBool("IsRunning", true);
                }
            }
            return;
        }

        Transform newTarget = manager.GetTarget();

        if (newTarget != null)
        {
            manager.target = newTarget;

            Vector3 targetPos = manager.GetAttackPosition(newTarget);
            manager.navMeshAgent.SetDestination(targetPos);

            if (manager.HasReachedTarget())
            {
                manager.SwitchState(manager.attackFireState);
            }
        }
        else
        {
            manager.unitAnimator.SetBool("IsRunning", false);
            manager.navMeshAgent.isStopped = true;
        }
    }
}
