using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.OpenXR.Features.Meta;
public class RoomScanner : MonoBehaviour
{
    [SerializeField] InputActionReference _roomScanButton;
    private ARSession _arSession;
    private void Awake()
    {
        _arSession = FindAnyObjectByType<ARSession>();
    }
    private void OnEnable ()
    {
        _roomScanButton.action.started += ScanRoom;
    }
    private void OnDisable()
    {
        _roomScanButton.action.started -= ScanRoom;
    }
    private void ScanRoom(InputAction.CallbackContext context)
    {
        MetaOpenXRSessionSubsystem subsystem = _arSession?.subsystem as MetaOpenXRSessionSubsystem;
        if (subsystem!=null)
        {
            bool ok = subsystem.TryRequestSceneCapture();
        }
    }
}
