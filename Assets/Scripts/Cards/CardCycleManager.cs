using UnityEngine;
using System.Collections.Generic;

public class CardCycleManager : MonoBehaviour
{
    public Transform[] cardSpawnPoints; 
    public GameObject cardPrefab;       

    private Queue<UnitCost> deckQueue = new Queue<UnitCost>();
    private List<CardDrag> activeCards = new List<CardDrag>();

    void Start()
    {
        if (DeckManager.Instance == null || DeckManager.Instance.selectedDeck.Count == 0)
        {
            Debug.LogError("DeckManager пуст! Карты не могут быть созданы.");
            return;
        }

        foreach (var card in DeckManager.Instance.selectedDeck)
            deckQueue.Enqueue(card);

        for (int i = 0; i < cardSpawnPoints.Length; i++)
            SpawnNextCardIntoSlot(i);
    }

    private void SpawnNextCardIntoSlot(int slotIndex)
    {
        if (deckQueue.Count == 0)
        {
            foreach (var cardd in DeckManager.Instance.selectedDeck)
                deckQueue.Enqueue(cardd);
        }

        UnitCost data = deckQueue.Dequeue();

        GameObject cardObj = Instantiate(
            cardPrefab,
            cardSpawnPoints[slotIndex].position,
            cardSpawnPoints[slotIndex].rotation
        );

        CardDrag card = cardObj.GetComponent<CardDrag>();
        card.data = data;

        activeCards.Insert(slotIndex, card);

        deckQueue.Enqueue(data);
    }


    public void OnCardUsed(CardDrag usedCard)
    {
        int slotIndex = activeCards.IndexOf(usedCard);

        if (slotIndex < 0)
        {
            Debug.LogError("Карты нет в activeCards!");
            return;
        }

        activeCards.RemoveAt(slotIndex);
        Destroy(usedCard.gameObject);

        SpawnNextCardIntoSlot(slotIndex);
    }
}
