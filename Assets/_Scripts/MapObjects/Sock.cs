using System;
using Unity.Netcode;
using UnityEngine;

public class Sock : Collectible
{
    public override void OnCollect()
    {
        if (GameManager.Instance.gameMode != GameMode.OnlineMultiplayer)
        {
            SoundManager.Instance.PlaySFXSound(SoundManager.Instance.Win);
            LevelManager.Instance.EndLevel();
        }
        else
        {
            WinLevelServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void WinLevelServerRpc()
    {
        // Notify all clients that the level is won
        WinLevelClientRpc();
    }

    [ClientRpc]
    private void WinLevelClientRpc()
    {
        SoundManager.Instance.PlaySFXSound(SoundManager.Instance.Win);
        LevelManager.Instance.EndLevel();
    }
}
