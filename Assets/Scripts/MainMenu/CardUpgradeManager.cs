using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CardData
{
    public int level;
    public int fragments;

    public CardData(int level, int fragments)
    {
        this.level = level;
        this.fragments = fragments;
    }
}

public class CardUpgradeManager : MonoBehaviour
{
    public static CardUpgradeManager Instance;
    private Dictionary<UnitCost, CardData> cards = new Dictionary<UnitCost, CardData>();

    private const int FragRequired = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public CardData GetCard(UnitCost unit)
    {
        if (!cards.ContainsKey(unit))
        {
            int level = PlayerPrefs.GetInt($"card_{unit.unitName}_level", 1);
            int frags = PlayerPrefs.GetInt($"card_{unit.unitName}_frags", 0);

            cards[unit] = new CardData(level, frags);
        }

        return cards[unit];
    }

    public bool TryUpgrade(UnitCost unit, int cost)
    {
        CardData card = GetCard(unit);

        if (card.fragments < FragRequired)
            return false;

        if (GoldManager.Instance.Gold < cost)
            return false;

        // ���������
        card.fragments -= FragRequired;
        card.level++;
        GoldManager.Instance.AddGold(-cost);

        SaveAllCards();
        return true;
    }

    public void AddFragments(UnitCost unit, int amount)
    {
        CardData card = GetCard(unit);
        card.fragments += amount;
        SaveAllCards();
    }

    private void SaveAllCards()
    {
        foreach (var pair in cards)
        {
            UnitCost unit = pair.Key;
            CardData c = pair.Value;

            string id = unit.unitName;

            PlayerPrefs.SetInt($"card_{id}_level", c.level);
            PlayerPrefs.SetInt($"card_{id}_frags", c.fragments);
        }

        PlayerPrefs.Save();
    }
}
