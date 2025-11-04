using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class StateManagerFlyColdDragon : UnitStateManager
{
    public DragFlyColdRunState dragFlyColdRunState = new DragFlyColdRunState();
    public DragFlyColdAttackState dragFlyColdAttackState = new DragFlyColdAttackState();
    public DragFlyColdDeathState deathState = new DragFlyColdDeathState();

    private UnitBaseState<StateManagerFlyColdDragon> currentState;

    [SerializeField] public GameObject attackEffect;
    public bool isAttackEffectActive = false;


    protected override void Start()
    {
        base.Start();
        StartCoroutine(EnableNavMeshAfterDeath());
        attackEffect.SetActive(false);
        SwitchState(dragFlyColdRunState);
    }

    protected override void Update()
    {
        base.Update();
        currentState?.UpdateState(this);
    }

    public void SwitchState(UnitBaseState<StateManagerFlyColdDragon> newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    public override void OnUnitDie()
    {
        SwitchState(deathState);
    }
    public IEnumerator EnableNavMeshAfterDeath()
    {
        yield return new WaitForSeconds(1.2f);
        if (navMeshAgent != null)
            navMeshAgent.enabled = true;
    }

    // Animation Events:
    public void OnOffDamagerFDC(int isOff)
    {
        if (isOff == 0 || isAttackEffectActive == true)
        {
            damageCollider.enabled = false;
            attackEffect.SetActive(false);
            isAttackEffectActive = false;
        }
        else
        {
            damageCollider.enabled = true;
            attackEffect.SetActive(true);
            isAttackEffectActive = true;
        }
    }
    public void FLCAnimationEvent_ResetDamage()
    {
        if (damageCollider == null) return;

        var damageScript = damageCollider.GetComponent<DamageFDC>();
        damageScript?.DFC_ResetDamageFromAnimation();
    }
}