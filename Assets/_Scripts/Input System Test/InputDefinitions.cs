using UnityEngine;

// Enum per identificare univocamente i personaggi
public enum CharacterID
{
    CharacterA,
    CharacterB
}

// Enum per la modalità di gioco
public enum GameMode
{
    SinglePlayer,
    LocalMultiplayer,
    OnlineMultiplayer
}

// Struct che rappresenta una "istantanea" completa degli input.
// Questa è l'informazione che i personaggi riceveranno.
public struct PlayerInputData
{
    public Vector2 Move;
    public bool JumpPressed;
    public bool FirePressed;
    // Aggiungi altre azioni qui se necessario
}