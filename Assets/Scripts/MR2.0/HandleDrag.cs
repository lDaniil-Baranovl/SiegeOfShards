using UnityEngine;
using UnityEngine.InputSystem;


public class HandleDrag : MonoBehaviour
{
    public Transform controller;                // Правый контроллер
    public InputActionReference triggerAction;  // Trigger action

    private bool isHeld = false;
    private Vector3 offset;

    void Update()
    {
        bool pressed = triggerAction.action.IsPressed();

        if (pressed && !isHeld)
        {
            isHeld = true;
            offset = transform.position - controller.position;
        }

        if (!pressed && isHeld)
        {
            isHeld = false;
        }

        if (isHeld)
        {
            transform.position = controller.position + offset;
        }
    }
}
