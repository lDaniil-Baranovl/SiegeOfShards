using UnityEngine;

public class RunCentaurState : CentaurBaseState
{
    private float delayTimer = 0f;
    private float delayBeforeRun = 0.5f; 
    private bool hasStartedRunning = false;

    public override void EnterState(CentaurStateManager manager)
    {
        delayTimer = 0f;
        hasStartedRunning = false;
        manager.centaur_runTime = 0f;
       
        manager.SetSpeed(manager.centaur_walkSpeed);
        manager.centaur_navMeshAgent.isStopped = false;
        manager.centaur_animator.SetBool("IsRunningCentaur", false);
        manager.centaur_animator.SetBool("IsAttackingCentaur", false);
    }

    public override void ExitState(CentaurStateManager manager)
    {
        manager.centaur_animator.SetBool("IsRunningCentaur", false);
    }

    public override void UpdateState(CentaurStateManager manager)
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
                    manager.centaur_animator.SetBool("IsRunningCentaur", true);
                }
            }
            return;
        }

        manager.centaur_runTime += Time.deltaTime;

        Transform newTarget = manager.GetTarget();

        if (newTarget != null)
        {
            manager.centaur_target = newTarget;

            manager.centaur_navMeshAgent.SetDestination(newTarget.position);

            if (manager.HasReachedTarget())
            {
                manager.SwitchState(manager.attackCentaurState);
            }
        }
        else
        {
            manager.centaur_animator.SetBool("IsRunningCentaur", false);
            manager.centaur_navMeshAgent.isStopped = true;
        }
    }
}