using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeckUI : MonoBehaviour
{
    public static DeckUI Instance;

    public GameObject[] buttonsEdit;
    private bool editMode = false;

    private UnitCost[] slotCards = new UnitCost[8];
    

    [Header("8 UI слотов под карты игрока")]
    public Button[] deckSlots;
    [Header(" нопки-крестики дл€ удалени€ карт (по одному на слот)")]
    public Image[] removeButtons;
    private void Awake()
    {
        Instance = this;

        deckSlots = Resources.FindObjectsOfTypeAll<Button>()
            .Where(img => img.CompareTag("DeckSlot"))
            .OrderBy(img => img.name)
            .ToArray();

        removeButtons = Resources.FindObjectsOfTypeAll<Image>()
            .Where(btn => btn.CompareTag("DeckRemove"))
            .OrderBy(btn => btn.name)
            .ToArray();
        foreach (var slot in deckSlots)
        {
            slot.transition = Selectable.Transition.None;
            slot.interactable = false;
        }
    }
    private void OnEnable() { if (DeckManager.Instance != null) RefreshUI(); }

    public void RefreshUI()
    {
        var deck = DeckManager.Instance.selectedDeck;

        for (int i = 0; i < deckSlots.Length; i++)
        {
            Image img = deckSlots[i].GetComponent<Image>();

            if (i < deck.Count)
            {
                var card = deck[i];
                slotCards[i] = card; // сохран€ем!

                img.sprite = card.icon;
                img.color = Color.white;
            }
            else
            {
                slotCards[i] = null; // слот пустой

                img.sprite = null;
                img.color = new Color(1, 1, 1, 0);
            }
        }
    }

    public void OnEdit()
    {
        editMode = !editMode;

        foreach (var button in buttonsEdit)
            button.gameObject.SetActive(editMode);

        for (int i = 0; i < deckSlots.Length; i++)
        {
            var slot = deckSlots[i];

            slot.onClick.RemoveAllListeners();

            if (editMode)
            {
                int index = i;
                slot.onClick.AddListener(() => RemoveCard(index));
                slot.interactable = true;
            }
            else
            {
                slot.interactable = false;
            }
        }
    }


    public void RemoveCard(int slotIndex)
    {
        UnitCost card = slotCards[slotIndex];

        if (card == null)
            return;

        DeckManager.Instance.RemoveFromDeck(card);
    }

}
