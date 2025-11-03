using UnityEngine;

public class DragFlyColdRunState : UnitBaseState<StateManagerFlyColdDragon>
{
    private float delayTimer = 0f;
    private float delayBeforeRun = 0.5f;
    private bool hasStartedRunning = false;

    public override void EnterState(StateManagerFlyColdDragon manager)
    {
        delayTimer = 0f;
        hasStartedRunning = false;

        manager.SetSpeed(manager.walkSpeed);
        manager.navMeshAgent.isStopped = false;
        manager.unitAnimator.SetBool("IsRunningdragFlyCold", false);
        manager.unitAnimator.SetBool("IsAttackingdragFlyCold", false);
    }

    public override void ExitState(StateManagerFlyColdDragon manager)
    {
        manager.unitAnimator.SetBool("IsRunningdragFlyCold", false);
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
                    manager.unitAnimator.SetBool("IsRunningdragFlyCold", true);
                }
            }
            return;
        }

        Transform newTarget = manager.GetTarget();

        if (newTarget != null)
        {
            manager.target = newTarget;

            manager.navMeshAgent.SetDestination(newTarget.position);

            if (manager.HasReachedTarget())
            {
                manager.SwitchState(manager.dragFlyColdAttackState);
            }
        }
        else
        {
            manager.unitAnimator.SetBool("IsRunningdragFlyCold", false);
            manager.navMeshAgent.isStopped = true;
        }
    }
}
