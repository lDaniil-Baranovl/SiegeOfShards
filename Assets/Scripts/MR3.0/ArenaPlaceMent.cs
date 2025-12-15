using UnityEngine;
using Meta.XR.MRUtilityKit;

public class ClashRoyaleArenaPlacement : MonoBehaviour
{
    [SerializeField] private GameObject arenaPrefab;
    [SerializeField] private float minTableSize = 0.8f; // ������� 80�� x 80��
    [SerializeField] private Material previewMaterial;
    [SerializeField] private float arenaHeightOffset = 0.05f; // ��������, ����� ����� ���� ���� ����� �����������

    private GameObject arenaPreview;
    private MRUKAnchor selectedSurface;
    private bool isPlaced = false;
    private bool autoPlacementEnabled = false; // ���� ��� ���������� ��������������� ����������

    void Start()
    {
        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
    }

    void OnSceneLoaded()
    {
        // �������������� ���������� ��������� ������ ���� ������� ����
        if (!autoPlacementEnabled)
        {
            Debug.Log("�������������� ���������� ��������� - ����������� ������ ����������");
            return;
        }

        // ����� ������ ����/�����������
        var bestSurface = FindBestPlaySurface();

        if (bestSurface != null)
        {
            ShowArenaPreview(bestSurface);
        }
        else
        {
            // Fallback - ������ ����������
            EnableManualPlacement();
        }
    }

    MRUKAnchor FindBestPlaySurface()
    {
        var room = MRUK.Instance.GetCurrentRoom();
        MRUKAnchor bestTable = null;
        float bestScore = 0;

        // ���� ����� � ������ �������������� �����������
        foreach (var anchor in room.Anchors)
        {
            // ��������� ��� ��� ����, ������ ������ ��� ���
            if (anchor.Label == MRUKAnchor.SceneLabels.TABLE ||
                anchor.Label == MRUKAnchor.SceneLabels.COUCH ||
                anchor.Label == MRUKAnchor.SceneLabels.OTHER)
            {
                var bounds = anchor.VolumeBounds.Value.size;

                // ��������� ������ (����� ����������/������������� �������)
                if (bounds.x >= minTableSize && bounds.z >= minTableSize)
                {
                    // ������: ������������ ����� �� ������ �����
                    float heightScore = 1f - Mathf.Abs(anchor.transform.position.y - 0.8f);
                    float sizeScore = Mathf.Min(bounds.x, bounds.z);
                    float score = heightScore + sizeScore;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTable = anchor;
                    }
                }
            }
        }

        return bestTable;
    }

    void ShowArenaPreview(MRUKAnchor surface)
    {
        selectedSurface = surface;

        // ������� ������ �����
        arenaPreview = Instantiate(arenaPrefab);

        // ������������� ������� � ������ �������� �����
        Vector3 surfacePosition = surface.transform.position;
        surfacePosition.y += arenaHeightOffset; // ��������� �������� �����

        arenaPreview.transform.position = surfacePosition;
        arenaPreview.transform.rotation = Quaternion.identity;

        // ���������� feedback (��������������)
        SetPreviewMode(arenaPreview, true);

        Debug.Log($"������� �����������: {surface.Label}, ������: {surface.VolumeBounds.Value.size}");
    }

    void Update()
    {
        if (!isPlaced && arenaPreview != null)
        {
            // A - ����������� ����������
            if (OVRInput.GetDown(OVRInput.Button.One))
            {
                ConfirmPlacement();
            }

            // B - ������ �����������
            if (OVRInput.GetDown(OVRInput.Button.Two))
            {
                EnableManualPlacement();
            }

            // Thumbstick - ������� �����
            float rotation = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick).x;
            arenaPreview.transform.Rotate(Vector3.up, rotation * 90f * Time.deltaTime);
        }
    }

    void ConfirmPlacement()
    {
        isPlaced = true;
        SetPreviewMode(arenaPreview, false);

        // Уведомляем другие системы о размещении арены
        ArenaPlacementEvents.InvokeArenaConfirmed();

        // ������� Spatial Anchor ��� ���������� �������
        CreateSpatialAnchor(arenaPreview.transform);

        Debug.Log("����� ���������!");
    }

    void SetPreviewMode(GameObject obj, bool isPreview)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        foreach (var rend in renderers)
        {
            if (isPreview)
            {
                rend.material = previewMaterial; // �������������� ��������
            }
            else
            {
                // ������� ������������ ���������
                rend.material.color = Color.white;
            }
        }
    }

    void EnableManualPlacement()
    {
        if (arenaPreview == null)
        {
            arenaPreview = Instantiate(arenaPrefab);
        }

        StartCoroutine(ManualPlacementCoroutine());
    }

    System.Collections.IEnumerator ManualPlacementCoroutine()
    {
        while (!isPlaced)
        {
            // Raycast �� ������� �����������
            if (XRPlayer.Instance != null && XRPlayer.Instance.rightController != null)
            {
                Transform controller = XRPlayer.Instance.rightController;
                Ray ray = new Ray(controller.position, controller.forward);

                if (Physics.Raycast(ray, out RaycastHit hit, 10f, LayerMask.GetMask("MRSceneMesh")))
                {
                    // ��������� ��� ����������� ��������������
                    if (Vector3.Dot(hit.normal, Vector3.up) > 0.8f)
                    {
                        // ��������� �������� ����� ��� ���������� �����
                        Vector3 position = hit.point;
                        position.y += arenaHeightOffset;

                        arenaPreview.transform.position = position;
                        // ������ ���������� ������� - ��������� ������ �� ���������
                        arenaPreview.transform.rotation = Quaternion.identity;
                    }
                }
            }

            yield return null;
        }
    }

    void CreateSpatialAnchor(Transform anchorTransform)
    {
        var spatialAnchor = anchorTransform.gameObject.AddComponent<OVRSpatialAnchor>();

        spatialAnchor.Save((anchor, success) =>
        {
            if (success)
            {
                // ��������� UUID ��� �������� � ��������� ���
                PlayerPrefs.SetString("ArenaAnchorUUID", anchor.Uuid.ToString());
                Debug.Log("Spatial Anchor ��������!");
            }
        });
    }

    #region Public API for HybridArenaPlacementManager

    /// <summary>
    /// ��������, ��������� �� �����
    /// </summary>
    public bool IsPlaced => isPlaced;

    /// <summary>
    /// ��������, ������� �� ���� ���� ���� ���������� ����
    /// </summary>
    public bool HasFoundTables => selectedSurface != null;

    /// <summary>
    /// �������� ������� ������� �����
    /// </summary>
    public Vector3 GetArenaPosition()
    {
        if (arenaPreview != null)
        {
            return arenaPreview.transform.position;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// �������� ���������� (��� �������)
    /// </summary>
    public void ResetPlacement()
    {
        if (arenaPreview != null)
        {
            Destroy(arenaPreview);
            arenaPreview = null;
        }
        selectedSurface = null;
        isPlaced = false;
        ArenaPlacementEvents.Reset();
    }

    #endregion
}