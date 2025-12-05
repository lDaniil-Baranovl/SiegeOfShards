using UnityEngine;

public class FixPosition : MonoBehaviour
{
    public GameObject handle;       // сам Handle целиком
    public GameObject sliderUI;     // Canvas или сам Slider

    public void Fix()
    {
        handle.SetActive(false);
        sliderUI.SetActive(false);

        Debug.Log("Поле закреплено: handle и slider отключены.");
    }
}
