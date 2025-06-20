using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    private void LateUpdate()
    {
        if (target != null)
        {
            Vector3 newPosition = target.position;
            newPosition.z = transform.position.z; // Keep the camera's z position unchanged
            newPosition.y = 5f;
            transform.position = newPosition;
        }
        //else
        //{
        //    Debug.LogWarning("Target for CameraFollow is not set.");
        //}
    }

    public void SetTarget(Transform newTarget) { target = newTarget; }

}
