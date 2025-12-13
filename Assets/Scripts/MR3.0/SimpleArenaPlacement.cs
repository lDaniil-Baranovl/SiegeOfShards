using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Упрощенная система размещения арены для MR
/// Показывает превью-объект перед игроком, который можно перетащить контроллером
/// </summary>
public class SimpleArenaPlacement : MonoBehaviour
{
    [Header("Arena Settings")]
    [SerializeField] private GameObject arenaPrefab; // Префаб настоящей арены
    [SerializeField] private GameObject previewPrefab; // Префаб превью (можно использовать тот же что и arenaPrefab)
    [SerializeField] private float distanceFromPlayer = 1.5f; // Расстояние перед игроком при спавне
    [SerializeField] private float heightOffset = 0.0f; // Высота относительно игрока

    [Header("Visual Feedback")]
    [SerializeField] private Color previewColor = new Color(0, 1, 0, 0.5f); // Зеленый полупрозрачный
    [SerializeField] private Material previewMaterial; // Опциональный материал для превью

    [Header("XR Input Actions")]
    [SerializeField] private InputActionReference dragAction; // XRI Right Interaction/Select (триггер)
    [SerializeField] private InputActionReference placeAction; // XRI Right Interaction/Activate (кнопка A)
    [SerializeField] private Transform rightController; // Трансформ правого контроллера

    // Runtime state
    private GameObject previewInstance;
    private GameObject arenaInstance;
    private bool isArenaPlaced = false;
    private bool isDragging = false;
    private Transform cameraTransform;

    void Start()
    {
        // Получаем камеру (голову игрока)
        cameraTransform = Camera.main.transform;

        // Создаем превью перед игроком
        SpawnPreview();
    }

    void OnEnable()
    {
        // Подписываемся на события Input Actions
        if (placeAction != null && placeAction.action != null)
        {
            placeAction.action.Enable();
            placeAction.action.performed += OnPlaceActionPerformed;
        }

        if (dragAction != null && dragAction.action != null)
        {
            dragAction.action.Enable();
        }
    }

    void OnDisable()
    {
        // Отписываемся от событий
        if (placeAction != null && placeAction.action != null)
        {
            placeAction.action.performed -= OnPlaceActionPerformed;
            placeAction.action.Disable();
        }

        if (dragAction != null && dragAction.action != null)
        {
            dragAction.action.Disable();
        }
    }

    void SpawnPreview()
    {
        if (previewPrefab == null && arenaPrefab == null)
        {
            Debug.LogError("[SimpleArenaPlacement] No prefab assigned!");
            return;
        }

        // Используем previewPrefab если есть, иначе arenaPrefab
        GameObject prefabToUse = previewPrefab != null ? previewPrefab : arenaPrefab;

        // Вычисляем позицию перед игроком
        Vector3 spawnPosition = cameraTransform.position + cameraTransform.forward * distanceFromPlayer;
        spawnPosition.y = cameraTransform.position.y + heightOffset;

        // Создаем превью
        previewInstance = Instantiate(prefabToUse, spawnPosition, Quaternion.identity);
        previewInstance.name = "Arena Preview";

        // Настраиваем визуал превью
        SetupPreviewVisuals();

        Debug.Log($"[SimpleArenaPlacement] Preview spawned at {spawnPosition}");
    }

    void SetupPreviewVisuals()
    {
        if (previewInstance == null) return;

        // Применяем материал или цвет к превью
        var renderers = previewInstance.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (previewMaterial != null)
            {
                // Используем заданный материал
                renderer.material = previewMaterial;
            }
            else
            {
                // Делаем полупрозрачным с зеленым цветом
                foreach (var mat in renderer.materials)
                {
                    // Пытаемся настроить прозрачность для URP
                    if (mat.HasProperty("_Surface"))
                    {
                        mat.SetFloat("_Surface", 1); // Transparent
                    }
                    if (mat.HasProperty("_Blend"))
                    {
                        mat.SetFloat("_Blend", 0); // Alpha
                    }

                    // Устанавливаем цвет
                    if (mat.HasProperty("_BaseColor"))
                    {
                        mat.SetColor("_BaseColor", previewColor);
                    }
                    else if (mat.HasProperty("_Color"))
                    {
                        mat.color = previewColor;
                    }

                    // Включаем прозрачность
                    mat.renderQueue = 3000;
                }
            }
        }

        // Отключаем коллайдеры у превью (чтобы не мешали)
        var colliders = previewInstance.GetComponentsInChildren<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
    }

    void Update()
    {
        if (isArenaPlaced) return;

        HandleInput();
    }

    void HandleInput()
    {
        // Проверяем нажатие триггера для перетаскивания
        if (dragAction != null && dragAction.action != null)
        {
            float dragValue = dragAction.action.ReadValue<float>();

            if (dragValue > 0.1f) // Триггер нажат
            {
                if (!isDragging)
                {
                    isDragging = true;
                    Debug.Log("[SimpleArenaPlacement] Started dragging");
                }

                // Перетаскиваем превью вслед за контроллером
                DragPreview();
            }
            else
            {
                if (isDragging)
                {
                    Debug.Log("[SimpleArenaPlacement] Stopped dragging");
                }
                isDragging = false;
            }
        }
    }

    void OnPlaceActionPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("[SimpleArenaPlacement] Place action performed!");
        ConfirmPlacement();
    }

    void DragPreview()
    {
        if (previewInstance == null || rightController == null) return;

        // Получаем позицию и направление контроллера
        Vector3 controllerPosition = rightController.position;
        Vector3 controllerForward = rightController.forward;

        // Размещаем превью на расстоянии от контроллера
        previewInstance.transform.position = controllerPosition + controllerForward * 0.5f;

        // Опционально: поворачиваем превью к игроку
        if (cameraTransform != null)
        {
            Vector3 lookDirection = cameraTransform.position - previewInstance.transform.position;
            lookDirection.y = 0; // Убираем наклон по Y
            if (lookDirection.magnitude > 0.01f)
            {
                previewInstance.transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    void ConfirmPlacement()
    {
        if (previewInstance == null || arenaPrefab == null)
        {
            Debug.LogWarning("[SimpleArenaPlacement] Cannot confirm placement!");
            return;
        }

        if (isArenaPlaced)
        {
            Debug.LogWarning("[SimpleArenaPlacement] Arena already placed!");
            return;
        }

        // Запоминаем позицию и поворот превью
        Vector3 finalPosition = previewInstance.transform.position;
        Quaternion finalRotation = previewInstance.transform.rotation;

        // Удаляем превью
        Destroy(previewInstance);
        previewInstance = null;

        // Создаем настоящую арену
        arenaInstance = Instantiate(arenaPrefab, finalPosition, finalRotation);
        arenaInstance.name = "Arena (Placed)";

        isArenaPlaced = true;

        Debug.Log($"[SimpleArenaPlacement] Arena placed at {finalPosition}");

        // Можно добавить создание Spatial Anchor
        CreateSpatialAnchor();
    }

    void CreateSpatialAnchor()
    {
        if (arenaInstance == null) return;

        // Создаем OVR Spatial Anchor для сохранения позиции
        var spatialAnchor = arenaInstance.AddComponent<OVRSpatialAnchor>();

        spatialAnchor.Save((anchor, success) =>
        {
            if (success)
            {
                PlayerPrefs.SetString("ArenaAnchorUUID", anchor.Uuid.ToString());
                Debug.Log("[SimpleArenaPlacement] Spatial Anchor saved!");
            }
            else
            {
                Debug.LogWarning("[SimpleArenaPlacement] Failed to save Spatial Anchor!");
            }
        });
    }

    /// <summary>
    /// Сбросить размещение (для тестирования)
    /// </summary>
    public void ResetPlacement()
    {
        if (arenaInstance != null)
        {
            Destroy(arenaInstance);
            arenaInstance = null;
        }

        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }

        isArenaPlaced = false;
        isDragging = false;

        SpawnPreview();
    }

    /// <summary>
    /// Проверка, размещена ли арена
    /// </summary>
    public bool IsPlaced => isArenaPlaced;

    /// <summary>
    /// Получить позицию арены
    /// </summary>
    public Vector3 GetArenaPosition()
    {
        if (arenaInstance != null)
            return arenaInstance.transform.position;

        if (previewInstance != null)
            return previewInstance.transform.position;

        return Vector3.zero;
    }

    void OnDestroy()
    {
        if (previewInstance != null)
        {
            Destroy(previewInstance);
        }
    }
}
