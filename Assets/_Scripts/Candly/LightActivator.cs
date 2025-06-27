using UnityEngine;

public class LightActivator : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent<ILightActivator>(out ILightActivator activator))
        {
            activator.OnLightActivate();
        }
    }
}
