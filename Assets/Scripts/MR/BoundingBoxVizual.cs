using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class BoundingBoxVizual : MonoBehaviour
{
    ARBoundingBox boundingBox;
    void Awake()
    {
        boundingBox = GetComponentInParent<ARBoundingBox>();
    }
    void Update()
    {
        if (boundingBox == null) return;
        Vector3 size = boundingBox.size;

        transform.localScale = size;
        transform.position = boundingBox.pose.position;
        transform.rotation = boundingBox.pose.rotation;
    }
}
