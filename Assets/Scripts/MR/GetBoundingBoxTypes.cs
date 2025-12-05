using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class GetBoundingBoxTypes : MonoBehaviour
{
    [SerializeField] ARBoundingBoxManager _aRBoundingBoxManager;
    public void DoDirtyThings()
    {
        foreach (ARBoundingBox bBox in _aRBoundingBoxManager.trackables)
        {
            Debug.Log(bBox.classifications);
        }
    }
    private void Awake()
    {
        _aRBoundingBoxManager.trackablesChanged.AddListener(ColorizeBed);
    }
    private void ColorizeBed(ARTrackablesChangedEventArgs<ARBoundingBox> trackablesChangedEventArgs)
    {
        foreach (var trackable in trackablesChangedEventArgs.added)
        {
            if (trackable.classifications == UnityEngine.XR.ARSubsystems.BoundingBoxClassifications.Bed) trackable.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
        }
    }
    private void OnDestroy()
    {
        _aRBoundingBoxManager.trackablesChanged.RemoveListener(ColorizeBed);
    }
}
