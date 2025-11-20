using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
public class SkeletStMan : UnitStateManager
{
    public SkeletRunSt skeletRunSt = new SkeletRunSt();
    public SkeletAttackSt skeletAttackSt = new SkeletAttackSt();
    public SkeletDeathSt skeletDeathSt = new SkeletDeathSt();

    private UnitBaseState<SkeletStMan> currentState;

    [SerializeField] public GameObject attackEffect;
    public bool isAttackEffectActive = false;

    public AudioClip attackSound;
    private AudioSource audioSource;

    [SerializeField] private float towerAttackDistance = 1.35f;

    protected override void Start()
    {
        base.Start();
        attackEffect.SetActive(false);
        SwitchState(skeletRunSt);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    protected override void Update()
    {
        base.Update();
        currentState?.UpdateState(this);
    }
    public override bool HasReachedTarget()
    {
        if (target == null) return false;

        float dist = Vector3.Distance(transform.position, target.position);
        if (target.GetComponent<HealthTower>() != null)
        {
            return dist <= towerAttackDistance;
        }
        return dist <= attackDistance;
    }
    public void SwitchState(UnitBaseState<SkeletStMan> newState)
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
        SwitchState(skeletDeathSt);
    }

    // Animation Events:
    public void OnOffDamagerSkel(int isOff)
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
    public void SkelAnimationEvent_ResetDamage()
    {
        if (damageCollider == null) return;

        var damageScript = damageCollider.GetComponent<DamageSkel>();
        damageScript?.Skel_ResetDamageFromAnimation();
    }
    public void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }
}
