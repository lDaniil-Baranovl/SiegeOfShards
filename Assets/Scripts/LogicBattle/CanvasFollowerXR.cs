using UnityEngine;

public class CanvasFollowerXR : MonoBehaviour
{
    public Transform xrOrigin;     // XR Origin (или Camera Offset)
    public Transform xrCamera;     // Основная XR камера
    public float distance = 1.5f;  // Расстояние перед глазами
    public float heightOffset = 0f;

    private void OnEnable()
    {
        PositionInFrontOfPlayer();
    }

    public void PositionInFrontOfPlayer()
    {
        if (xrCamera == null) return;

        // Вычисляем направление только по горизонту
        Vector3 forwardFlat = xrCamera.forward;
        forwardFlat.y = 0;
        forwardFlat.Normalize();

        // Ставим канвас перед игроком
        transform.position = xrCamera.position + forwardFlat * distance;

        // Фиксируем высоту
        transform.position = new Vector3(
            transform.position.x,
            xrCamera.position.y + heightOffset,
            transform.position.z
        );

        // Разворачиваем канвас к игроку по Y (без X/Z)
        Vector3 lookPos = xrCamera.position;
        lookPos.y = transform.position.y;

        transform.LookAt(lookPos);

        // ПОВОРОТ НА 180°, чтобы лицевая сторона была к игроку
        transform.Rotate(0, 180f, 0);

        // Убираем X и Z вращения
        Vector3 rot = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0, rot.y, 0);
    }

}
