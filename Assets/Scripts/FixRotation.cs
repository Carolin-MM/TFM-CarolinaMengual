using UnityEngine;

public class FixRotation : MonoBehaviour
{
    private Quaternion _rotation;

    private void Awake()
    {
       _rotation = transform.rotation;
    }

    private void LateUpdate()
    {
        transform.rotation = _rotation;
    }
}
