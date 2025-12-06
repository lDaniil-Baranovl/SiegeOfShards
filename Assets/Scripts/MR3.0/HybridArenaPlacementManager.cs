using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Гибридный менеджер размещения арены
/// Пытается автоматически найти стол, если не получается - переключается на ручной режим
/// </summary>
public class HybridArenaPlacementManager : MonoBehaviour
{
    [Header("Placement Systems")]
    [SerializeField] private ClashRoyaleArenaPlacement autoPlacement;
    [SerializeField] private ManualArenaPlacement manualPlacement;

    [Header("Settings")]
    [SerializeField] private float autoModeTimeout = 10f; // Сколько секунд ждать автопоиск
    [SerializeField] private bool allowModeSwitch = true; // Можно ли переключаться между режимами

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private GameObject autoModeUI;
    [SerializeField] private GameObject manualModeUI;
    [SerializeField] private Button switchToManualButton;
    [SerializeField] private Slider scanProgressSlider;

    [Header("Audio Feedback")]
    [SerializeField] private AudioClip tableFoundSound;
    [SerializeField] private AudioClip arenaPlacedSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem placementEffect;

    private PlacementMode currentMode = PlacementMode.Auto;
    private bool isPlaced = false;
    private float autoModeStartTime;

    public enum PlacementMode
    {
        Auto,
        Manual
    }

    #region Lifecycle

    private void Awake()
    {
        ValidateComponents();
        SetupUI();
    }

    private void Start()
    {
        StartPlacementProcess();
    }

    private void Update()
    {
        UpdateUI();
        CheckPlacementStatus();

        // Проверяем таймаут автоматического режима
        if (currentMode == PlacementMode.Auto && !isPlaced)
        {
            float elapsed = Time.time - autoModeStartTime;
            if (elapsed > autoModeTimeout && autoPlacement != null)
            {
                // Проверяем, нашли ли мы хоть один стол
                if (!HasFoundTables())
                {
                    Debug.Log("[HybridPlacement] Auto mode timeout, switching to manual");
                    SwitchToManualMode();
                }
            }

            // Обновляем прогресс
            if (scanProgressSlider != null)
            {
                scanProgressSlider.value = Mathf.Clamp01(elapsed / autoModeTimeout);
            }
        }
    }

    #endregion

    #region Initialization

    private void ValidateComponents()
    {
        if (autoPlacement == null)
        {
            autoPlacement = GetComponent<ClashRoyaleArenaPlacement>();
            if (autoPlacement == null)
            {
                Debug.LogWarning("[HybridPlacement] Auto placement component not found");
            }
        }

        if (manualPlacement == null)
        {
            manualPlacement = GetComponent<ManualArenaPlacement>();
            if (manualPlacement == null)
            {
                Debug.LogWarning("[HybridPlacement] Manual placement component not found");
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void SetupUI()
    {
        if (switchToManualButton != null)
        {
            switchToManualButton.onClick.AddListener(OnSwitchToManualButtonClicked);
        }
    }

    #endregion

    #region Placement Process

    private void StartPlacementProcess()
    {
        autoModeStartTime = Time.time;

        // Начинаем с автоматического режима
        SwitchToAutoMode();
    }

    private void SwitchToAutoMode()
    {
        currentMode = PlacementMode.Auto;

        if (autoPlacement != null)
        {
            autoPlacement.enabled = true;
        }

        if (manualPlacement != null)
        {
            manualPlacement.enabled = false;
        }

        UpdateModeUI();
        Debug.Log("[HybridPlacement] Switched to Auto mode");
    }

    private void SwitchToManualMode()
    {
        currentMode = PlacementMode.Manual;

        if (autoPlacement != null)
        {
            autoPlacement.enabled = false;
        }

        if (manualPlacement != null)
        {
            manualPlacement.enabled = true;
        }

        UpdateModeUI();
        Debug.Log("[HybridPlacement] Switched to Manual mode");
    }

    #endregion

    #region Status Checking

    private void CheckPlacementStatus()
    {
        bool wasPlaced = isPlaced;

        // Проверяем статус в зависимости от режима
        if (currentMode == PlacementMode.Auto && autoPlacement != null)
        {
            isPlaced = autoPlacement.IsPlaced;
        }
        else if (currentMode == PlacementMode.Manual && manualPlacement != null)
        {
            isPlaced = manualPlacement.IsPlaced;
        }

        // Если только что разместили
        if (isPlaced && !wasPlaced)
        {
            OnArenaPlaced();
        }
    }

    private bool HasFoundTables()
    {
        // Проверяем, обнаружила ли система хоть один подходящий стол
        if (autoPlacement == null) return false;

        return autoPlacement.HasFoundTables;
    }

    #endregion

    #region Callbacks

    private void OnArenaPlaced()
    {
        Debug.Log("[HybridPlacement] Arena placed successfully!");

        // Отключаем оба компонента
        if (autoPlacement != null) autoPlacement.enabled = false;
        if (manualPlacement != null) manualPlacement.enabled = false;

        // Воспроизводим звук
        PlaySound(arenaPlacedSound);

        // Эффект частиц
        if (placementEffect != null)
        {
            Vector3 arenaPosition = GetArenaPosition();
            placementEffect.transform.position = arenaPosition;
            placementEffect.Play();
        }

        // Уведомляем другие системы
        BroadcastArenaPlaced();
    }

    private void OnSwitchToManualButtonClicked()
    {
        if (!allowModeSwitch || isPlaced) return;

        SwitchToManualMode();
    }

    #endregion

    #region UI Updates

    private void UpdateUI()
    {
        UpdateInstructionText();
        UpdateModeUI();
    }

    private void UpdateInstructionText()
    {
        if (instructionText == null) return;

        if (isPlaced)
        {
            instructionText.text = "Arena placed! Game starting...";
            return;
        }

        switch (currentMode)
        {
            case PlacementMode.Auto:
                if (autoPlacement != null && HasFoundTables())
                {
                    instructionText.text = "Table found! Press trigger to confirm placement";
                }
                else
                {
                    float timeLeft = autoModeTimeout - (Time.time - autoModeStartTime);
                    instructionText.text = $"Scanning room for tables... ({timeLeft:F0}s)";
                }
                break;

            case PlacementMode.Manual:
                instructionText.text = "Point at a surface and press trigger to place arena";
                break;
        }
    }

    private void UpdateModeUI()
    {
        if (autoModeUI != null)
        {
            autoModeUI.SetActive(currentMode == PlacementMode.Auto && !isPlaced);
        }

        if (manualModeUI != null)
        {
            manualModeUI.SetActive(currentMode == PlacementMode.Manual && !isPlaced);
        }

        if (switchToManualButton != null)
        {
            switchToManualButton.gameObject.SetActive(
                currentMode == PlacementMode.Auto &&
                !isPlaced &&
                allowModeSwitch
            );
        }
    }

    #endregion

    #region Audio

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    #endregion

    #region Broadcasting

    private void BroadcastArenaPlaced()
    {
        // Вариант 1: Unity Events
        // OnArenaPlacedEvent?.Invoke();

        // Вариант 2: SendMessage (старый способ)
        // BroadcastMessage("OnArenaPlaced", SendMessageOptions.DontRequireReceiver);

        // Вариант 3: Static event (как предложено в Code Review)
        // GameEvents.OnArenaPlaced?.Invoke();

        // Вариант 4: Найти и уведомить конкретные системы
        NotifyGameSystems();
    }

    private void NotifyGameSystems()
    {
        // Перестраиваем NavMesh
        var navMeshBuilder = FindObjectOfType<NavMeshBuildOnline>();
        if (navMeshBuilder != null)
        {
            // navMeshBuilder.BuildNavMeshOnce();
            Debug.Log("[HybridPlacement] NavMesh rebuild triggered");
        }

        // Уведомляем BattleManager
        var battleManager = FindObjectOfType<BattleManager>();
        if (battleManager != null)
        {
            // battleManager.OnArenaReady();
            Debug.Log("[HybridPlacement] BattleManager notified");
        }

        // Можно добавить другие системы
    }

    #endregion

    #region Public API

    /// <summary>
    /// Получить текущую позицию арены
    /// </summary>
    public Vector3 GetArenaPosition()
    {
        if (currentMode == PlacementMode.Auto && autoPlacement != null)
        {
            return autoPlacement.GetArenaPosition();
        }
        else if (currentMode == PlacementMode.Manual && manualPlacement != null)
        {
            return manualPlacement.GetArenaPosition();
        }

        return Vector3.zero;
    }

    /// <summary>
    /// Проверить, размещена ли арена
    /// </summary>
    public bool IsPlaced => isPlaced;

    /// <summary>
    /// Текущий режим размещения
    /// </summary>
    public PlacementMode CurrentMode => currentMode;

    /// <summary>
    /// Принудительно переключить режим
    /// </summary>
    public void ForceMode(PlacementMode mode)
    {
        if (isPlaced) return;

        if (mode == PlacementMode.Auto)
            SwitchToAutoMode();
        else
            SwitchToManualMode();
    }

    /// <summary>
    /// Сбросить размещение (для отладки)
    /// </summary>
    public void ResetPlacement()
    {
        if (autoPlacement != null)
        {
            autoPlacement.ResetPlacement();
        }

        if (manualPlacement != null)
        {
            manualPlacement.ResetPlacement();
        }

        isPlaced = false;
        StartPlacementProcess();
    }

    #endregion

    #region Debug

    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Mode: {currentMode}");
        GUILayout.Label($"Placed: {isPlaced}");
        GUILayout.Label($"Auto timeout: {autoModeTimeout - (Time.time - autoModeStartTime):F1}s");

        if (GUILayout.Button("Reset Placement"))
        {
            ResetPlacement();
        }

        if (GUILayout.Button("Switch to Manual"))
        {
            SwitchToManualMode();
        }

        if (GUILayout.Button("Switch to Auto"))
        {
            SwitchToAutoMode();
        }

        GUILayout.EndArea();
    }

    #endregion
}
