using UnityEngine;
using TMPro;

public class CardRewardPopupController : MonoBehaviour
{
    public CardVisual cardVisual;
    public TextMeshProUGUI amountText;

    public void Show(Sprite icon, int amount, float duration)
    {
        if (cardVisual != null)
            cardVisual.SetIcon(icon);

        if (amountText != null)
            amountText.text = $"+{amount}";

        if (Camera.main != null)
            transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);

        Destroy(gameObject, duration);
    }
}
