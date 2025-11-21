using UnityEngine;

public class CardDrag : MonoBehaviour
{
    [Header("Summon Circle")]
    public GameObject summonCirclePrefab;
    private GameObject summonCircleInstance;

    public LayerMask battlefieldMask;
    public UnitCost data;

    private bool isDragging = false;
    private Vector3 startPosition;

    public CanvasGroup canvasGroup;
    void Start()
    {
        startPosition = transform.position;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Update()
    {
        UpdateCardLockState();

        HandleMouseInput();

        if (!isDragging) return;

        FollowCursor();
        UpdateSummonCircle();
    }
    private void UpdateCardLockState()
    {
        int current = ElixirManager.Instance.GetElixir();

        if (current < data.elixirCost)
        {
            canvasGroup.alpha = 0.4f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(r, out RaycastHit hit) && hit.transform == transform)
            {
                if (ElixirManager.Instance.GetElixir() < data.elixirCost)
                    return;
                BeginDrag();
            }
        } 
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }
    }

    private void BeginDrag()
    {
        isDragging = true;
    }

    private void EndDrag()
    {
        isDragging = false;

        if (summonCircleInstance != null)
            summonCircleInstance.SetActive(false);

        if (!Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down,
            out RaycastHit hit, 20f, battlefieldMask))
        {
            ReturnCard();
            return;
        }

        if (!ElixirManager.Instance.TrySpend(data.elixirCost))
        {
            ReturnCard();
            return;
        }

        SpawnUnit(hit.point);
    }

    private void UpdateSummonCircle()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 2f, Vector3.down,
            out RaycastHit hit, 20f, battlefieldMask))
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

    private void FollowCursor()
    {
        Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(r, out RaycastHit hit, 100f))
        {
            transform.position = hit.point + Vector3.up * 0.3f;
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

        FindObjectOfType<CardCycleManager>().OnCardUsed(this);
    }


    private void ReturnCard()
    {
        transform.position = startPosition;
    }
}
