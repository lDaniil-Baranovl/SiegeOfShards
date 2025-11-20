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

    [Header("Sound Settings")]
    [SerializeField] private AudioClip meteorSound;

    [SerializeField] private float baseInterval = 1f;       
    [SerializeField] private float randomOffset = 0.5f;     

    [Header("Effect Lifetime")]
    [SerializeField] private float effectDuration = 4f;     
    [SerializeField] private float blockEdgeTime = 0.5f;    

    private AudioSource audioSource;
    private float startTime;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        startTime = Time.time;

        StartCoroutine(SoundRoutine());

        Destroy(gameObject, effectDuration);
    }

    private IEnumerator SoundRoutine()
    {
        while (true)
        {
            float elapsed = Time.time - startTime;
            if (elapsed > effectDuration - blockEdgeTime)
                yield break;
            if (elapsed >= blockEdgeTime)
            {
                audioSource.PlayOneShot(meteorSound);
            }
            float delay = baseInterval + Random.Range(-randomOffset, randomOffset);
            if (delay < 0.1f) delay = 0.1f;

            yield return new WaitForSeconds(delay);
        }
    }
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
