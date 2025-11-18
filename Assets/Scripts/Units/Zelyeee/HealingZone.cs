using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class HealingZone : MonoBehaviour
{
    [SerializeField] private int healAmount = 5;
    [SerializeField] private float healInterval = 1f;
    [Header("Who take hill")]
    [SerializeField] public int zoneTeamID = 0;

    private HashSet<Health> unitsInside = new HashSet<Health>();
    private bool isHealing = false;

    private void OnTriggerEnter(Collider other)
    {
        Health hp = other.GetComponent<Health>();
        if (hp == null) return;

        if (hp.GetTeam() == zoneTeamID)
        {
            unitsInside.Add(hp);
            StartHealing();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Health hp = other.GetComponent<Health>();
        if (hp != null)
        {
            unitsInside.Remove(hp);
        }
    }

    private void StartHealing()
    {
        if (!isHealing)
        {
            isHealing = true;
            StartCoroutine(HealRoutine());
        }
    }

    private IEnumerator HealRoutine()
    {
        while (unitsInside.Count > 0)
        {
            foreach (Health hp in unitsInside)
            {
                if (hp == null || hp.IsDead) continue;

                hp.CurrentHealth = Mathf.Min(hp.CurrentHealth + healAmount, hp.maxHealth);

                HealthBar hb = hp.GetComponentInChildren<HealthBar>();
                if (hb != null)
                    hb.UpdateHealthBar();
            }

            yield return new WaitForSeconds(healInterval);
        }

        isHealing = false;
    }
}
