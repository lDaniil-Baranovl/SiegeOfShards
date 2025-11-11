using UnityEngine;

public class GolStateMan : UnitStateManager
{
    public GolRunState golRunState = new GolRunState();
    public GolAttackState golAttackState = new GolAttackState();
    public DeathGolState deathGolState = new DeathGolState();

    private UnitBaseState<GolStateMan> currentState;

    [SerializeField] public GameObject attackEffect;
    public bool isAttackEffectActive = false;

    public AudioClip attackSound;
    private AudioSource audioSource;
    protected override void Start()
    {
        base.Start();
        attackEffect.SetActive(false);
        SwitchState(golRunState);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    protected override void Update()
    {
        base.Update();
        currentState?.UpdateState(this);
    }

    public void SwitchState(UnitBaseState<GolStateMan> newState)
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
        SwitchState(deathGolState);
    }

    public override Transform GetTarget()
    {
        return GetClosestTower();
    }

    // Animation Events:
    public void OnOffDamagerGol(int isOff)
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
    public void GolAnimationEvent_ResetDamage()
    {
        if (damageCollider == null) return;

        var damageScript = damageCollider.GetComponent<DamageGol>();
        damageScript?.Gol_ResetDamageFromAnimation();
    }
    public void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
}
