using UnityEngine;
using UnityEngine.UI;

public class ScaleWithSlider : MonoBehaviour
{
    public Transform arenaRoot;
    public Slider slider;

    public float minScale = 0.2f;
    public float maxScale = 3f;

    void Start()
    {
        slider.onValueChanged.AddListener(OnSliderChanged);
        slider.minValue = 0f;
        slider.maxValue = 1f;

        // Инициализация
        float current = Mathf.InverseLerp(minScale, maxScale, arenaRoot.localScale.x);
        slider.value = current;
    }

    private void OnSliderChanged(float value)
    {
        float scale = Mathf.Lerp(minScale, maxScale, value);
        arenaRoot.localScale = new Vector3(scale, scale, scale);
    }
}
