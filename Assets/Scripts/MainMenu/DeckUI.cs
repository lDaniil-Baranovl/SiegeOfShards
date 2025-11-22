using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DeckUI : MonoBehaviour
{
    public Transform deckSlotsParent;
    private List<Image> slotImages = new List<Image>();

    private void Start()
    {
        foreach (Transform t in deckSlotsParent)
            slotImages.Add(t.GetComponent<Image>());
    }

    private void Update()
    {
        UpdateDeckUI();
    }

    private void UpdateDeckUI()
    {
        for (int i = 0; i < slotImages.Count; i++)
        {
            if (i < DeckManager.Instance.selectedDeck.Count)
            {
                slotImages[i].color = Color.white;
                slotImages[i].sprite = DeckManager.Instance.selectedDeck[i].prefabs[0].GetComponent<SpriteRenderer>().sprite;
            }
            else
            {
                slotImages[i].color = new Color(1, 1, 1, 0.3f);
                slotImages[i].sprite = null;
            }
        }
    }
}
