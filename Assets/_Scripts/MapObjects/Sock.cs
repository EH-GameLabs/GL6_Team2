using UnityEngine;

public class Sock : Collectible
{
    public override void OnCollect()
    {
        SoundManager.Instance.PlaySFXSound(SoundManager.Instance.Win);
        LevelManager.Instance.EndLevel();
    }
}
