using UnityEngine;

public abstract class FlyColdDragonBaseState
{
    public abstract void EnterState(StateManagerFlyColdDragon manager);
    public abstract void ExitState(StateManagerFlyColdDragon manager);
    public abstract void UpdateState(StateManagerFlyColdDragon manager);
}
