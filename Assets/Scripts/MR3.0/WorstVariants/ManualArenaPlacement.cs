//using UnityEngine;
//using UnityEngine.InputSystem;
//using UnityEngine.XR.ARFoundation;

///// <summary>
///// Альтернативная система размещения арены - ручное размещение с помощью контроллера
///// Полезна как fallback если автоматическое распознавание столов не работает
///// </summary>
//public class ManualArenaPlacement : MonoBehaviour
//{
//    [Header("Arena Settings")]
//    [SerializeField] private GameObject arenaPrefab;
//    [SerializeField] private float raycastDistance = 5f;
//    [SerializeField] private LayerMask placementLayers; // Слои для размещения (например, Spatial Mesh)
//    [SerializeField] private float arenaHeightOffset = 0.05f; // Смещение вверх над поверхностью

//    [Header("XR Input")]
//    [SerializeField] private Transform rightController;
//    [SerializeField] private InputActionReference aimAction;
//    [SerializeField] private InputActionReference placeAction;

//    [Header("Visual Feedback")]
//    [SerializeField] private GameObject previewPrefab;
//    [SerializeField] private Color validColor = new Color(0, 1, 0, 0.5f);
//    [SerializeField] private Color invalidColor = new Color(1, 0, 0, 0.5f);

//    [Header("AR Components")]
//    [SerializeField] private ARAnchorManager anchorManager;

//    // Runtime state
//    private GameObject previewInstance;
//    private GameObject arenaInstance;
//    private ARAnchor arenaAnchor;
//    private bool isArenaPlaced = false;
//    private bool isValidPlacement = false;
//    private Vector3 targetPosition;
//    private Quaternion targetRotation;

//    private void Awake()
//    {
//        if (anchorManager == null)
//        {
//            anchorManager = FindObjectOfType<ARAnchorManager>();
//        }

//        CreatePreview();
//    }

//    private void OnEnable()
//    {
//        if (placeAction != null)
//        {
//            placeAction.action.performed += OnPlacePressed;
//        }
//    }

//    private void OnDisable()
//    {
//        if (placeAction != null)
//        {
//            placeAction.action.performed -= OnPlacePressed;
//        }
//    }

//    private void Update()
//    {
//        if (isArenaPlaced) return;

//        UpdatePreviewPosition();
//    }

//    private void CreatePreview()
//    {
//        if (previewPrefab != null)
//        {
//            previewInstance = Instantiate(previewPrefab);
//        }
//        else if (arenaPrefab != null)
//        {
//            // Используем сам префаб арены как превью
//            previewInstance = Instantiate(arenaPrefab);

//            // Делаем полупрозрачным
//            MakeTransparent(previewInstance.transform);
//        }

//        if (previewInstance != null)
//        {
//            previewInstance.name = "ArenaPreview";
//            previewInstance.SetActive(false);

//            // Отключаем коллайдеры у превью
//            foreach (var collider in previewInstance.GetComponentsInChildren<Collider>())
//            {
//                collider.enabled = false;
//            }
//        }
//    }

//    private void MakeTransparent(Transform root)
//    {
//        foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>())
//        {
//            foreach (var mat in renderer.materials)
//            {
//                // Try to set transparent mode if supported
//                if (mat.HasProperty("_Surface"))
//                    mat.SetFloat("_Surface", 1); // Transparent

//                if (mat.HasProperty("_Blend"))
//                    mat.SetFloat("_Blend", 0);   // Alpha

//                // Try different color properties (different shaders use different names)
//                if (mat.HasProperty("_Color"))
//                {
//                    Color color = mat.color;
//                    color.a = 0.5f;
//                    mat.color = color;
//                }
//                else if (mat.HasProperty("_BaseColor"))
//                {
//                    Color color = mat.GetColor("_BaseColor");
//                    color.a = 0.5f;
//                    mat.SetColor("_BaseColor", color);
//                }
//            }
//        }
//    }

//    private void UpdatePreviewPosition()
//    {
//        if (rightController == null || previewInstance == null)
//        {
//            previewInstance?.SetActive(false);
//            return;
//        }

//        // Raycast от контроллера
//        Ray ray = new Ray(rightController.position, rightController.forward);
//        RaycastHit hit;

//        if (Physics.Raycast(ray, out hit, raycastDistance, placementLayers))
//        {
//            // Нашли поверхность
//            // Добавляем смещение вверх над поверхностью
//            targetPosition = hit.point + Vector3.up * arenaHeightOffset;
//            // Используем стандартную ориентацию без поворота
//            targetRotation = Quaternion.identity;

//            // Проверяем валидность размещения
//            isValidPlacement = IsValidPlacement(hit);

//            // Показываем превью
//            previewInstance.SetActive(true);
//            previewInstance.transform.position = targetPosition;
//            previewInstance.transform.rotation = targetRotation;

//            // Обновляем цвет
//            UpdatePreviewColor(isValidPlacement);
//        }
//        else
//        {
//            // Нет попадания
//            previewInstance.SetActive(false);
//            isValidPlacement = false;
//        }
//    }

//    private bool IsValidPlacement(RaycastHit hit)
//    {
//        // Проверяем горизонтальность поверхности
//        float surfaceAngle = Vector3.Angle(hit.normal, Vector3.up);
//        bool isHorizontal = surfaceAngle < 15f; // Допуск 15 градусов

//        // Можно добавить дополнительные проверки:
//        // - Размер поверхности
//        // - Высота от пола
//        // - Наличие препятствий и т.д.

//        return isHorizontal;
//    }

//    private void UpdatePreviewColor(bool isValid)
//    {
//        if (previewInstance == null) return;

//        Color targetColor = isValid ? validColor : invalidColor;

//        foreach (var renderer in previewInstance.GetComponentsInChildren<MeshRenderer>())
//        {
//            foreach (var mat in renderer.materials)
//            {
//                // Try different color properties (different shaders use different names)
//                if (mat.HasProperty("_Color"))
//                {
//                    Color currentColor = mat.color;
//                    currentColor.r = targetColor.r;
//                    currentColor.g = targetColor.g;
//                    currentColor.b = targetColor.b;
//                    mat.color = currentColor;
//                }
//                else if (mat.HasProperty("_BaseColor"))
//                {
//                    Color currentColor = mat.GetColor("_BaseColor");
//                    currentColor.r = targetColor.r;
//                    currentColor.g = targetColor.g;
//                    currentColor.b = targetColor.b;
//                    mat.SetColor("_BaseColor", currentColor);
//                }
//            }
//        }
//    }

//    private void OnPlacePressed(InputAction.CallbackContext context)
//    {
//        if (isArenaPlaced || !isValidPlacement) return;

//        PlaceArena();
//    }

//    private void PlaceArena()
//    {
//        if (arenaPrefab == null)
//        {
//            Debug.LogWarning("[ManualArenaPlacement] Arena prefab is null!");
//            return;
//        }

//        // Проверяем, не создана ли уже арена
//        if (arenaInstance != null)
//        {
//            Debug.LogWarning("[ManualArenaPlacement] Arena already placed!");
//            return;
//        }

//        // Создаем арену сначала
//        arenaInstance = Instantiate(arenaPrefab, targetPosition, targetRotation);
//        arenaInstance.name = "Arena (Manual Placement)";

//        // Создаем AR Anchor на позиции арены
//        if (anchorManager != null && arenaInstance != null)
//        {
//            arenaAnchor = arenaInstance.AddComponent<ARAnchor>();
//            if (arenaAnchor != null)
//            {
//                Debug.Log($"[ManualArenaPlacement] AR Anchor created at {targetPosition}");
//            }
//        }

//        isArenaPlaced = true;

//        // Скрываем превью
//        if (previewInstance != null)
//        {
//            previewInstance.SetActive(false);
//        }

//        Debug.Log($"[ManualArenaPlacement] Arena placed at {targetPosition}");

//        OnArenaPlaced();
//    }

//    private void OnArenaPlaced()
//    {
//        // Уведомляем другие системы о размещении арены
//        ArenaPlacementEvents.InvokeArenaConfirmed();

//        // Здесь можно вызвать события, перестроить NavMesh и т.д.
//        // Аналогично автоматической системе размещения
//    }

//    public void ResetPlacement()
//    {
//        if (arenaInstance != null)
//        {
//            Destroy(arenaInstance);
//            arenaInstance = null;
//        }

//        if (arenaAnchor != null)
//        {
//            Destroy(arenaAnchor);
//            arenaAnchor = null;
//        }

//        isArenaPlaced = false;
//        ArenaPlacementEvents.Reset();

//        if (previewInstance != null)
//        {
//            previewInstance.SetActive(true);
//        }
//    }

//    public bool IsPlaced => isArenaPlaced;

//    public Vector3 GetArenaPosition()
//    {
//        return arenaInstance != null ? arenaInstance.transform.position : Vector3.zero;
//    }

//    private void OnDrawGizmos()
//    {
//        if (!Application.isPlaying || rightController == null) return;

//        // Визуализация луча размещения
//        Gizmos.color = isValidPlacement ? Color.green : Color.red;
//        Gizmos.DrawRay(rightController.position, rightController.forward * raycastDistance);

//        if (isValidPlacement)
//        {
//            Gizmos.DrawSphere(targetPosition, 0.05f);
//        }
//    }
//}
