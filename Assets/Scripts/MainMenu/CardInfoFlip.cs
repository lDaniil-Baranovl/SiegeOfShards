using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CardInfoFlip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("XR Input (правый контроллер, кнопка A)")]
    public InputActionProperty flipAction;

    [Header("Source Image для переворота")]
    public Image targetImage;

    [Tooltip("Изображение, которое показывается на обратной стороне карты")]
    public Sprite infoSprite;

    [Header("Анимация")]
    public float flipDuration = 0.3f;

    private Sprite frontSprite;
    private bool isFlipped;
    private bool isAnimating;
    private bool isHovered;

    private void Awake()
    {
        flipAction.action.Enable();

        if (targetImage == null)
            targetImage = GetComponent<Image>();

        frontSprite = targetImage.sprite;
    }

    private void Update()
    {
        if (!isHovered || isAnimating)
            return;

        if (flipAction.action.WasPressedThisFrame())
            StartCoroutine(FlipRoutine());
    }

    public void OnPointerEnter(PointerEventData eventData) => isHovered = true;

    public void OnPointerExit(PointerEventData eventData) => isHovered = false;

    private IEnumerator FlipRoutine()
    {
        isAnimating = true;

        Vector3 scale = transform.localScale;
        float fullX = Mathf.Abs(scale.x);
        float halfDuration = flipDuration / 2f;

        yield return ScaleX(scale.y, scale.z, fullX, 0f, halfDuration);

        targetImage.sprite = isFlipped ? frontSprite : infoSprite;
        isFlipped = !isFlipped;

        yield return ScaleX(scale.y, scale.z, 0f, fullX, halfDuration);

        isAnimating = false;
    }

    private IEnumerator ScaleX(float y, float z, float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float x = Mathf.Lerp(from, to, t / duration);
            transform.localScale = new Vector3(x, y, z);
            yield return null;
        }

        transform.localScale = new Vector3(to, y, z);
    }
}
