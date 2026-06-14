using TMPro;
using UnityEngine;

public class SpawnInHand : MonoBehaviour
{
    public GameObject itemPrefab;
    public TextMeshProUGUI priceText;
    public int casePrice = 150;

    [Header("Появление сундука перед игроком")]
    public float spawnDistance = 0.8f;
    public float spawnHeightOffset = -0.3f;

    private GameObject currentItem;

    private void Start()
    {
        UpdatePriceUI();
    }

    private void UpdatePriceUI()
    {
        if (priceText != null)
            priceText.text = casePrice.ToString();
    }

    public void SpawnItem()
    {
        // 1. проверяем, есть ли бесплатная попытка
        bool canUse = TryOpenCaseManager.Instance.UseCaseAttempt();

        if (!canUse)
        {
            // 2. если попыток нет — покупаем за золото
            bool bought = TryOpenCaseManager.Instance.BuyCase(casePrice);

            if (!bought)
            {
                Debug.Log("Не хватает золота для покупки сундука!");
                return;
            }

            Debug.Log("Сундук куплен за золото!");
        }

        // 3. убираем предыдущий сундук, если он ещё валяется
        if (currentItem != null)
            Destroy(currentItem);

        // 4. спавним сундук перед игроком
        Transform head = Camera.main.transform;
        Vector3 spawnPos = head.position + head.forward * spawnDistance;
        spawnPos.y += spawnHeightOffset;

        Quaternion spawnRot = Quaternion.LookRotation(head.forward, Vector3.up);

        currentItem = Instantiate(itemPrefab, spawnPos, spawnRot);

        Debug.Log("Сундук появился перед игроком, можно бросать!");
    }
}
