using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCycleManager : MonoBehaviour
{
    public Transform[] cardSpawnPoints; 
    public GameObject cardPrefab;       

    private Queue<UnitCost> deckQueue = new Queue<UnitCost>();
    //private List<CardDrag> activeCards = new List<CardDrag>();
    private List<CardDragXR> activeCards = new List<CardDragXR>();

    IEnumerator Start()
    {
        while (XRPlayer.Instance == null || XRPlayer.Instance.leftController == null)
            yield return null;

        foreach (var card in DeckManager.Instance.selectedDeck)
            deckQueue.Enqueue(card);

        for (int i = 0; i < cardSpawnPoints.Length; i++)
            SpawnNextCardIntoSlot(i);
    }


    private void SpawnNextCardIntoSlot(int slotIndex)
    {
        if (deckQueue.Count == 0)
        {
            foreach (var c in DeckManager.Instance.selectedDeck)
                deckQueue.Enqueue(c);
        }

        UnitCost data = deckQueue.Dequeue();

        // Создаём карту в точке спавна
        GameObject cardObj = Instantiate(
            cardPrefab,
            cardSpawnPoints[slotIndex].position,
            cardSpawnPoints[slotIndex].rotation
        );

        // Делаем её дочерней левой руки, но С СОХРАНЕНИЕМ мировой позиции
        cardObj.transform.SetParent(XRPlayer.Instance.leftController, true);

        CardDragXR card = cardObj.GetComponent<CardDragXR>();
        card.data = data;
        card.rightController = XRPlayer.Instance.rightController;
        card.homeSlot = cardSpawnPoints[slotIndex];

        activeCards.Insert(slotIndex, card);
        deckQueue.Enqueue(data);
    }
    public void OnCardUsed(CardDragXR usedCard)
    {
        int slotIndex = activeCards.IndexOf(usedCard);

        activeCards.RemoveAt(slotIndex);
        Destroy(usedCard.gameObject);

        SpawnNextCardIntoSlot(slotIndex);
    }
}
