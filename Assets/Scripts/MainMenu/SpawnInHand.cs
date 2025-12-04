using UnityEngine;

public class SpawnInHand : MonoBehaviour
{
    [Header("Префаб, который появится в руке")]
    public GameObject itemPrefab;

    [Header("Точка, где будет появляться предмет (кость руки)")]
    public Transform handPoint;

    private GameObject currentItem;

    // Вызывайте этот метод через кнопку Unity (OnClick)
    public void SpawnItem()
    {
        // Если в руке уже есть предмет — удаляем
        if (currentItem != null)
        {
            Destroy(currentItem);
        }

        // Создаём объект
        currentItem = Instantiate(itemPrefab, handPoint);

        // Выравниваем позицию и поворот относительно руки
        currentItem.transform.localPosition = Vector3.zero;
        currentItem.transform.localRotation = Quaternion.identity;
    }
}
