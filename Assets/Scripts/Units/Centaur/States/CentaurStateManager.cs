using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.AI;

public class CentaurStateManager : MonoBehaviour
{
    [SerializeField] public Animator centaur_animator;
    [SerializeField] public NavMeshAgent centaur_navMeshAgent;
    [SerializeField] public Transform[] tower_enemy;
    [SerializeField] public Transform enemy;
    public bool canMove = false;

    CentaurBaseState currentState;
    //public CentaurIdleState centaurIdleState = new CentaurIdleState();
    public void SwitchState(CentaurBaseState newState)
    {
        if (currentState != null)
        {
            currentState.ExitState(this);
        }
        currentState = newState;
        currentState.EnterState(this);
    }
    private void Start()
    {
        //SwitchState(centaurIdleState);
    }

}
