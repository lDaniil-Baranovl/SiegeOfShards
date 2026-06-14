using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class CaseCubeController : MonoBehaviour
{
    [Header("Возможные карты для выпадения")]
    public UnitCost[] possibleCards;

    [Header("UI награды (4 картинки)")]
    public Image rewardIcon; // ← 4 Image в инспекторе
    public TextMeshProUGUI rewardText;
    public Transform rewardFace;

    [Header("Настройки выпадения")]
    public int minFragments = 3;
    public int maxFragments = 10;

    private bool rewardGiven = false;

    public void OnCubeThrown()
    {
        StartCoroutine(WaitForSettle());
    }

    private IEnumerator WaitForSettle()
    {
        Rigidbody rb = GetComponent<Rigidbody>();

        yield return new WaitForSeconds(1f);

        while (rb.linearVelocity.magnitude > 0.1f ||
               rb.angularVelocity.magnitude > 0.1f)
        {
            yield return null;
        }

        GiveRandomReward();
    }

    private void GiveRandomReward()
    {
        if (rewardGiven) return;
        rewardGiven = true;

        if (possibleCards == null || possibleCards.Length == 0)
        {
            Debug.Log("Сундук открыт! (награды пока не настроены)");
            return;
        }

        // 1. Выбираем героя
        UnitCost card = possibleCards[Random.Range(0, possibleCards.Length)];
        int amount = Random.Range(minFragments, maxFragments + 1);

        // 2. Начисляем фрагменты
        CardUpgradeManager.Instance.AddFragments(card, amount);

        // 3. Показываем спрайт героя в UI награды (если оно назначено)
        if (rewardIcon != null)
        {
            rewardIcon.sprite = card.icon;
            rewardIcon.enabled = true;
        }

        // 4. Текст награды
        if (rewardText != null)
            rewardText.text = $"+{amount}";

        // 5. Поворачиваем UI к камере (если это 3D)
        if (rewardFace != null && Camera.main != null)
            rewardFace.LookAt(Camera.main.transform);
    }
}
