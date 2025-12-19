using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleArenaPlacement : MonoBehaviour
{
    [Header("Arena Settings")]
    [SerializeField] private GameObject arenaPrefab; 
    [SerializeField] private GameObject previewPrefab; 
    [SerializeField] private float distanceFromPlayer = 1.5f; 
    [SerializeField] private float heightOffset = 0.0f; 

    [Header("Visual Feedback")]
    [SerializeField] private Color previewColor = new Color(0, 1, 0, 0.5f);
    [SerializeField] private Material previewMaterial; 

    [Header("XR Input Actions")]
    [SerializeField] private InputActionReference dragAction; 
    [SerializeField] private InputActionReference placeAction; 
    [SerializeField] private Transform rightController;

    private GameObject previewInstance;
    private GameObject arenaInstance;
    private bool isArenaPlaced = false;
    private bool isDragging = false;
    private Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        SpawnPreview();
    }

    void OnEnable()
    {
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

        GameObject prefabToUse = previewPrefab != null ? previewPrefab : arenaPrefab;

        Vector3 spawnPosition = cameraTransform.position + cameraTransform.forward * distanceFromPlayer;
        spawnPosition.y = cameraTransform.position.y + heightOffset;

        previewInstance = Instantiate(prefabToUse, spawnPosition, Quaternion.identity);
        previewInstance.name = "Arena Preview";

        SetupPreviewVisuals();

        Debug.Log($"[SimpleArenaPlacement] Preview spawned at {spawnPosition}");
    }

    void SetupPreviewVisuals()
    {
        if (previewInstance == null) return;

        var renderers = previewInstance.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (previewMaterial != null)
            {
                Material[] materials = new Material[renderer.sharedMaterials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = previewMaterial;
                }
                renderer.sharedMaterials = materials;
            }
            else
            {
                Material[] originalMaterials = renderer.sharedMaterials;
                Material[] newMaterials = new Material[originalMaterials.Length];

                for (int i = 0; i < originalMaterials.Length; i++)
                {
                    if (originalMaterials[i] != null)
                    {
                        newMaterials[i] = new Material(originalMaterials[i]);

                        if (newMaterials[i].HasProperty("_Surface"))
                        {
                            newMaterials[i].SetFloat("_Surface", 1);
                        }
                        if (newMaterials[i].HasProperty("_Blend"))
                        {
                            newMaterials[i].SetFloat("_Blend", 0);
                        }

                        if (newMaterials[i].HasProperty("_BaseColor"))
                        {
                            newMaterials[i].SetColor("_BaseColor", previewColor);
                        }
                        else if (newMaterials[i].HasProperty("_Color"))
                        {
                            newMaterials[i].SetColor("_Color", previewColor);
                        }
                        newMaterials[i].renderQueue = 3000;
                    }
                }

                renderer.sharedMaterials = newMaterials;
            }
        }

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
        if (dragAction != null && dragAction.action != null)
        {
            float dragValue = dragAction.action.ReadValue<float>();

            if (dragValue > 0.1f)
            {
                if (!isDragging)
                {
                    isDragging = true;
                    Debug.Log("[SimpleArenaPlacement] Started dragging");
                }

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

        Vector3 controllerPosition = rightController.position;
        Vector3 controllerForward = rightController.forward;

        previewInstance.transform.position = controllerPosition + controllerForward * 0.5f;

        if (cameraTransform != null)
        {
            Vector3 lookDirection = cameraTransform.position - previewInstance.transform.position;
            lookDirection.y = 0; 
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

        Vector3 finalPosition = previewInstance.transform.position;
        Quaternion finalRotation = previewInstance.transform.rotation;

        Destroy(previewInstance);
        previewInstance = null;

        arenaInstance = Instantiate(arenaPrefab, finalPosition, finalRotation);
        arenaInstance.name = "Arena (Placed)";


        foreach (Transform child in arenaInstance.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "Terrain" || child.name.Contains("Water") || child.name.Contains("water"))
            {
                child.gameObject.SetActive(true);
                Debug.Log($"[SimpleArenaPlacement] Activated {child.name}");
            }
        }

        isArenaPlaced = true;

        Debug.Log($"[SimpleArenaPlacement] Arena placed at {finalPosition}");

        ArenaPlacementEvents.InvokeArenaConfirmed();


        CreateSpatialAnchor();
    }

    void CreateSpatialAnchor()
    {
        if (arenaInstance == null) return;

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

        // Сбрасываем глобальное состояние
        ArenaPlacementEvents.Reset();

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
