using UnityEngine;

public class Candy : Collectible
{
    public override void OnCollect()
    {
        if (GameManager.Instance.gameMode == GameMode.OnlineMultiplayer)
        {
            LevelManager.Instance.AddSockServerRpc();
        }
        else
        {
            LevelManager.Instance.AddCandy();
        }
    }
}
