using UnityEngine;

public abstract class UnitBaseState<T> where T : UnitStateManager
{
    public abstract void EnterState(T manager);
    public abstract void ExitState(T manager);
    public abstract void UpdateState(T manager);
}