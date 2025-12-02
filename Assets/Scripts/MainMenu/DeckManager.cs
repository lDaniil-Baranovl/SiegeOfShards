using UnityEngine;
using System.Collections.Generic;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    [Header("¬ыбранные игроком 8 карт")]
    public List<UnitCost> selectedDeck = new List<UnitCost>();

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public bool AddToDeck(UnitCost card)
    {
        if (selectedDeck.Contains(card))
        {
            Debug.Log("Ёта карта уже есть в колоде!");
            return false;
        }
        if (selectedDeck.Count >= 8)
            return false;

        selectedDeck.Add(card);

        DeckUI.Instance.RefreshUI();

        return true;
    }

    public void RemoveFromDeck(UnitCost card)
    {
        selectedDeck.Remove(card);

        DeckUI.Instance.RefreshUI();
    }

    public void ClearDeck()
    {
        selectedDeck.Clear();

        DeckUI.Instance.RefreshUI();
    }

}
