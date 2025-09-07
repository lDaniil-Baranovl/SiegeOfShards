using UnityEngine;
using UnityEngine.AI;

public class DeathCentaur : CentaurBaseState
{
    public override void EnterState(CentaurStateManager manager)
    {

        manager.centaur_animator.SetTrigger("DeathCentaur");
        manager.centaur_navMeshAgent.isStopped = true;
        manager.centaur_navMeshAgent.enabled = false;

        var colliders = manager.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }


        var rb = manager.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

    }

    public override void ExitState(CentaurStateManager manager) { }
    public override void UpdateState(CentaurStateManager manager) { }
}