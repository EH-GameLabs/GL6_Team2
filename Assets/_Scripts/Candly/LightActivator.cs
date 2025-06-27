using UnityEngine;

public class LightActivator : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        Debug.Log("Candly ha incontrato: " + other.name);
        if (other.TryGetComponent<ILightActivator>(out ILightActivator activator))
        {
            activator.OnLightActivate();
        }
    }
}
