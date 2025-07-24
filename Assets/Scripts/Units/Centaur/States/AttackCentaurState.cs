using UnityEngine;

public class AttackCentaurState : CentaurBaseState
{
    private bool usedSpecialAttack = false;

    public override void EnterState(CentaurStateManager manager)
    {
        manager.centaur_navMeshAgent.isStopped = true;

        usedSpecialAttack = false;

        if (manager.centaur_runTime >= 3f)
        {
            // Быстрая/особая атака после длинного бега
            manager.centaur_animator.SetTrigger("SpecialAttackCentaur");
            usedSpecialAttack = true;
        }
        else
        {
            // Обычная атака
            manager.centaur_animator.SetBool("IsAttackingCentaur", true);
        }

        // Сбросим беговой таймер
        manager.centaur_runTime = 0f;
    }

    public override void ExitState(CentaurStateManager manager)
    {
        manager.centaur_navMeshAgent.isStopped = false;
        manager.centaur_animator.SetBool("IsAttackingCentaur", false);
    }

    public override void UpdateState(CentaurStateManager manager)
    {

        if (manager.centaur_target == null || Vector3.Distance(manager.transform.position, manager.centaur_target.position) > manager.attackDistance + 1f)
        {
            manager.SwitchState(manager.runCentaurState);
            return;
        }
    }
}