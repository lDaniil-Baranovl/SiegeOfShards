using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
// Компонент требует наличия ARBoundingBox
[RequireComponent(typeof(ARBoundingBox))]
public class ARBoundingBoxColorizer : MonoBehaviour
{
    // Свойство для управления визуализацией границ
    public bool isVisualise
    {
        get => _meshRenderer.enabled; // Получаем текущее состояние видимости
        set => _meshRenderer.enabled = value; // Устанавливаем видимость
    }
    [SerializeField] private float defaultColorAlpha = 0.25f; // Прозрачность цвета по умолчанию
  private Color _defaultColor; // Цвет по умолчанию, определенный на основе классификации
  private ARBoundingBox _boundingBox; // Компонент ARBoundingBox
    private MeshRenderer _meshRenderer; // Компонент MeshRenderer
    private void Awake()
    {
        // Инициализация компонентов
        _boundingBox = GetComponent<ARBoundingBox>();
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (_boundingBox == null || _meshRenderer == null)
        {
            Debug.LogError("ARBoundingBox или MeshRenderer компонент отсутствует.");
            return;
        }
        // Устанавливаем цвет меша (границ) на основе классификации
        var defaultColor =
  GetColorByClassification(_boundingBox.classifications);
        defaultColor.a = defaultColorAlpha; // Устанавливаем прозрачность цвета
        _meshRenderer.material.color = defaultColor;
        // Устанавливаем размер меша (границ)
        _meshRenderer.transform.localScale = _boundingBox.size;
        // Устанавливаем начальное состояние визуализации
        _meshRenderer.enabled = isVisualise;
    }
    // Метод для получения цвета на основе классификации границ
    private static Color
  GetColorByClassification(BoundingBoxClassifications classification)
    {
        return classification switch
        {
        BoundingBoxClassifications.Couch => Color.blue,
        BoundingBoxClassifications.Table => Color.yellow,
        BoundingBoxClassifications.Bed => Color.cyan,
        BoundingBoxClassifications.Lamp => Color.magenta,
        BoundingBoxClassifications.Plant => Color.green,
        BoundingBoxClassifications.Screen => Color.white,
        BoundingBoxClassifications.Storage => Color.red,
        BoundingBoxClassifications.None => Color.gray,
        BoundingBoxClassifications.Other => Color.gray,
        _ => Color.gray // Цвет по умолчанию для неизвестной классификации
        };
    }
}
