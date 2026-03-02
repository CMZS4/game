using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class Door : MonoBehaviour
{
    [SerializeField] private int requiredKeys = 1;
    [SerializeField] private Color lockedColor = Color.red;
    [SerializeField] private Color unlockedColor = Color.green;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        bool unlocked = LevelManager.Instance != null && LevelManager.Instance.CurrentKeys >= requiredKeys;
        spriteRenderer.color = unlocked ? unlockedColor : lockedColor;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || LevelManager.Instance == null) return;
        if (LevelManager.Instance.CurrentKeys >= requiredKeys)
        {
            LevelManager.Instance.NextLevel();
        }
    }
}
