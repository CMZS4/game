using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private Vector3 localOffset = new Vector3(4f, 0f, 0f);
    [SerializeField] private float speed = 2f;

    private Vector3 startPos;

    private void Awake()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
        transform.position = Vector3.Lerp(startPos, startPos + localOffset, t);
    }
}
