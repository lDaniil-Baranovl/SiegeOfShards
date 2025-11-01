using UnityEngine;

public class DragFlyColdRunState : FlyColdDragonBaseState
{
    private float delayTimer = 0f;
    private float delayBeforeRun = 0.5f;
    private bool hasStartedRunning = false;

    public override void EnterState(StateManagerFlyColdDragon manager)
    {
        delayTimer = 0f;
        hasStartedRunning = false;
        manager.dragFlyCold_runTime = 0f;

        manager.SetSpeed(manager.dragFlyCold_walkSpeed);
        manager.dragFlyCold_navMeshAgent.isStopped = false;
        manager.dragFlyCold_animator.SetBool("IsRunningdragFlyCold", false);
        manager.dragFlyCold_animator.SetBool("IsAttackingdragFlyCold", false);
    }

    public override void ExitState(StateManagerFlyColdDragon manager)
    {
        manager.dragFlyCold_animator.SetBool("IsRunningdragFlyCold", false);
    }

    public override void UpdateState(StateManagerFlyColdDragon manager)
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
                    manager.SetDestination(target);
                    manager.dragFlyCold_animator.SetBool("IsRunningdragFlyCold", true);
                }
            }
            return;
        }

        manager.dragFlyCold_runTime += Time.deltaTime;

        Transform newTarget = manager.GetTarget();

        if (newTarget != null)
        {
            manager.dragFlyCold_target = newTarget;

            manager.dragFlyCold_navMeshAgent.SetDestination(newTarget.position);

            if (manager.HasReachedTarget())
            {
                manager.SwitchState(manager.dragFlyColdAttackState);
            }
        }
        else
        {
            manager.dragFlyCold_animator.SetBool("IsRunningdragFlyCold", false);
            manager.dragFlyCold_navMeshAgent.isStopped = true;
        }
    }
}
