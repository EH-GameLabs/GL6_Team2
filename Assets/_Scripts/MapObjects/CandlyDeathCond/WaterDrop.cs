using Unity.Netcode;
using UnityEngine;

public class WaterDrop : NetworkBehaviour
{
    // parte da un punto in alto e scende verso il basso
    public float speed = 5f; // velocit� di caduta


    private void Update()
    {
        // Muove la goccia d'acqua verso il basso
        transform.position += Vector3.down * speed * Time.deltaTime;
        // Se la goccia esce dalla visuale, la distrugge
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.GetComponent<PlayerController>() == null) return;

        // Se la goccia colpisce candly
        if (other.transform.GetComponent<PlayerController>().characterId == CharacterID.CharacterB)
        {
            SoundManager.Instance.PlaySFXSound(SoundManager.Instance.CandlyDie);
            GameManager.Instance.LoseLife(CharacterID.CharacterB);
            Destroy(gameObject);
        }
        else
        {
            SoundManager.Instance.PlaySFXSound(SoundManager.Instance.WaterDrop);
            Destroy(gameObject);
        }
    }
}
