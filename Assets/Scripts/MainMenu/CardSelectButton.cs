using UnityEngine;
using UnityEngine.UI;

public class CardSelectButton : MonoBehaviour
{
    public UnitCost cardData;

    public Button button;

    private void Start()
    {
        button.onClick.AddListener(OnSelect);
    }

    private void OnSelect()
    {
        if (DeckManager.Instance.AddToDeck(cardData))
        {
            Debug.Log(cardData.unitName + " добавлена в колоду!");
        }
        else
        {
            Debug.Log(" олода заполнена (8 карт)");
        }
    }
}
