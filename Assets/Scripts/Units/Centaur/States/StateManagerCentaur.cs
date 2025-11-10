using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CentaurStateManager : UnitStateManager
{
    public float centaur_runTime = 3f;

    public RunCentaurState runCentaurState = new RunCentaurState();
    public AttackCentaurState attackCentaurState = new AttackCentaurState();
    public DeathCentaur deathCentaurState = new DeathCentaur();

    private UnitBaseState<CentaurStateManager> currentState;

    [HideInInspector] public float runTime;

    [SerializeField] public GameObject attackEffect;
    public AudioClip specialAttackSound;
    public bool isAttackEffectActive = false;

    public AudioClip attackSound;
    private AudioSource audioSource;
    protected override void Start()
    {
        base.Start();
        attackEffect.SetActive(false);
        SwitchState(runCentaurState);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    protected override void Update()
    {
        base.Update();
        currentState?.UpdateState(this);
    }

    public void SwitchState(UnitBaseState<CentaurStateManager> newState)
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
        SwitchState(deathCentaurState);
    }

    // Animation Events:
    public void OnOffDamagerCen(int isOff)
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
    public void CenAnimationEvent_ResetDamage()
    {
        if (damageCollider == null) return;

        var damageScript = damageCollider.GetComponent<DamageCentaur>();
        damageScript?.CEN_ResetDamageFromAnimation();
    }
    public void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
    public void PlaySpecialAttackSound()
    {
        if (specialAttackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(specialAttackSound);
        }
        else if (attackSound != null && audioSource != null) // fallback на обычный звук
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
}