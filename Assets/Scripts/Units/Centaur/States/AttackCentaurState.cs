using UnityEngine;

public class AttackCentaurState : UnitBaseState<CentaurStateManager>
{
    private bool usedSpecialAttack = false;
    private DamageCentaur attackScript;
    public override void EnterState(CentaurStateManager manager)
    {
        manager.navMeshAgent.isStopped = true;

        attackScript = manager.damageCollider.GetComponent<DamageCentaur>();
        
        if (manager.centaur_runTime >= 3f)
        {
            manager.unitAnimator.SetTrigger("SpecialAttackCentaur");
            if (attackScript != null)
                attackScript.SetSpecialAttack(true);
        }
        else
        {
            manager.unitAnimator.SetBool("IsAttackingCentaur", true);
            if (attackScript != null)
                attackScript.SetSpecialAttack(false);
        }

        manager.centaur_runTime = 0f;
    }

    public override void ExitState(CentaurStateManager manager)
    {
        manager.navMeshAgent.isStopped = false;
        manager.unitAnimator.SetBool("IsAttackingCentaur", false);
        
    }

    public override void UpdateState(CentaurStateManager manager)
    {

        if (manager.target == null || Vector3.Distance(manager.transform.position, manager.target.position) > manager.attackDistance + 1f)
        {
            manager.SwitchState(manager.runCentaurState);
            return;
        }
        Vector3 direction = (manager.target.position - manager.transform.position).normalized;
        direction.y = 0; // чтобы не наклонялся вверх/вниз

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            manager.transform.rotation = Quaternion.Slerp(manager.transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
}