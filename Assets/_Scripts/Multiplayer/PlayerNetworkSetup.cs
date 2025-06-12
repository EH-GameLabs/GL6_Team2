using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerNetworkSetup : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Esegui solo sul proprietario di questo oggetto
        if (!IsOwner) return;

        // Determina quale personaggio controllare in base all'ID del client.
        // L'Host (ClientId 0) controlla A, il primo client che si unisce (ClientId 1) controlla B.
        // Questa logica può essere resa più complessa (es. scelta nel lobby).
        CharacterID controlledCharacter = (OwnerClientId == 0) ? CharacterID.CharacterA : CharacterID.CharacterB;

        // Registra questo giocatore online con l'InputManager
        InputManager.Instance.RegisterPlayer((int)OwnerClientId, controlledCharacter, GetComponent<PlayerInput>());
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        InputManager.Instance.UnregisterPlayer((int)OwnerClientId);
    }
}