using UnityEngine;
using System.Collections.Generic;

public class CardCycleManager : MonoBehaviour
{
    public Transform cardSlotsParent;
    public GameObject cardPrefab;

    private Queue<UnitCost> deckQueue = new Queue<UnitCost>();
    private List<CardDrag> activeCards = new List<CardDrag>();

    void Start()
    {
        foreach (var card in DeckManager.Instance.selectedDeck)
            deckQueue.Enqueue(card);

        for (int i = 0; i < 4; i++)
            SpawnNextCardIntoSlot(i);
    }

    private void SpawnNextCardIntoSlot(int slotIndex)
    {
        UnitCost data = deckQueue.Dequeue();

        GameObject cardObj = Instantiate(cardPrefab, cardSlotsParent.GetChild(slotIndex));
        CardDrag card = cardObj.GetComponent<CardDrag>();
        card.data = data;

        activeCards.Add(card);

        deckQueue.Enqueue(data);
    }

    public void OnCardUsed(CardDrag usedCard)
    {
        int slotIndex = activeCards.IndexOf(usedCard);
        activeCards.Remove(usedCard);

        Destroy(usedCard.gameObject);

        SpawnNextCardIntoSlot(slotIndex);
    }
}
