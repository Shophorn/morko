using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Quaternion rotation;
    private Vector3 offset;

    void Start ()
    {
        offset = transform.position;
        rotation = transform.rotation;
    }

    void LateUpdate ()
    {
        transform.position = transform.parent.position + offset;
        transform.rotation = rotation;
    }
}