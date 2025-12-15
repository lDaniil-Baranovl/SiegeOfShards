using UnityEngine;

public class OpenXRControllerFlipFix : MonoBehaviour
{
    [Tooltip("Поворот, который будет добавлен ТОЛЬКО в билде")]
    public Vector3 rotationOffset = new Vector3(180f, 0f, 0f);

    private Quaternion _offset;

    void Awake()
    {
#if !UNITY_EDITOR
        _offset = Quaternion.Euler(rotationOffset);
#endif
    }

    void LateUpdate()
    {
#if !UNITY_EDITOR
        // OpenXR может перезаписывать rotation каждый кадр,
        // поэтому фикс применяем в LateUpdate
        transform.localRotation = transform.localRotation * _offset;
#endif
    }
}
