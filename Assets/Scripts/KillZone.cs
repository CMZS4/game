using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LevelManager.Instance?.RespawnPlayer();
        }
    }
}
