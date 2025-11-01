using UnityEngine;
using UnityEngine.AI;
using System.Collections;
public class DragFlyColdDeathState : FlyColdDragonBaseState
{
    public override void EnterState(StateManagerFlyColdDragon manager)
    {
        DisableAllColliders(manager);
        DisableAllRenderers(manager);
        ActivateDeathEffect(manager);

        manager.StartCoroutine(DestroyWithDelay(manager));
    }
    private void DisableAllColliders(StateManagerFlyColdDragon manager)
    {
        var colliders = manager.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
    }
    private void DisableAllRenderers(StateManagerFlyColdDragon manager)
    {
        var renderers = manager.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }
    }
    private void ActivateDeathEffect(StateManagerFlyColdDragon manager)
    {
        if (manager.effectDieth != null)
        {
            manager.effectDieth.transform.position = manager.transform.position + Vector3.up * 1.25f;
            manager.effectDieth.SetActive(true);
        }
    }

    private IEnumerator DestroyWithDelay(StateManagerFlyColdDragon manager)
    {
        yield return new WaitForSeconds(1f);
        GameObject.Destroy(manager.gameObject);
    }

    public override void UpdateState(StateManagerFlyColdDragon manager) { }
    public override void ExitState(StateManagerFlyColdDragon manager) { }
}
