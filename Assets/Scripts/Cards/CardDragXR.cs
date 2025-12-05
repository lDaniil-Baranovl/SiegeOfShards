/// <summary>
/// public InputActionProperty LeftGrip;
/// в Awake: LeftGrip.action.Enable();
/// LeftGrip.action.ReadValue<float>
/// <XRController>{RightHand}/{Grip}
/// </summary>
/// 
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CardDragXR : MonoBehaviour
{
    [Header("XR")]
    public Transform rightController;
    public InputActionProperty gripAction;

    [Header("Battlefield")]
    public LayerMask battlefieldMask;
    public GameObject summonCirclePrefab;

    private GameObject summonCircleInstance;
    private bool isHeld = false;

    public UnitCost data;
    public Transform homeSlot;

    private bool isReturning = false;
    private Coroutine returnRoutine;

    private bool isHovered = false;
    public static CardDragXR currentHeldCard = null;

    private XRGrabInteractable grab;

    void Awake()
    {
        gripAction.action.Enable();
        grab = GetComponent<XRGrabInteractable>();
        if (grab != null)
        {
            // Подписываемся, чтобы реагировать также на select (если XR toolkit захватывает объект напрямую)
            grab.selectEntered.AddListener(OnSelectEntered);
            grab.selectExited.AddListener(OnSelectExited);
        }
    }

    void Start()
    {
        rightController = XRPlayer.Instance.rightController;
    }

    // Если XR toolkit захватил объект — помечаем как взятый
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // Если уже держим другую карту — сбрасываем (не даём второму контроллеру взять)
        if (currentHeldCard != null && currentHeldCard != this)
            return;

        StartHolding();
    }

    // Если XR toolkit отпустил — вызываем нашу логику релиза
    private void OnSelectExited(SelectExitEventArgs args)
    {
        Release();
    }

    public void Init(UnitCost cardData)
    {
        data = cardData;
    }

    void Update()
    {
        float grip = gripAction.action.ReadValue<float>();

        // Если уже держим другую карту — выходим
        if (currentHeldCard != null && currentHeldCard != this)
            return;

        // Взяли через grip + hover (на случай, если toolkit не сработал)
        if (!isHeld && isHovered && grip > 0.7f)
        {
            if (!IsActuallyUnderControllerRay()) return;
            if (grab != null) grab.enabled = false;
            StartHolding();
        }
        // Отпустили через grip
        else if (isHeld && grip < 0.2f)
        {
            Release();
            // снова включаем grab (оно будет готово для следующего спавна)
            if (grab != null) grab.enabled = true;
        }

        if (isHeld)
            UpdateSummonCircle();
    }
    public void OnHoverEntered()
    {
        if (currentHeldCard != null) return;
        isHovered = true;
    }

    public void OnHoverExited()
    {
        if (currentHeldCard != null) return;
        isHovered = false;
    }

    private void StartHolding()
    {
        if (currentHeldCard != null && currentHeldCard != this)
            return;

        currentHeldCard = this;
        isHeld = true;

        // Привяжем картe позицию под контроллер (иногда полезно сделать parent)
        transform.SetParent(rightController, true); 

        Debug.Log("Карта взята XR: " + name);
    }

    private void Release()
    {
        if (currentHeldCard != this)
            return;

        transform.SetParent(null, true);

        isHeld = false;
        currentHeldCard = null;

        if (summonCircleInstance != null)
            summonCircleInstance.SetActive(false);

        bool used = false;

        if (Physics.Raycast(rightController.position, Vector3.down, out RaycastHit hit, 5f, battlefieldMask))
        {
            if (ElixirManager.Instance.TrySpend(data.elixirCost))
            {
                used = true;
                SpawnUnit(hit.point);
                FindObjectOfType<CardCycleManager>().OnCardUsed(this);
            }
        }

        if (!used)
        {
            // Прежде чем анимировать возврат — убедимся, что XRGrab не мешает.
            if (grab != null)
            {
                // временно отключаем компонент — это заставит interactor отпустить объект
                grab.enabled = false;
            }

            ReturnToHome();

            // В конце ReturnAnimation мы снова включим grab (см. ReturnAnimation)
        }
    }

    public void ReturnToHome()
    {
        if (isReturning)
            return;

        if (homeSlot == null)
        {
            Debug.LogError("homeSlot не назначен!");
            return;
        }

        isReturning = true;

        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        // Гарантированно отвязываем до анимации
        transform.SetParent(null);

        returnRoutine = StartCoroutine(ReturnAnimation());
    }

    private IEnumerator ReturnAnimation()
    {
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 endPos = homeSlot.position;
        Quaternion endRot = homeSlot.rotation;

        float duration = 0.25f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float smooth = Mathf.SmoothStep(0, 1, t);

            transform.position = Vector3.Lerp(startPos, endPos, smooth);
            transform.Rotate(0, 900f * Time.deltaTime, 0, Space.Self);

            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;

        // Делаем карту дочерним объектом слота (чтобы следующее чтение позиции было корректным)
        transform.SetParent(homeSlot, true);

        // Возврат завершён — снова можно включить grab
        if (grab != null)
            grab.enabled = true;

        isReturning = false;
        returnRoutine = null;
    }

    private void UpdateSummonCircle()
    {
        if (!isHeld) return;

        Debug.DrawRay(rightController.position, Vector3.down * 5f, Color.green);

        if (Physics.Raycast(rightController.position, Vector3.down,
            out RaycastHit hit, 5f, battlefieldMask))
        {
            if (summonCircleInstance == null)
                summonCircleInstance = Instantiate(summonCirclePrefab);

            summonCircleInstance.SetActive(true);
            summonCircleInstance.transform.position = hit.point + Vector3.up * 0.05f;
        }
        else
        {
            if (summonCircleInstance != null)
                summonCircleInstance.SetActive(false);
        }
    }

    private void SpawnUnit(Vector3 pos)
    {
        for (int i = 0; i < data.prefabs.Length; i++)
        {
            Vector3 spawnPos = pos;

            if (data.spawnOffsets != null && i < data.spawnOffsets.Length)
                spawnPos += data.spawnOffsets[i];

            Instantiate(data.prefabs[i], spawnPos, Quaternion.identity);
        }
    }

    private bool IsActuallyUnderControllerRay()
    {
        if (rightController == null) return false;

        if (Physics.Raycast(
                rightController.position,
                rightController.forward,
                out RaycastHit hit,
                10f))
        {
            CardDragXR card = hit.collider.GetComponentInParent<CardDragXR>();
            return card == this;
        }

        return false;
    }

}


