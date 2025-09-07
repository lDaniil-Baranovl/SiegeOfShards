using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DeathCentaur : CentaurBaseState
{
    public override void EnterState(CentaurStateManager manager)
    {
        manager.centaur_animator.SetTrigger("Die");
        manager.centaur_animator.SetBool("IsRunningCentaur", false);
        manager.centaur_animator.SetBool("IsAttackingCentaur", false);
        manager.centaur_navMeshAgent.isStopped = true;
        manager.centaur_navMeshAgent.enabled = false;

        var colliders = manager.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        var rb = manager.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
        }

        manager.StartCoroutine(WaitAndDie(manager, 3f));
    }
    private IEnumerator WaitAndDie(CentaurStateManager manager, float delay)
    {
        yield return new WaitForSeconds(delay);

        // ƒополнительна€ проверка перед уничтожением
        if (manager != null && manager.gameObject != null)
        {
            GameObject.Destroy(manager.gameObject);
        }
    }
    public override void ExitState(CentaurStateManager manager)
    {     
    }
    public override void UpdateState(CentaurStateManager manager)
    {   
    }
}
