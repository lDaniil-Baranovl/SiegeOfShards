using UnityEngine;

public class FloatingLimit : MonoBehaviour
{
    public float minY = -1f;
    public float resetHeight = 1.5f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (transform.position.y < minY)
        {
            rb.linearVelocity = Vector3.zero;
            transform.position = new Vector3(0, resetHeight, 0);
        }
    }
}
