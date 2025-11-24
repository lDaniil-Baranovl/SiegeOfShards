using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
[RequireComponent(typeof(ARPlaneManager))]
[RequireComponent(typeof(ARBoundingBoxManager))]
public class DebugMode : MonoBehaviour
{
    [SerializeField]
    private InputActionReference toggleSurfaceRenderingAction; // Действие ввода для переключения визуализа
    [SerializeField] private bool isVisualiseOnStart; // Начальное состоян визуализации
    private bool _isVisualise; // Текущее состояние визуализации
    private ARPlaneManager _planeManager; // Компонент для управления ARплоскостями
    private ARBoundingBoxManager _boundingBoxManager; // Компонент для управления AR-границами
    [SerializeField] private Canvas debugPanel;
    private void Awake()
    {
        // Инициализация компонентов
        _planeManager = GetComponent<ARPlaneManager>();
        _boundingBoxManager = GetComponent<ARBoundingBoxManager>();
        _isVisualise = isVisualiseOnStart; // Устанавливаем начальное состояние визуализации
    }
    public void OnEnable()
    {
        // Подписка на действие ввода для переключения визуализации
        toggleSurfaceRenderingAction.action.started += OnToggleSurfaceRendering;
        // Подписка на события изменения AR-плоскостей и границ
        _planeManager.trackablesChanged.AddListener(OnPlanesChanged);

        _boundingBoxManager.trackablesChanged.AddListener(OnBoundingBoxesChanged);
        // Обновление начальной визуализации PlaneUpdateVisualisation();
        BoundingBoxUpdateVisualisation();
    }
    public void OnDisable()
    {
        // Отписка от действия ввода
        toggleSurfaceRenderingAction.action.started -= OnToggleSurfaceRendering;
        // Отписка от событий изменения AR-плоскостей и границ
        _planeManager.trackablesChanged.RemoveListener(OnPlanesChanged);
        _boundingBoxManager.trackablesChanged.RemoveListener(OnBoundingBoxesChanged);
    }
    // Метод-обработчик для изменения состояния AR-плоскостей
    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> arg)
    {
        PlaneUpdateVisualisation();
    }
    //Метод обработчик для изменения состояния AR-границ
    private void OnBoundingBoxesChanged(ARTrackablesChangedEventArgs<ARBoundingBox> arg)
    {
        BoundingBoxUpdateVisualisation();
    }
    // Метод-обработчик для переключения визуализации при срабатывании действия ввода
    private void OnToggleSurfaceRendering(InputAction.CallbackContext obj)
    {
        debugPanel.enabled = _isVisualise;
        _isVisualise = !_isVisualise; // Переключение состояния визуализац
        PlaneUpdateVisualisation();
        BoundingBoxUpdateVisualisation();
    }
    // Обновление визуализации AR-плоскостей
    private void PlaneUpdateVisualisation()
    {
        foreach (var arPlane in _planeManager.trackables)
        {
            if (arPlane.TryGetComponent(out ARPlaneColorizer arPlaneColorizer))
            {
                arPlaneColorizer.isVisualise = _isVisualise; // Устанавливаем состояние визуализации
            }
        }
    }
    // Обновление визуализации AR-границ
    private void BoundingBoxUpdateVisualisation()
    {
        foreach (var arBoundingBox in _boundingBoxManager.trackables)
        {
            if (arBoundingBox.TryGetComponent(out ARBoundingBoxColorizer arBoundingBoxColorizer))
            {
                arBoundingBoxColorizer.isVisualise = _isVisualise; // Устанавливаем состояние визуализации
            }
        }
    }
}
