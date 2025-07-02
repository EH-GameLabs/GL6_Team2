using UnityEngine;

public class Candy : Collectible
{
    public override void OnCollect()
    {
        SoundManager.Instance.PlaySFXSound(SoundManager.Instance.CandyGrabbed);
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
