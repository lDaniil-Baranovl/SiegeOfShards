using UnityEngine;

public class CrystalAnimationAdvanced : MonoBehaviour
{
    [SerializeField] private float floatHeight = 0.5f;
    [SerializeField] private float floatSpeed = 1f;

    [SerializeField] private float rotationSpeedX = 0f;
    [SerializeField] private float rotationSpeedY = -30f;
    [SerializeField] private float rotationSpeedZ = 0f;

    [SerializeField] private bool enablePulsing = true;
    [SerializeField] private float pulseIntensity = 0.1f;
    [SerializeField] private float pulseSpeed = 2f;


    [SerializeField] private bool randomizeParameters = true;

    private Vector3 startPosition;
    private Vector3 startScale;
    private float timeCounter;
    private float randomOffset;

    void Start()
    {
   
        startPosition = transform.localPosition;
        startScale = transform.localScale;

        if (randomizeParameters)
        {
            randomOffset = Random.Range(0f, 360f);
            floatHeight *= Random.Range(0.8f, 1.2f);
            floatSpeed *= Random.Range(0.8f, 1.2f);
            rotationSpeedY *= Random.Range(0.8f, 1.2f);
        }

        timeCounter = randomOffset * Mathf.Deg2Rad;
    }

    void Update()
    {
        timeCounter += Time.deltaTime * floatSpeed;

        float newY = startPosition.y + Mathf.Sin(timeCounter) * floatHeight;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);

        transform.Rotate(
            rotationSpeedX * Time.deltaTime,
            rotationSpeedY * Time.deltaTime, 
            rotationSpeedZ * Time.deltaTime,
            Space.World
        );

        if (enablePulsing)
        {
            float pulse = 1f + Mathf.Sin(timeCounter * pulseSpeed) * pulseIntensity;
            transform.localScale = startScale * pulse;
        }
    }
}