using UnityEngine;
using System.Collections;

public class UnitDeathState<T> : UnitBaseState<T> where T : UnitStateManager
{
    public override void EnterState(T manager)
    {
        OnEnterDeath(manager);
    }

    public override void ExitState(T manager) { }
    public override void UpdateState(T manager) { }

    protected virtual void OnEnterDeath(T manager)
    {
        DisableAllColliders(manager);
        DisableAllRenderers(manager);
        ActivateDeathEffect(manager);

        manager.StartCoroutine(DestroyWithDelay(manager));
    }

    protected void DisableAllColliders(T manager)
    {
        var colliders = manager.GetComponentsInChildren<Collider>();
        foreach (var c in colliders)
            c.enabled = false;
    }

    protected void DisableAllRenderers(T manager)
    {
        var renderers = manager.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
            r.enabled = false;
    }

    protected void ActivateDeathEffect(T manager)
    {
        if (manager.deathEffect != null)
        {
            manager.deathEffect.transform.position = manager.transform.position + Vector3.up * 1.25f;
            manager.deathEffect.SetActive(true);
        }
    }

    protected IEnumerator DestroyWithDelay(T manager)
    {
        yield return new WaitForSeconds(1f);
        GameObject.Destroy(manager.gameObject);
    }
}