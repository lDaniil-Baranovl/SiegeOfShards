using System.Collections;
using UnityEngine;

public class EnemyAudienceAnimator : MonoBehaviour
{
    private enum IdleState { Idle, SitIdle, Dancing }

    [SerializeField] private Animator animator;
    [SerializeField] private float minSwitchDelay = 15f;
    [SerializeField] private float maxSwitchDelay = 20f;
    [SerializeField] private float maxDanceDuration = 5f;

    private static readonly int BrokeTowerTrigger = Animator.StringToHash("BrokeTower");
    private static readonly int IdleTrigger = Animator.StringToHash("Idle");
    private static readonly int SitIdleTrigger = Animator.StringToHash("SitIdle");
    private static readonly int DancingTrigger = Animator.StringToHash("Dancing");

    private IdleState currentState = IdleState.Idle;
    private Coroutine switchRoutine;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        BattleManager.OnTowerDestroyed += HandleTowerDestroyed;
        switchRoutine = StartCoroutine(RandomStateSwitchLoop());
    }

    private void OnDisable()
    {
        BattleManager.OnTowerDestroyed -= HandleTowerDestroyed;

        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
            switchRoutine = null;
        }
    }

    // Игрок с Team Id = 0 потерял башню — противник реагирует и перестаёт переключать idle-анимации
    private void HandleTowerDestroyed(int teamID)
    {
        if (teamID != 0) return;

        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
            switchRoutine = null;
        }

        animator.SetTrigger(BrokeTowerTrigger);
    }

    private IEnumerator RandomStateSwitchLoop()
    {
        while (true)
        {
            float delay = currentState == IdleState.Dancing
                ? maxDanceDuration
                : Random.Range(minSwitchDelay, maxSwitchDelay);

            yield return new WaitForSeconds(delay);
            SwitchToRandomState();
        }
    }

    private void SwitchToRandomState()
    {
        IdleState nextState;
        do
        {
            nextState = (IdleState)Random.Range(0, 3);
        } while (nextState == currentState);

        switch (nextState)
        {
            case IdleState.Idle:
                animator.SetTrigger(IdleTrigger);
                break;
            case IdleState.SitIdle:
                animator.SetTrigger(SitIdleTrigger);
                break;
            case IdleState.Dancing:
                animator.SetTrigger(DancingTrigger);
                break;
        }

        currentState = nextState;
    }
}
