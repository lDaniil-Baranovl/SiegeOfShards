using UnityEngine;

public class RunCentaurState : CentaurBaseState
{
    private float delayTimer = 0f;
    private float delayBeforeRun = 1.3f; // врем€ ожидани€ перед движением
    private bool hasStartedRunning = false;

    public override void EnterState(CentaurStateManager manager)
    {
        delayTimer = 0f;
        hasStartedRunning = false;
        manager.centaur_runTime = 0f;
        // —брос анимаций
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

        // ѕровер€ем и обновл€ем цель каждый кадр
        Transform newTarget = manager.GetTarget();

        if (newTarget != null)
        {
            // ѕ≈–≈«јѕ»—џ¬ј≈ћ координаты, даже если цель та же
            manager.centaur_target = newTarget;

            // ќЅЌќ¬Ћя≈ћ SetDestination каждый кадр Ч теперь кентавр будет точно преследовать
            manager.centaur_navMeshAgent.SetDestination(newTarget.position);

            // ≈сли достиг цели
            if (manager.HasReachedTarget())
            {
                manager.SwitchState(manager.attackCentaurState);
            }
        }
        else
        {
            // ≈сли цели нет Ч остановим анимацию бега
            manager.centaur_animator.SetBool("IsRunningCentaur", false);
            manager.centaur_navMeshAgent.isStopped = true;
        }
    }
}