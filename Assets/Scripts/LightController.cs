using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LightController : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 120f;
    [SerializeField] private float rayDistance = 20f;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask shadowPlatformMask;
    [SerializeField] private float interactRadius = 2f;

    private LineRenderer lineRenderer;
    private bool dragging;
    private Camera mainCam;
    private bool grabbedByPlayer;
    private Transform player;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        mainCam = Camera.main;
        ShadowPlatform.RegisterGlobalLight(this);
    }

    private void OnDestroy()
    {
        if (ShadowPlatform.GlobalLight == this)
        {
            ShadowPlatform.RegisterGlobalLight(null);
        }
    }

    private void Update()
    {
        RotateLamp();
        HandleMove();
        CastLightRay();
    }

    private void RotateLamp()
    {
        float rot = 0f;
        if (Input.GetKey(KeyCode.Q)) rot += 1f;
        if (Input.GetKey(KeyCode.E)) rot -= 1f;
        transform.Rotate(Vector3.forward, rot * rotateSpeed * Time.deltaTime);
    }

    private void HandleMove()
    {
        if (mainCam == null) mainCam = Camera.main;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouse = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mouse.z = transform.position.z;
            if (Vector2.Distance(mouse, transform.position) <= interactRadius)
            {
                dragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        }

        if (dragging)
        {
            Vector3 mouse = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mouse.z = transform.position.z;
            transform.position = mouse;
        }

        if (player != null)
        {
            bool inRange = Vector2.Distance(player.position, transform.position) <= interactRadius;
            if (Input.GetKey(KeyCode.E) && inRange)
            {
                grabbedByPlayer = true;
            }
            else if (!Input.GetKey(KeyCode.E))
            {
                grabbedByPlayer = false;
            }

            if (grabbedByPlayer)
            {
                Vector3 target = player.position + new Vector3(1.25f, 0.25f, 0f);
                transform.position = Vector3.Lerp(transform.position, target, 16f * Time.deltaTime);
            }
        }
    }

    private void CastLightRay()
    {
        Vector2 origin = transform.position;
        Vector2 dir = transform.right;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, rayDistance, obstacleMask);
        Vector2 end = hit.collider != null ? hit.point : origin + dir * rayDistance;

        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, end);

        var platforms = FindObjectsOfType<ShadowPlatform>();
        foreach (var platform in platforms)
        {
            platform.UpdateByLight(origin, end);
        }
    }

    public bool IsPointLit(Vector2 point, Vector2 rayStart, Vector2 rayEnd)
    {
        Vector2 line = rayEnd - rayStart;
        float length = line.magnitude;
        if (length <= Mathf.Epsilon) return false;

        Vector2 lineDir = line / length;
        float projection = Vector2.Dot(point - rayStart, lineDir);
        if (projection < 0f || projection > length) return false;

        Vector2 closest = rayStart + lineDir * projection;
        float dist = Vector2.Distance(point, closest);
        return dist < 0.55f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            grabbedByPlayer = false;
            player = null;
        }
    }
}
