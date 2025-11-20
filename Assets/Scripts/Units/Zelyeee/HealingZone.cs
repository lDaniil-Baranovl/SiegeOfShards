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

    [SerializeField] private AudioClip healSound;
    private AudioSource audioSource;
    [Header("Audio Loop Settings")]
    [SerializeField] private float loopStartTime = 0.8f;
    [SerializeField] private float loopEndTime = 1.8f;
    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(AudioLoopRoutine());
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.pitch = 0.95f;

        if (healSound != null)
        {
            audioSource.clip = healSound;
            audioSource.Play();
        }
        Destroy(gameObject, 8f);
    }
    private IEnumerator AudioLoopRoutine()
    {
        if (healSound == null) yield break;

        audioSource.clip = healSound;
        audioSource.Play();
        audioSource.time = loopStartTime;
        while (true)
        {
            if (audioSource.time >= loopEndTime)
            {
                audioSource.time = loopStartTime;
            }

            yield return null;
        }
    }
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
