using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;

    [Header("Interaction")]
    public float maxInteractionDistance = 2f;

    [Header("Map Bounds")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 moveInput;
    private Vector2 lastMoveDirection;

    [Header("Footstep Audio")]
    public AudioSource footstepAudioSource;
    public AudioClip footstepClip;
    public float footstepInterval = 0.35f;

    private float footstepTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        lastMoveDirection = Vector2.down;

        if (footstepAudioSource == null)
        {
            footstepAudioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        // Input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        moveInput = moveInput.normalized;

        // Save last direction
        if (moveInput != Vector2.zero)
        {
            lastMoveDirection = moveInput;
        }

        HandleAnimation();
        HandleFootsteps();
    }

    void FixedUpdate()
    {
        // Stop horizontal movement if trying to move outside the map
        if (rb.position.x <= minBounds.x && moveInput.x < 0f)
        {
            moveInput.x = 0f;
        }
        else if (rb.position.x >= maxBounds.x && moveInput.x > 0f)
        {
            moveInput.x = 0f;
        }

        // Stop vertical movement if trying to move outside the map
        if (rb.position.y <= minBounds.y && moveInput.y < 0f)
        {
            moveInput.y = 0f;
        }
        else if (rb.position.y >= maxBounds.y && moveInput.y > 0f)
        {
            moveInput.y = 0f;
        }

        rb.linearVelocity = moveInput * moveSpeed;

        // Final safety clamp
        Vector2 clampedPosition = rb.position;

        clampedPosition.x = Mathf.Clamp(
            clampedPosition.x,
            minBounds.x,
            maxBounds.x
        );

        clampedPosition.y = Mathf.Clamp(
            clampedPosition.y,
            minBounds.y,
            maxBounds.y
        );

        rb.position = clampedPosition;
    }

    void HandleAnimation()
    {
        if (animator == null)
            return;

        // Send values to animator
        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);

        animator.SetFloat("LastMoveX", lastMoveDirection.x);
        animator.SetFloat("LastMoveY", lastMoveDirection.y);

        animator.SetBool("IsMoving", moveInput != Vector2.zero);
    }

    void HandleFootsteps()
    {
        if (footstepAudioSource == null || footstepClip == null)
            return;

        bool isMoving = moveInput != Vector2.zero;

        if (!isMoving)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0f)
        {
            footstepAudioSource.PlayOneShot(footstepClip);
            footstepTimer = footstepInterval;
        }
    }

    void OnDisable()
    {
        rb.linearVelocity = Vector2.zero;
    }
}