using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DelayedExplosionDamage : MonoBehaviour
{
    [SerializeField] private int damage = 100;
    [SerializeField] private float delay = 2f;
    [SerializeField] public int teamID = 0;

    private readonly HashSet<Health> targets = new HashSet<Health>();
    private bool damageApplied = false;

    [Header("Cast + Explosion Sounds")]
    [SerializeField] private AudioClip castSound;
    [SerializeField] private AudioClip explosionSound;

    private void Start()
    {
        StartCoroutine(CastExplosionSoundRoutine());
        Destroy(gameObject, 2f);
    }
    private void Awake()
    {
        StartCoroutine(DelayedDamage());
    }
    private IEnumerator CastExplosionSoundRoutine()
    {
        if (castSound == null || explosionSound == null)
            yield break;
        AudioSource castSource = gameObject.AddComponent<AudioSource>();
        AudioSource explosionSource = gameObject.AddComponent<AudioSource>();

        castSource.playOnAwake = false;
        explosionSource.playOnAwake = false;

        castSource.PlayOneShot(castSound);

        yield return new WaitForSeconds(0.85f);

        explosionSource.PlayOneShot(explosionSound);

        float fadeTime = 0.4f;   
        float t = 0f;

        float startVolume = castSource.volume;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            castSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }

        castSource.volume = 0f;
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
    }
}