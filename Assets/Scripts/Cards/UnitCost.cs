using UnityEngine;

public enum CardType
{
    Unit,
    Spell
}

public enum SpellType
{
    None,
    Damage,
    Freeze,
    Heal
}

[CreateAssetMenu(fileName = "UnitData", menuName = "Game/Unit Data")]
public class UnitCost : ScriptableObject
{
    public string unitName;
    public Sprite icon;

    [Header("Card Type")]
    public CardType cardType = CardType.Unit;
    public SpellType spellType = SpellType.None;

    [Header("Unit Properties")]
    public bool isFlying = false;
    public bool canTargetAir = false;

    [Header("Multiple unit prefabs")]
    public GameObject[] prefabs;

    public int elixirCost;

    [Header("Optional spawn offsets")]
    public Vector3[] spawnOffsets;

    public bool IsSpell() => cardType == CardType.Spell;
    public bool IsUnit() => cardType == CardType.Unit;
}
