using UnityEngine;

public class Sock : Collectible
{
    public override void OnCollect()
    {
        LevelManager.Instance.EndLevel();
    }
}
