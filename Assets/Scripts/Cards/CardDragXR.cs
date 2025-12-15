/// <summary>
/// public InputActionProperty LeftGrip;
/// � Awake: LeftGrip.action.Enable();
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
    [Tooltip("Максимальная дистанция raycast для размещения карт")]
    public float maxRaycastDistance = 25f;

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
            // �������������, ����� ����������� ����� �� select (���� XR toolkit ����������� ������ ��������)
            grab.selectEntered.AddListener(OnSelectEntered);
            grab.selectExited.AddListener(OnSelectExited);
        }
    }

    void Start()
    {
        rightController = XRPlayer.Instance.rightController;
    }

    // ���� XR toolkit �������� ������ � �������� ��� ������
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        // ���� ��� ������ ������ ����� � ���������� (�� ��� ������� ����������� �����)
        if (currentHeldCard != null && currentHeldCard != this)
            return;

        StartHolding();
    }

    // ���� XR toolkit �������� � �������� ���� ������ ������
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

        // ���� ��� ������ ������ ����� � �������
        if (currentHeldCard != null && currentHeldCard != this)
            return;

        // ����� ����� grip + hover (�� ������, ���� toolkit �� ��������)
        if (!isHeld && isHovered && grip > 0.7f)
        {
            if (!IsActuallyUnderControllerRay()) return;
            if (grab != null) grab.enabled = false;
            StartHolding();
        }
        // ��������� ����� grip
        else if (isHeld && grip < 0.2f)
        {
            Release();
            // ����� �������� grab (��� ����� ������ ��� ���������� ������)
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

        // �������� ����e ������� ��� ���������� (������ ������� ������� parent)
        transform.SetParent(rightController, true); 

        Debug.Log("����� ����� XR: " + name);
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

        // Используем raycast в направлении контроллера (куда указывает)
        if (Physics.Raycast(rightController.position, rightController.forward, out RaycastHit hit, maxRaycastDistance, battlefieldMask))
        {
            if (ElixirManager.Instance.TrySpend(data.elixirCost))
            {
                used = true;
                SpawnUnit(hit.point);
                if (summonCircleInstance != null)
                {
                    Destroy(summonCircleInstance);
                    summonCircleInstance = null;
                }
                FindObjectOfType<CardCycleManager>().OnCardUsed(this);
            }
        }

        if (!used)
        {
            // ������ ��� ����������� ������� � ��������, ��� XRGrab �� ������.
            if (grab != null)
            {
                // �������� ��������� ��������� � ��� �������� interactor ��������� ������
                grab.enabled = false;
            }

            ReturnToHome();

            // � ����� ReturnAnimation �� ����� ������� grab (��. ReturnAnimation)
        }
    }

    public void ReturnToHome()
    {
        if (isReturning)
            return;

        if (summonCircleInstance != null)
        {
            Destroy(summonCircleInstance);
            summonCircleInstance = null;
        }

        if (homeSlot == null)
        {
            Debug.LogError("homeSlot �� ��������!");
            return;
        }

        isReturning = true;

        if (returnRoutine != null)
            StopCoroutine(returnRoutine);

        // �������������� ���������� �� ��������
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

        // ������ ����� �������� �������� ����� (����� ��������� ������ ������� ���� ����������)
        transform.SetParent(homeSlot, true);

        // ������� �������� � ����� ����� �������� grab
        if (grab != null)
            grab.enabled = true;

        isReturning = false;
        returnRoutine = null;
    }

    private void UpdateSummonCircle()
    {
        if (!isHeld) return;

        // Используем raycast в направлении, куда указывает контроллер (как указка)
        Debug.DrawRay(rightController.position, rightController.forward * maxRaycastDistance, Color.green);

        if (Physics.Raycast(rightController.position, rightController.forward,
            out RaycastHit hit, maxRaycastDistance, battlefieldMask))
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
                maxRaycastDistance))
        {
            CardDragXR card = hit.collider.GetComponentInParent<CardDragXR>();
            return card == this;
        }

        return false;
    }

}


