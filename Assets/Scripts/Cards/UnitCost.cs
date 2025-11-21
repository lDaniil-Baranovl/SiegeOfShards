using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Game/Unit Data")]
public class UnitCost : ScriptableObject
{
    public string unitName;

    [Header("Multiple unit prefabs")]
    public GameObject[] prefabs;

    public int elixirCost;

    [Header("Optional spawn offsets")]
    public Vector3[] spawnOffsets;
}
