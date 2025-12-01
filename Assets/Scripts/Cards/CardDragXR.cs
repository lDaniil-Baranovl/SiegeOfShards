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

public class CardDragXR : MonoBehaviour
{
    [Header("XR")]
    public Transform rightController;       // Контроллер, откуда бросаем луч
    public InputActionProperty gripAction;  // Значение грипа

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

    void Start()
    {
        rightController = XRPlayer.Instance.rightController;
    }

    void Awake()
    {
        gripAction.action.Enable();
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

        // Взяли
        if (!isHeld && isHovered && grip > 0.7f)
        {
            StartHolding();
        }
        // Отпустили
        else if (isHeld && grip < 0.2f)
        {
            Release();
        }

        // Пока держим карточку — обновляем круг
        if (isHeld)
            UpdateSummonCircle();
    }
    public void OnHoverEntered()
    {
        if (currentHeldCard != null) return; // если уже держим карту — игнорировать hover
        isHovered = true;
    }

    public void OnHoverExited()
    {
        if (currentHeldCard != null) return;
        isHovered = false;
    }

    private void StartHolding()
    {
        // Если уже держим другую карту — эту игнорируем
        if (currentHeldCard != null && currentHeldCard != this)
            return;

        currentHeldCard = this;
        isHeld = true;

        Debug.Log("Карта взята XR: " + name);
    }


    private void Release()
    {
        if (currentHeldCard != this)
            return; // не наша карта – не отпускать

        isHeld = false;
        currentHeldCard = null;

        if (summonCircleInstance != null)
            summonCircleInstance.SetActive(false);

        bool used = false;

        // Проверка поля
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
            ReturnToHome();
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

        // ВАЖНО! Перед возвратом — отвязываем от контроллера
        transform.SetParent(null);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float smooth = Mathf.SmoothStep(0, 1, t);

            // ДВИГАЕТ ТОЛЬКО ЭТУ КОНКРЕТНУЮ КАРТУ
            transform.position = Vector3.Lerp(startPos, endPos, smooth);

            // КРУТИТ ТОЛЬКО ЭТУ КОНКРЕТНУЮ КАРТУ
            transform.Rotate(0, 900f * Time.deltaTime, 0, Space.Self);

            yield return null;
        }

        transform.position = endPos;
        transform.rotation = endRot;

        // Делает карту дочерним объектом слота
        transform.SetParent(homeSlot.parent);

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

}

