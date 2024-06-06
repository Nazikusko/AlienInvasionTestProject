using UnityEngine;

internal class CameraMove : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform;

    void LateUpdate()
    {
        transform.position = _targetTransform.position;
    }
}
