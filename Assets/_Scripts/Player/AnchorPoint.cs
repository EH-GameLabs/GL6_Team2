using System;
using UnityEngine;

public class AnchorPoint : MonoBehaviour
{
    private SpriteRenderer[] spriteRenderers;

    private void Start()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        if (spriteRenderers == null)
        {
            Debug.LogError("MeshRenderer component not found on AnchorPoint.");
        }
        SetSpriteEnable(false);
    }

    private void SetSpriteEnable(bool isOpen)
    {
        int openIndex = isOpen ? 1 : 0;

        foreach (var spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.gameObject.SetActive(false);
            }
        }

        spriteRenderers[openIndex].gameObject.SetActive(true);
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
                SetSpriteEnable(true);
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
                SetSpriteEnable(false);
            }
        }
    }
}
