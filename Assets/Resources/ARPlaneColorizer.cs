using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
[RequireComponent(typeof(ARPlane))]
[RequireComponent(typeof(MeshRenderer))]
public class ARPlaneColorizer : MonoBehaviour
{
    // Свойство для управления визуализацией плоскости
    public bool isVisualise
    {
        get => _isVisualise;
        set
        {
            _isVisualise = value;
            UpdateColor(); // Обновляем цвет при изменении значения
        }
    }
    private const float DEFAULT_COLOR_ALPHA = 0.25f; // Прозрачность цвета по умолчанию
    private bool _isVisualise; // Внутреннее поле для свойства isVisualise
    private Color _defaultColor; // Цвет по умолчанию, определенный на основе классификации
    private ARPlane _arPlane; // Компонент ARPlane
    private MeshRenderer _meshRenderer; // Компонент MeshRenderer
    private LineRenderer _lineRenderer; // Компонент LineRenderer
    private void Awake()
    {
        // Инициализация компонентов
        _arPlane = GetComponent<ARPlane>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _lineRenderer = GetComponent<LineRenderer>();
        if (_arPlane == null || _meshRenderer == null)
        {
            Debug.LogError("ARPlane или MeshRenderer компонент отсутствует.");
            return;
        }
        // Получаем цвет материала по классификации
        _defaultColor = GetColorByClassification(_arPlane.classifications);
        _defaultColor.a = DEFAULT_COLOR_ALPHA; // Устанавливаем прозрачность цвета
        // Устанавливаем начальный цвет UpdateColor();
        // Подписка на событие изменения границ плоскости
        _arPlane.boundaryChanged += OnPlaneBoundaryChanged;
    }
    private void OnDestroy()
    {
        // Отписываемся от события при уничтожении объекта
        if (_arPlane != null)
        {
            _arPlane.boundaryChanged -= OnPlaneBoundaryChanged;
        }
    }
    private void OnPlaneBoundaryChanged(ARPlaneBoundaryChangedEventArgs eventArgs)
    {
        // Обновляем цвет при изменении границ плоскости
        UpdateColor();
    }
    // Метод для обновления цвета материала плоскости
    private void UpdateColor()
    {
        _meshRenderer.materials[0].color = _isVisualise ? Color.clear :_defaultColor;
        _lineRenderer.startColor = _isVisualise ? Color.clear : Color.white;
    }
    // Метод для получения цвета на основе классификации плоскости
    private static Color GetColorByClassification(PlaneClassifications classifications)
    {
        return classifications switch
        {
            PlaneClassifications.Floor => Color.green,
            PlaneClassifications.WallFace => Color.white,
            PlaneClassifications.Ceiling => Color.red,
            PlaneClassifications.Table => Color.yellow,
            PlaneClassifications.Couch => Color.blue,
            PlaneClassifications.Seat => Color.blue,
            PlaneClassifications.SeatOfAnyType => Color.blue,
            PlaneClassifications.WallArt => new Color(1f, 0.4f, 0f),
            //orange
            PlaneClassifications.DoorFrame => Color.magenta,
            PlaneClassifications.WindowFrame => Color.cyan,
            _ => Color.gray // Цвет по умолчанию
        };
    }
}