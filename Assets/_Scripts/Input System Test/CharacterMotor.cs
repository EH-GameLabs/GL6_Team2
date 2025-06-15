using UnityEngine;

public class CharacterMotor : MonoBehaviour
{
    [Header("Character Identity")]
    [Tooltip("Imposta questo nell'inspector: CharacterA o CharacterB")]
    [SerializeField] private CharacterID characterId;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private PlayerInputData currentInput;

    void Update()
    {
        // Chiede costantemente all'InputManager i suoi input
        currentInput = InputManager.Instance.GetInputForCharacter(characterId);

        // Applica gli input
        HandleMovement();
        HandleActions();
    }

    private void HandleMovement()
    {
        if (currentInput.Move != Vector2.zero)
        {
            Vector3 movement = new Vector3(currentInput.Move.x, 0, 0);
            transform.Translate(movement * moveSpeed * Time.deltaTime);
        }
    }

    private void HandleActions()
    {
        if (currentInput.JumpPressed)
        {
            Debug.Log($"{characterId} is Jumping!");
            // Aggiungi qui la tua logica di salto
        }
        if (currentInput.FirePressed)
        {
            Debug.Log($"{characterId} is Firing!");
            // Aggiungi qui la tua logica di sparo
        }
    }
}