using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class ShadowPlatform : MonoBehaviour
{
    [SerializeField] private bool invertLogic;

    private Collider2D platformCollider;
    private SpriteRenderer spriteRenderer;

    public static LightController GlobalLight { get; private set; }

    public static void RegisterGlobalLight(LightController light)
    {
        GlobalLight = light;
    }

    private void Awake()
    {
        platformCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void UpdateByLight(Vector2 rayStart, Vector2 rayEnd)
    {
        Vector2 point = platformCollider.bounds.center;
        bool lit = GlobalLight != null && GlobalLight.IsPointLit(point, rayStart, rayEnd);
        bool solid = invertLogic ? !lit : lit;

        platformCollider.enabled = solid;
        spriteRenderer.color = solid ? Color.white : new Color(1f, 1f, 1f, 0.4f);
    }
}
