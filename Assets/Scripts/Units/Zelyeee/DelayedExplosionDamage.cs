using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DelayedExplosionDamage : MonoBehaviour
{
    [SerializeField] private int damage = 100;
    [SerializeField] private float delay = 2f;
    [SerializeField] public int teamID = 0;
    [SerializeField] private bool destroyAfterHit = true; 

    private readonly HashSet<Health> targets = new HashSet<Health>();
    private bool damageApplied = false;

    private void Awake()
    {
        
        StartCoroutine(DelayedDamage());
    }

    private void OnTriggerEnter(Collider other)
    {
        Health hp = other.GetComponent<Health>();
        if (hp == null) return;

        
        if (hp.GetTeam() != teamID)
            return;

        targets.Add(hp);
    }

    private void OnTriggerExit(Collider other)
    {
        Health hp = other.GetComponent<Health>();
        if (hp != null && targets.Contains(hp))
            targets.Remove(hp);
    }

    private IEnumerator DelayedDamage()
    {
        
        yield return new WaitForSeconds(delay);

        if (damageApplied)
            yield break;

        damageApplied = true;

        foreach (var unit in targets)
        {
            if (unit != null && !unit.IsDead)
                unit.ApplyDamage(damage, "Potion Explosion");
        }

        if (destroyAfterHit)
            Destroy(gameObject);
    }
}