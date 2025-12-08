using UnityEngine;

public class SpawnInHand : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform handPoint;
    public int casePrice = 150;

    private GameObject currentItem;

    public void SpawnItem()
    {
        // 1. Проверяем есть ли попытки
        bool canUse = TryOpenCaseManager.Instance.UseCaseAttempt();

        if (!canUse)
        {
            Debug.Log("Нет попыток! Попробую купить...");

            // 2. Если попытки закончились — пытаемся купить кейс
            bool bought = TryOpenCaseManager.Instance.BuyCase(casePrice);

            if (!bought)
            {
                Debug.Log("Не хватает золота для покупки кейса!");
                return;
            }

            Debug.Log("Кейс куплен за золото!");
        }

        // 3. Спавним предмет
        if (currentItem != null)
            Destroy(currentItem);

        currentItem = Instantiate(itemPrefab, handPoint);
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;

        Debug.Log("Кейс открыт, предмет выдан!");
    }
}
