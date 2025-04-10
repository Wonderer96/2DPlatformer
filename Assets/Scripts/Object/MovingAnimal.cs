using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class MovingAnimal : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Vector2 raycastOffset = new Vector2(0.5f, 0f);
    [SerializeField] private float detectionDistance = 0.1f;

    [Header("Player Ride Settings")]
    [SerializeField] private float playerCheckRadius = 0.1f;
    [SerializeField] private Vector2 playerCheckOffset = new Vector2(0f, 0.5f);

    public Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private int _currentDirection = 1;
    private Transform _playerParent;
    private Transform _currentPlayer;

    [Header("Turn Settings")]
    [SerializeField] private float maxTurnsPerSecond = 2f;
    private float lastTurnTime = -Mathf.Infinity;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentDirection = _spriteRenderer.flipX ? -1 : 1;
    }

    private void FixedUpdate()
    {
        Movement();
        ObstacleDetection();
        CheckPlayerRiding();
    }

    private void Movement()
    {
        _rb.velocity = new Vector2(moveSpeed * _currentDirection, _rb.velocity.y);
    }

    private void ObstacleDetection()
    {
        Vector2 rayOrigin = (Vector2)transform.position +
                            new Vector2(raycastOffset.x * _currentDirection, raycastOffset.y);

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(obstacleLayer);
        filter.useTriggers = false;

        RaycastHit2D[] hits = new RaycastHit2D[1]; // 只需要一个命中即可
        int hitCount = Physics2D.Raycast(rayOrigin, Vector2.right * _currentDirection, filter, hits, detectionDistance);

        if (hitCount > 0 && hits[0].collider != null)
        {
            ChangeDirection();
        }
    }



    private void ChangeDirection()
    {
        float timeSinceLastTurn = Time.time - lastTurnTime;
        float minInterval = 1f / maxTurnsPerSecond;

        if (timeSinceLastTurn < minInterval)
            return;

        lastTurnTime = Time.time;

        _currentDirection *= -1;
        _spriteRenderer.flipX = !_spriteRenderer.flipX;
    }


    private void CheckPlayerRiding()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            (Vector2)transform.position + playerCheckOffset,
            playerCheckRadius
        );

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                HandlePlayerRiding(hit.transform);
                return;
            }
        }

        ReleasePlayer();
    }

    private void HandlePlayerRiding(Transform player)
    {
        if (player.parent != transform)
        {
            _playerParent = player.parent;
            _currentPlayer = player;
            player.SetParent(transform);
        }
    }

    private void ReleasePlayer()
    {
        if (_currentPlayer != null)
        {
            _currentPlayer.SetParent(_playerParent);
            _playerParent = null;
            _currentPlayer = null;
        }
    }


    private void OnDrawGizmosSelected()
    {
        // Draw obstacle detection ray
        Gizmos.color = Color.red;
        Vector2 rayOrigin = (Vector2)transform.position +
                           new Vector2(raycastOffset.x * _currentDirection, raycastOffset.y);
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector2.right * _currentDirection * detectionDistance);

        // Draw player check area
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere((Vector2)transform.position + playerCheckOffset, playerCheckRadius);
    }
}
