using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Audio;

public class StateManagerFireDragon : UnitStateManager
{
    private UnitBaseState<StateManagerFireDragon> currentState;

    [SerializeField] public GameObject attackEffect;
    public bool isAttackEffectActive = false;

    public AttackFireState attackFireState = new AttackFireState();
    public FireDrgRunState fireDrgRunState = new FireDrgRunState();
    public DeathFireState deathFireState = new DeathFireState();

    public AudioClip attackSound;
    private AudioSource audioSource;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(EnableNavMeshAfterDeath());
        attackEffect.SetActive(false);
        SwitchState(fireDrgRunState);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    protected override void Update()
    {
        base.Update();
        currentState?.UpdateState(this);
    }

    public void SwitchState(UnitBaseState<StateManagerFireDragon> newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    public override void OnUnitDie()
    {
        if (isDead) return;
        isDead = true;

        gameObject.layer = LayerMask.NameToLayer("Dead");

        if (damageCollider != null) damageCollider.enabled = false;
        if (attackEffect != null) attackEffect.SetActive(false);

        if (navMeshAgent != null) navMeshAgent.isStopped = true;
        if (unitAnimator != null)
        {
            unitAnimator.SetBool("IsAttacking", false);
            unitAnimator.SetBool("IsRunning", false);
        }
        SwitchState(deathFireState);
    }
    public IEnumerator EnableNavMeshAfterDeath()
    {
        yield return new WaitForSeconds(1.2f);
        if (navMeshAgent != null)
            navMeshAgent.enabled = true;
    }

    // Animation Events:
    public void OnOffDamagerFireDr(int isOff)
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
    public void FiraDrgAnimationEvent_ResetDamage()
    {
        if (damageCollider == null) return;

        var damageScript = damageCollider.GetComponent<DamageFiraDrg>();
        damageScript?.FiraDrgAnimationEvent_ResetDamage();
    }
    public void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
}
