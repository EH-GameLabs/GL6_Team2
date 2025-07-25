using UnityEngine;

// Enum per identificare univocamente i personaggi
public enum CharacterID
{
    CharacterA,
    CharacterB
}

// Enum per la modalit� di gioco
public enum GameMode
{
    SinglePlayer,
    LocalMultiplayer,
    OnlineMultiplayer
}

// Struct che rappresenta una "istantanea" completa degli input.
// Questa � l'informazione che i personaggi riceveranno.
[System.Serializable]
public struct PlayerInputData
{
    public Vector2 Move;
    public Vector2 Look;
    public bool JumpPressed;
    public bool FirePressed;
    // Aggiungi altre azioni qui se necessario
}