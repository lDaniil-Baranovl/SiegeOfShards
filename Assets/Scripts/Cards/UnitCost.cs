using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Game/Unit Data")]
public class UnitCost : ScriptableObject
{
    public string unitName;
    public GameObject prefab;
    public int elixirCost;
}
