using UnityEngine;

// Walks left and right around its starting point.
// Temporary: this becomes part of the full enemy AI later.
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 1.5f;      // units per second
    [SerializeField] private float patrolDistance = 3f;   // how far he walks from the start before turning

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private int direction = 1; // 1 = right, -1 = left

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
    }

    private void Update()
    {
        // Move a little bit every frame
        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;

        // Turn around when he goes too far from the start
        float distanceFromStart = transform.position.x - startPosition.x;
        if (distanceFromStart >= patrolDistance) direction = -1;
        else if (distanceFromStart <= -patrolDistance) direction = 1;

        // The art faces right, so flip it when walking left
        spriteRenderer.flipX = direction < 0;
    }

    // Draws the patrol range in the Scene view when he is selected
    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? startPosition : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(center + Vector3.left * patrolDistance, center + Vector3.right * patrolDistance);
    }
}