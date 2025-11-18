using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeZone : MonoBehaviour
{
    [Header("Freeze Settings")]
    [SerializeField] private float lifeTime = 6f;

    [Header("Team Affected")]
    [SerializeField] private int teamID = 0;

    [Header("Freeze Effect")]
    [SerializeField] private GameObject freezeEffectPrefab;
    [SerializeField] private AudioClip freezeSound;

    private readonly HashSet<Health> frozenUnits = new HashSet<Health>();
    private readonly Dictionary<Health, GameObject> activeEffects = new Dictionary<Health, GameObject>();

    private AudioSource audioSource;
    [Header("Audio Loop Settings")]
    [SerializeField] private float loopStartTime = 0.8f;
    [SerializeField] private float loopEndTime = 2.7f;


    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(AudioLoopRoutine());
        audioSource.playOnAwake = false;
        audioSource.loop = true;


        if (freezeSound != null)
        {
            audioSource.clip = freezeSound;
            audioSource.Play();
        }
        CaptureUnitsInZone();

        foreach (var hp in frozenUnits)
        {
            hp?.Freeze();
            CreateEffect(hp);
        }

        Destroy(gameObject, lifeTime);
    }
    private IEnumerator AudioLoopRoutine()
    {
        if (freezeSound == null) yield break;

        audioSource.clip = freezeSound;
        audioSource.Play();

        // —разу перематываем в начало луп-зоны
        audioSource.time = loopStartTime;

        while (true)
        {
            // если дошли до конца лупа Ч перемотка назад
            if (audioSource.time >= loopEndTime)
            {
                audioSource.time = loopStartTime;
            }

            yield return null;
        }
    }
    private void CaptureUnitsInZone()
    {
        float radius = GetZoneRadius();

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (var h in hits)
        {
            Health hp = h.GetComponent<Health>();
            if (hp == null) continue;

            if (hp.GetTeam() == teamID)
                frozenUnits.Add(hp);
        }
    }

    private void CreateEffect(Health hp)
    {
        if (freezeEffectPrefab == null) return;
        GameObject fx = Instantiate(freezeEffectPrefab, hp.transform);

        fx.transform.localPosition = Vector3.zero;

        activeEffects[hp] = fx;
    }
    private void RemoveEffect(Health hp)
    {
        if (activeEffects.ContainsKey(hp))
        {
            if (activeEffects[hp] != null)
                Destroy(activeEffects[hp]);

            activeEffects.Remove(hp);
        }
    }
    private float GetZoneRadius()
    {
        SphereCollider sphere = GetComponent<SphereCollider>();
        if (sphere != null)
            return sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);

        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
            return Mathf.Max(box.size.x, box.size.z) * 0.5f * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);

        Debug.LogError("FreezeZone: “ребуетс€ SphereCollider или BoxCollider!");
        return 0f;
    }
    private void OnDestroy()
    {
        foreach (var hp in frozenUnits)
        {
            if (hp != null)
            {
                hp.Unfreeze();
                RemoveEffect(hp);
            }
        }
    }
    private void OnTriggerEnter(Collider other) { }
    private void OnTriggerExit(Collider other) { }
}
