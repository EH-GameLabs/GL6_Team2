using UnityEngine;

public class Sock : Collectible
{
    public override void OnCollect()
    {
        if (GameManager.Instance.gameMode == GameMode.OnlineMultiplayer)
        {
            LevelManager.Instance.AddSockServerRpc();
        }
        else
        {
            LevelManager.Instance.AddSock();
        }
    }
}
