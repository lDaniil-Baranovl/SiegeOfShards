/// <summary>
/// public InputActionProperty LeftGrip;
/// Awake: LeftGrip.action.Enable();
/// LeftGrip.action.ReadValue<float>
/// <XRController>{RightHand}/{Grip}
/// </summary>
/// 
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

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
            grab.selectEntered.AddListener(OnSelectEntered);
            grab.selectExited.AddListener(OnSelectExited);
        }
    }

    void Start()
    {
        rightController = XRPlayer.Instance.rightController;

        if (grab != null)
            grab.selectFilters.Add(new XRSelectFilterDelegate((interactor, interactable) => IsRightControllerInteractor(interactor)));
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!IsRightControllerInteractor(args.interactorObject))
            return;

        if (currentHeldCard != null && currentHeldCard != this)
            return;

        StartHolding();
    }
    private void OnSelectExited(SelectExitEventArgs args)
    {
        if (!IsRightControllerInteractor(args.interactorObject))
            return;

        Release();
    }

    private bool IsRightControllerInteractor(IXRInteractor interactor)
    {
        return rightController != null && interactor != null && interactor.transform.IsChildOf(rightController);
    }

    public void Init(UnitCost cardData)
    {
        data = cardData;
    }

    void Update()
    {
        float grip = gripAction.action.ReadValue<float>();

        if (currentHeldCard != null && currentHeldCard != this)
            return;

        if (!isHeld && isHovered && grip > 0.7f)
        {
            if (!IsActuallyUnderControllerRay()) return;
            if (grab != null) grab.enabled = false;
            StartHolding();
        }
        else if (isHeld && grip < 0.2f)
        {
            Release();
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

        transform.SetParent(rightController, true); 

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
            if (grab != null)
            {
                grab.enabled = false;
            }

            ReturnToHome();

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

        transform.SetParent(null);

        returnRoutine = StartCoroutine(ReturnAnimation());
    }

    private IEnumerator ReturnAnimation()
    {
        Vector3 startPos = transform.position;

        float duration = 0.25f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float smooth = Mathf.SmoothStep(0, 1, t);

            // homeSlot закреплён на руке игрока и постоянно двигается,
            // поэтому цель берём "вживую" каждый кадр, а не один раз.
            transform.position = Vector3.Lerp(startPos, homeSlot.position, smooth);
            transform.Rotate(0, 900f * Time.deltaTime, 0, Space.Self);

            yield return null;
        }

        // Снэп делаем по актуальным координатам слота, и только потом
        // репарентим — в этот момент относительный поворот нулевой,
        // поэтому SetParent не ломает localScale (без этого был stretch).
        transform.position = homeSlot.position;
        transform.rotation = homeSlot.rotation;

        transform.SetParent(homeSlot, true);

        if (grab != null)
            grab.enabled = true;

        isReturning = false;
        returnRoutine = null;
    }

    private void UpdateSummonCircle()
    {
        if (!isHeld) return;

        // raycast (указка)
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


