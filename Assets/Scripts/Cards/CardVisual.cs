using UnityEngine;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour
{
    public Image iconUI;

    public void SetIcon(Sprite sprite)
    {
        if (iconUI != null)
        {
            iconUI.sprite = sprite;
            iconUI.color = Color.white;
        }
    }
}
