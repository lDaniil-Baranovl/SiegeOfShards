using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class DeathCentaur : CentaurBaseState
{
    public override void EnterState(CentaurStateManager manager)
    {
        DisableAllColliders(manager);
        DisableAllRenderers(manager);
        ActivateDeathEffect(manager);

        manager.StartCoroutine(DestroyWithDelay(manager));
    }
    private void DisableAllColliders(CentaurStateManager manager)
    {
        var colliders = manager.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
    }
    private void DisableAllRenderers(CentaurStateManager manager)
    {
        var renderers = manager.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }
    }
    private void ActivateDeathEffect(CentaurStateManager manager)
    {
        if (manager.effectDieth != null)
        {
            manager.effectDieth.transform.position = manager.transform.position + Vector3.up * 1.25f;
            manager.effectDieth.SetActive(true);
        }
    }

    private IEnumerator DestroyWithDelay(CentaurStateManager manager)
    {
        yield return new WaitForSeconds(1f);
        GameObject.Destroy(manager.gameObject);
    }

    public override void UpdateState(CentaurStateManager manager) { }
    public override void ExitState(CentaurStateManager manager) { }
}