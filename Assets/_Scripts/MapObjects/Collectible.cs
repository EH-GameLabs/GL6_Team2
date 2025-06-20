using UnityEngine;

public abstract class Collectible : MonoBehaviour, ICollectibles
{
    public abstract void OnCollect();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnCollect();
            Destroy(gameObject);
        }
    }
}
