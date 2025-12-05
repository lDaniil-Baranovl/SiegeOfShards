using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardCycleManager : MonoBehaviour
{
    public Transform[] cardSpawnPoints;
    public GameObject cardPrefab;

    private CardDragXR hovered;

    private Queue<UnitCost> deckQueue = new Queue<UnitCost>();
    //private List<CardDrag> activeCards = new List<CardDrag>();
    private List<CardDragXR> activeCards = new List<CardDragXR>();

    IEnumerator Start()
    {
        while (XRPlayer.Instance == null || XRPlayer.Instance.leftController == null)
            yield return null;

        HashSet<string> usedNames = new HashSet<string>();
        foreach (var card in DeckManager.Instance.selectedDeck)
        {
            if (!usedNames.Contains(card.unitName))
            {
                usedNames.Add(card.unitName);
                deckQueue.Enqueue(card);
            }
        }

        for (int i = 0; i < cardSpawnPoints.Length; i++)
            SpawnNextCardIntoSlot(i);
    }
    void Update()
    {
        Transform rc = XRPlayer.Instance.rightController;
        Ray ray = new Ray(rc.position, rc.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            CardDragXR card = hit.collider.GetComponentInParent<CardDragXR>();

            if (card != hovered)
            {
                if (hovered != null) hovered.OnHoverExited();
                if (card != null) card.OnHoverEntered();

                hovered = card;
            }
        }
        else
        {
            if (hovered != null)
            {
                hovered.OnHoverExited();
                hovered = null;
            }
        }
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

        CardVisual visual = cardObj.GetComponent<CardVisual>();
        if (visual != null)
            visual.SetIcon(data.icon);

        CardDragXR card = cardObj.GetComponent<CardDragXR>();
        card.data = data;
        card.rightController = XRPlayer.Instance.rightController;
        card.homeSlot = cardSpawnPoints[slotIndex];

        if (slotIndex <= activeCards.Count)
            activeCards.Insert(slotIndex, card);
        else
            activeCards.Add(card);
    }
    public void OnCardUsed(CardDragXR usedCard)
    {
        int slotIndex = activeCards.IndexOf(usedCard);
        if (slotIndex < 0)
        {
            Debug.LogWarning("Попытка использовать карту, которой нет в activeCards: " + usedCard.name);
            Destroy(usedCard.gameObject);
            // можно попытаться найти свободный слот и SpawnNextCardIntoSlot,
            // но лучше логировать и вернуться.
            return;
        }

        activeCards.RemoveAt(slotIndex);
        Destroy(usedCard.gameObject);
        deckQueue.Enqueue(usedCard.data);
        SpawnNextCardIntoSlot(slotIndex);
    }

}