using UnityEngine;

public class XRPlayer : MonoBehaviour
{
    public static XRPlayer Instance;

    public Transform leftController;
    public Transform rightController;

    void Awake()
    {
        Instance = this;
    }
}


