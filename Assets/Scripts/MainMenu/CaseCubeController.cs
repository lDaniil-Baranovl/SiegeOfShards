using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
public class CaseCubeController : MonoBehaviour
{
    [Header("Возможные карты для выпадения")]
    public UnitCost[] possibleCards;

    [Header("Отображение награды")]
    public Image rewardIcon;
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

        while (rb.velocity.magnitude > 0.1f || rb.angularVelocity.magnitude > 0.1f)
            yield return null;

        GiveRandomReward();
    }

    private void GiveRandomReward()
    {
        if (rewardGiven) return;
        rewardGiven = true;

        UnitCost card = possibleCards[Random.Range(0, possibleCards.Length)];
        int amount = Random.Range(minFragments, maxFragments + 1);

        CardUpgradeManager.Instance.AddFragments(card, amount);

        rewardIcon.sprite = card.icon;
        rewardText.text = $"+{amount}";

        rewardFace.LookAt(Camera.main.transform);
    }
}
