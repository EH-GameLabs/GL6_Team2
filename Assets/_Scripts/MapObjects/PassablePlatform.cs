using UnityEngine;

public class PassablePlatform : MonoBehaviour
{
    [SerializeField] private Collider platformCollider;
    public void SetTrigger(bool value) { platformCollider.isTrigger = value; }


}
