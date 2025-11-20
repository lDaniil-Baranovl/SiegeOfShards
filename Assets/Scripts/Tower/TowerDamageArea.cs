using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerSingleTargetDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damagePerTick = 10f;
    public float tickRate = 0.5f;

    [Header("Team Settings")]
    public int teamID = 1;

    [Header("Laser Settings")]
    public LineRenderer laser;
    public Transform laserOrigin;

    private readonly List<Health> unitsInRange = new List<Health>();
    private Health currentTarget;
    private Coroutine attackRoutine;

    private void Start()
    {
        if (laser == null)
        {
            laser = gameObject.AddComponent<LineRenderer>();
            laser.startWidth = 0.08f;
            laser.endWidth = 0.08f;
            laser.enabled = false;
        }

        if (laserOrigin == null)
            laserOrigin = transform;
    }

    private void Update()
    {
        if (currentTarget != null && !currentTarget.IsDead)
        {
            laser.enabled = true;

            laser.SetPosition(0, laserOrigin.position);
            laser.SetPosition(1, currentTarget.transform.position + Vector3.up * 0.5f);
        }
        else
        {
            laser.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Health unit = other.GetComponent<Health>();
        if (unit == null) return;
        if (unit.GetTeam() == teamID) return;
        if (unit.IsDead) return;

        if (!unitsInRange.Contains(unit))
            unitsInRange.Add(unit);

        TryAcquireTarget();
    }

    private void OnTriggerExit(Collider other)
    {
        Health unit = other.GetComponent<Health>();
        if (unit == null) return;

        unitsInRange.Remove(unit);

        if (unit == currentTarget)
        {
            currentTarget = null;
            TryAcquireTarget();
        }
    }

    private void TryAcquireTarget()
    {
        if (currentTarget != null && !currentTarget.IsDead)
            return;

        currentTarget = GetClosestUnit();

        if (currentTarget != null)
        {
            if (attackRoutine == null)
                attackRoutine = StartCoroutine(AttackRoutine());
        }
        else
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
                attackRoutine = null;
            }

            laser.enabled = false;
        }
    }

    private Health GetClosestUnit()
    {
        float minDist = float.MaxValue;
        Health closest = null;

        foreach (var unit in unitsInRange)
        {
            if (unit == null || unit.IsDead) continue;

            float dist = Vector3.Distance(transform.position, unit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = unit;
            }
        }

        return closest;
    }

    private IEnumerator AttackRoutine()
    {
        while (true)
        {
            if (currentTarget == null || currentTarget.IsDead)
            {
                TryAcquireTarget();
                yield break;
            }

            currentTarget.ApplyDamage(Mathf.RoundToInt(damagePerTick), "Tower");
            yield return new WaitForSeconds(tickRate);
        }
    }
}
