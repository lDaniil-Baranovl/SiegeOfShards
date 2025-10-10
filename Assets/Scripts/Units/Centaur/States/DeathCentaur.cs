using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DeathCentaur : CentaurBaseState
{
    public override void EnterState(CentaurStateManager manager)
    {
        Debug.Log("Enter Death State - Instant Disappear with Effect");
        StopAllMovement(manager);
        DisableAllColliders(manager);
        DisableAllRenderers(manager);
        ActivateDeathEffect(manager);

        manager.StartCoroutine(DestroyWithDelay(manager));
    }

    private void StopAllMovement(CentaurStateManager manager)
    {
        var agent = manager.centaur_navMeshAgent;
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
            Debug.Log("NavMeshAgent disabled");
        }
        var rbs = manager.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.Log($"Rigidbody {rb.name} kinematic: true");
        }
    }
    private void DisableAllColliders(CentaurStateManager manager)
    {
        var colliders = manager.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
            Debug.Log($"Collider {collider.name} disabled");
        }
    }
    private void DisableAllRenderers(CentaurStateManager manager)
    {
        var renderers = manager.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
            Debug.Log($"Renderer {renderer.name} disabled");
        }
    }
    private void ActivateDeathEffect(CentaurStateManager manager)
    {
        if (manager.effectDieth != null)
        {
            manager.effectDieth.transform.position = manager.transform.position;
            manager.effectDieth.SetActive(true);
            Debug.Log("Death effect activated");
        }
        else
        {
            Debug.LogWarning("Death effect reference is missing in CentaurStateManager");
        }
    }

    private IEnumerator DestroyWithDelay(CentaurStateManager manager)
    {
        yield return new WaitForSeconds(1f);
        GameObject.Destroy(manager.gameObject);
        Debug.Log("Centaur destroyed with death effect");
    }

    public override void UpdateState(CentaurStateManager manager) { }
    public override void ExitState(CentaurStateManager manager) { }
}