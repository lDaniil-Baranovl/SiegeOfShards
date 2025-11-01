using UnityEngine;

public class DragFlyColdAttackState : FlyColdDragonBaseState
{
    public DamageFDC attackScript;
    public override void EnterState(StateManagerFlyColdDragon manager)
    {
        manager.dragFlyCold_navMeshAgent.isStopped = true;

        attackScript = manager.dragFlyCold_damageCollider.GetComponent<DamageFDC>();

        manager.dragFlyCold_animator.SetBool("IsAttackingdragFlyCold", true);
    }

    public override void ExitState(StateManagerFlyColdDragon manager)
    {
        manager.dragFlyCold_navMeshAgent.isStopped = false;
        manager.dragFlyCold_animator.SetBool("IsAttackingdragFlyCold", false);

    }

    public override void UpdateState(StateManagerFlyColdDragon manager)
    {

        if (manager.dragFlyCold_target == null || Vector3.Distance(manager.transform.position, manager.dragFlyCold_target.position) > manager.attackDistance + 1f)
        {
            manager.SwitchState(manager.dragFlyColdRunState);
            return;
        }
        Vector3 direction = (manager.dragFlyCold_target.position - manager.transform.position).normalized;
        direction.y = 0; // чтобы не наклонялся вверх/вниз

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            manager.transform.rotation = Quaternion.Slerp(manager.transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
}
