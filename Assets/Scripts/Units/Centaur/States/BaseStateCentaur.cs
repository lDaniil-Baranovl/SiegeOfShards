using UnityEngine;

public abstract class CentaurBaseState
{
    public abstract void EnterState(CentaurStateManager manager);
    public abstract void ExitState(CentaurStateManager manager);
    public abstract void UpdateState(CentaurStateManager manager);

}