using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.OpenXR.Features.Meta;
using UnityEngine.XR.ARSubsystems;
public class ResetRoomButton : MonoBehaviour
{
    public void ResetRoom()
    {
        var arSession = FindAnyObjectByType<ARSession>();
        if (arSession == null) return;

        var subsystem = arSession.subsystem;

        bool success = (subsystem as MetaOpenXRSessionSubsystem)?.TryRequestSceneCapture() ?? false;

        Debug.Log($"Запрос на захват сцены Meta OpenXR завершен: {success}");
    }
}
