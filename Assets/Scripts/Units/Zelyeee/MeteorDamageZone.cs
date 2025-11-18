using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeteorDamageZone : MonoBehaviour
{
    [SerializeField] private int damagePerTick = 10;
    [SerializeField] private float tickRate = 1f;
    [SerializeField] private float firstHitDelay = 1f;
    [Header("Who take damage")]
    [SerializeField] public int teamID = 0;

    private bool isWorking = false;

   
    private readonly Dictionary<Health, float> enterTime = new Dictionary<Health, float>();
    private readonly HashSet<Health> unitsInZone = new HashSet<Health>();

    private void OnTriggerEnter(Collider other)
    {
        Health hp = other.GetComponent<Health>();
        if (hp == null) return;

        if (hp.GetTeam() == teamID)
        {
            unitsInZone.Add(hp);
            enterTime[hp] = Time.time;  
            TryStartDamage();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Health hp = other.GetComponent<Health>();
        if (hp == null) return;

        if (unitsInZone.Contains(hp))
            unitsInZone.Remove(hp);

        if (enterTime.ContainsKey(hp))
            enterTime.Remove(hp);
    }

    private void TryStartDamage()
    {
        if (!isWorking)
            StartCoroutine(DamageRoutine());
    }

    private IEnumerator DamageRoutine()
    {
        isWorking = true;

        while (unitsInZone.Count > 0)
        {
            foreach (var unit in unitsInZone)
            {
                if (unit == null || unit.IsDead)
                    continue;

                float timeInside = Time.time - enterTime[unit];

                if (timeInside < firstHitDelay)
                    continue;

                unit.ApplyDamage(damagePerTick, "Meteor Rain");
            }

            yield return new WaitForSeconds(tickRate);
        }

        isWorking = false;
    }
}
