using UnityEngine;

public class PassablePlatform : MonoBehaviour
{
    [SerializeField] private Collider platformCollider;

    private void Awake()
    {
        if (platformCollider == null)
        {
            platformCollider = GetComponentInChildren<Collider>();
        }
    }

    public void SetTrigger(bool value) { platformCollider.isTrigger = value; }


}
