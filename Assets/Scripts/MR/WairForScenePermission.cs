using UnityEngine;
using UnityEngine.XR.ARFoundation;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class WaitForScenePermission : MonoBehaviour
{
    const string PermissionId = "com.oculus.permission.USE_SCENE";

    [SerializeField] ARBoundingBoxManager boundingBoxManager;

    private void Awake()
    {
        if (boundingBoxManager) boundingBoxManager.enabled = false;
    }

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (Permission.HasUserAuthorizedPermission(PermissionId))
        {
            OnGranted(PermissionId);
        }
        else
        {
            var cb = new PermissionCallbacks();
            cb.PermissionGranted += OnGranted;
            cb.PermissionDenied += OnDenied;
            Permission.RequestUserPermission(PermissionId, cb);
        }
#else
        OnGranted(PermissionId);
#endif
    }

    void OnGranted(string _)
    {
        Debug.Log("USE_SCENE granted.");
        if (boundingBoxManager) boundingBoxManager.enabled = true;
    }

    void OnDenied(string _)
    {
        Debug.LogWarning("USE_SCENE denied. Managers stay disabled.");
    }
}