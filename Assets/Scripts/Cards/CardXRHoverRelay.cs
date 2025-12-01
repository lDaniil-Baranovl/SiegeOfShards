using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CardXRHoverRelay : MonoBehaviour
{
    private CardDragXR card;

    void Awake()
    {
        card = GetComponent<CardDragXR>();

        var grab = GetComponent<XRGrabInteractable>();
        grab.hoverEntered.AddListener(OnHoverEnter);
        grab.hoverExited.AddListener(OnHoverExit);
    }

    public void OnHoverEnter(HoverEnterEventArgs arg)
    {
        card.OnHoverEntered();
    }

    public void OnHoverExit(HoverExitEventArgs arg)
    {
        card.OnHoverExited();
    }
}
