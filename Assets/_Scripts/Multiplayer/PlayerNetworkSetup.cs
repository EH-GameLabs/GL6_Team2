using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(PlayerInput))]
public class PlayerNetworkSetup : NetworkBehaviour
{
    [SerializeField] private PlayerInput input;
    [SerializeField] private InputActionAsset inputActions;

    public override void OnNetworkSpawn()
    {
        // Esegui solo sul proprietario di questo oggetto
        if (!IsOwner) return;

        input = GetComponent<PlayerInput>();
        input.enabled = true;
        input.actions = inputActions;

        // Determina quale personaggio controllare in base all'ID del client.
        // L'Host (ClientId 0) controlla A, il primo client che si unisce (ClientId 1) controlla B.
        // Questa logica può essere resa più complessa (es. scelta nel lobby).
        CharacterID controlledCharacter = (OwnerClientId == 0) ? CharacterID.CharacterA : CharacterID.CharacterB;

        // Registra questo giocatore online con l'InputManager
        InputManager.Instance.RegisterPlayer((int)OwnerClientId, controlledCharacter, input);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        InputManager.Instance.UnregisterPlayer((int)OwnerClientId);
    }
}