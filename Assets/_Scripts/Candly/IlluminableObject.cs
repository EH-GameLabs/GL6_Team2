using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class IlluminableObject : MonoBehaviour, ILightActivator
{
    [Header("Base Settings")]
    [SerializeField] private float lightDuration = 0.5f;
    [SerializeField] private UnityEvent OnStartEvent;

    [Header("Events")]
    [SerializeField] private UnityEvent onLightActivate;
    [SerializeField] private UnityEvent onLightDeactivate;

    private float currentLightDuration = 0;
    private bool isActive = false;

    private void Start()
    {
        OnStartEvent?.Invoke();
        isActive = false;
    }

    private void Update()
    {
        if (currentLightDuration <= 0) return;

        currentLightDuration -= Time.deltaTime;
        if (currentLightDuration <= 0)
        {
            OnLightDeactivate();
            currentLightDuration = 0;
        }
    }

    public void OnLightActivate()
    {
        if (isActive)
        {
            currentLightDuration = lightDuration;
            return;
        }

        if (GameManager.Instance.gameMode == GameMode.OnlineMultiplayer)
        {
            OnLightActivateServerRpc();
        }
        else
        {
            ActivateEvent();
        }
    }

    public void OnLightDeactivate()
    {
        if (GameManager.Instance.gameMode == GameMode.OnlineMultiplayer)
        {
            OnLightDeactivateServerRpc();
        }
        else
        {
            DeactivateEvent();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnLightActivateServerRpc()
    {
        OnLightActivateClientRpc();
    }

    [ClientRpc]
    private void OnLightActivateClientRpc()
    {
        ActivateEvent();
    }

    private void ActivateEvent()
    {
        isActive = true;
        currentLightDuration = lightDuration;
        onLightActivate?.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnLightDeactivateServerRpc()
    {
        OnLightDeactivateClientRpc();
    }

    [ClientRpc]
    private void OnLightDeactivateClientRpc()
    {
        DeactivateEvent();
    }

    private void DeactivateEvent()
    {
        onLightDeactivate?.Invoke();
        isActive = false;
    }

    // tutte le possibili actions base

    public void SetGameObjectActive(bool active) { gameObject.SetActive(active); }
    public void ActivateGameObject(GameObject gameObject) { gameObject.SetActive(true); }
    public void DeactivateGameObject(GameObject gameObject) { gameObject.SetActive(false); }

    public float GetLightDuration()
    {
        return lightDuration;
    }
}
