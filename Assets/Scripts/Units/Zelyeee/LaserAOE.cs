using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaserAOE : MonoBehaviour
{
    [Header("Percent Damage Per Wave (sum must be 80)")]
    [Range(0, 100)] public float wave1Percent = 10f;
    [Range(0, 100)] public float wave2Percent = 15f;
    [Range(0, 100)] public float wave3Percent = 15f;
    [Range(0, 100)] public float wave4Percent = 20f;
    [Range(0, 100)] public float wave5Percent = 20f;

    [SerializeField] private float waveInterval = 1.5f;

    [Header("Who take damage")]
    [SerializeField] private int teamID = 0;

    private bool isWorking = false;

    private readonly Dictionary<Health, int> unitWave = new Dictionary<Health, int>();
    private readonly HashSet<Health> unitsInZone = new HashSet<Health>();


    private void Start()
    {
        Destroy(gameObject, 15f);
    }


    private void OnTriggerEnter(Collider other)
    {
        Health hp = other.GetComponent<Health>();
        if (hp == null) return;

        if (hp.GetTeam() == teamID)
        {
            unitsInZone.Add(hp);
            unitWave[hp] = 0;
            TryStartDamage();
        }
    }


    private void OnTriggerExit(Collider other)
    {
        Health hp = other.GetComponent<Health>();
        if (hp == null) return;

        unitsInZone.Remove(hp);
        unitWave.Remove(hp);
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

                int wave = unitWave[unit];
                float damage = 0;

                switch (wave)
                {
                    case 0:
                        damage = unit.GetMaxHealth() * (wave1Percent / 100f);
                        unitWave[unit] = 1;
                        break;

                    case 1:
                        damage = unit.GetMaxHealth() * (wave2Percent / 100f);
                        unitWave[unit] = 2;
                        break;

                    case 2:
                        damage = unit.GetMaxHealth() * (wave3Percent / 100f);
                        unitWave[unit] = 3;
                        break;

                    case 3:
                        damage = unit.GetMaxHealth() * (wave4Percent / 100f);
                        unitWave[unit] = 4;
                        break;

                    case 4:
                        damage = unit.GetMaxHealth() * (wave5Percent / 100f);
                        unitWave[unit] = 5;
                        break;

                    case 5:
                        continue;
                }

                unit.ApplyDamage(Mathf.RoundToInt(damage), "Laser AOE % Wave");
            }

            yield return new WaitForSeconds(waveInterval);
        }

        isWorking = false;
    }
}
