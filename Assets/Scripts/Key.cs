using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Key : MonoBehaviour
{
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player")) return;

        collected = true;
        LevelManager.Instance?.CollectKey();
        gameObject.SetActive(false);
    }
}
