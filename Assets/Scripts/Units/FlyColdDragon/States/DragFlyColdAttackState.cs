using UnityEngine;

public class DragFlyColdAttackState : UnitBaseState<StateManagerFlyColdDragon>
{
    public DamageFDC attackScript;
    public override void EnterState(StateManagerFlyColdDragon manager)
    {
        manager.navMeshAgent.isStopped = true;

        attackScript = manager.damageCollider.GetComponent<DamageFDC>();

        manager.unitAnimator.SetBool("IsAttackingdragFlyCold", true);
    }

    public override void ExitState(StateManagerFlyColdDragon manager)
    {
        manager.navMeshAgent.isStopped = false;
        manager.unitAnimator.SetBool("IsAttackingdragFlyCold", false);
        if (manager.damageCollider != null)
            manager.damageCollider.enabled = false;

        if (manager.attackEffect != null)
            manager.attackEffect.SetActive(false);

        manager.isAttackEffectActive = false;
    }

    public override void UpdateState(StateManagerFlyColdDragon manager)
    {

        if (manager.target == null || Vector3.Distance(manager.transform.position, manager.target.position) > manager.attackDistance + 1f)
        {
            manager.SwitchState(manager.dragFlyColdRunState);
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
