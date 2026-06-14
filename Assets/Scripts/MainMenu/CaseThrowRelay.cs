using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CaseThrowRelay : MonoBehaviour
{
    private CaseCubeController caseController;

    void Awake()
    {
        caseController = GetComponent<CaseCubeController>();

        var grab = GetComponent<XRGrabInteractable>();
        grab.selectExited.AddListener(OnReleased);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        caseController.OnCubeThrown();
    }
}
