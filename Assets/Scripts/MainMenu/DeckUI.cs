using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeckUI : MonoBehaviour
{
    public static DeckUI Instance;

    [Header("8 UI слотов под карты игрока")]
    public Image[] deckSlots;
    [Header(" нопки-крестики дл€ удалени€ карт (по одному на слот)")]
    public Button[] removeButtons;
    private void Awake()
    {
        Instance = this;

        deckSlots = Resources.FindObjectsOfTypeAll<Image>()
            .Where(img => img.CompareTag("DeckSlot"))
            .OrderBy(img => img.name)
            .ToArray();

        removeButtons = Resources.FindObjectsOfTypeAll<Button>()
            .Where(btn => btn.CompareTag("DeckRemove"))
            .OrderBy(btn => btn.name)
            .ToArray();

        for (int i = 0; i < removeButtons.Length; i++)
        {
            int index = i;
            removeButtons[i].onClick.AddListener(() => RemoveCardAtIndex(index));
        }
    }

    public void RefreshUI()
    {
        int count = DeckManager.Instance.selectedDeck.Count;

        for (int i = 0; i < deckSlots.Length; i++)
        {
            if (i < count)
            {
                var card = DeckManager.Instance.selectedDeck[i];
                deckSlots[i].sprite = card.icon;
                deckSlots[i].color = Color.white;
            }
            else
            {
                deckSlots[i].sprite = null;
                deckSlots[i].color = new Color(1, 1, 1, 0);
            }
        }
    }
    public void RemoveCardAtIndex(int index)
    {
        if (index < 0 || index >= DeckManager.Instance.selectedDeck.Count)
            return;

        DeckManager.Instance.selectedDeck.RemoveAt(index);
        RefreshUI();
    }

}
