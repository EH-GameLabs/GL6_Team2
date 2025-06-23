using UnityEngine;

public class AnchorPoint : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    private void Start()
    {
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer component not found on AnchorPoint.");
        }
        ChangeMeshRendererColor(Color.red);
    }

    private void ChangeMeshRendererColor(Color color)
    {
        meshRenderer.material.color = color;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Grapple3D grapple = other.GetComponent<Grapple3D>();
            if (grapple != null && !grapple.isGrappling)
            {
                grapple.canGrapple = true;
                grapple.SetAnchorPoint(transform);
                // animation canGrapple = true
                ChangeMeshRendererColor(Color.green);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Grapple3D grapple = other.GetComponent<Grapple3D>();
            if (grapple != null && !grapple.isGrappling)
            {
                grapple.canGrapple = false;
                grapple.SetAnchorPoint(null);
                // animation canGrapple = false
                ChangeMeshRendererColor(Color.red);
            }
        }
    }
}
