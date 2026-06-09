using UnityEngine;

public class PlayerHidingController : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public SpriteRenderer playerSpriteRenderer;
    public Collider2D playerCollider;

    [Header("Hiding State")]
    public bool IsHiding { get; private set; }

    private HidingSpot currentHidingSpot;

    void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (playerCollider == null)
        {
            playerCollider = GetComponent<Collider2D>();
        }
    }

    void Update()
    {
        if (!IsHiding)
            return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
        {
            ExitHidingSpot();
        }
    }

    public void HideInside(HidingSpot hidingSpot, Transform hidePoint)
    {
        if (IsHiding)
            return;

        currentHidingSpot = hidingSpot;
        IsHiding = true;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (hidePoint != null)
        {
            transform.position = hidePoint.position;
        }

        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.enabled = false;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }
    }

    public void ExitHidingSpot()
    {
        if (!IsHiding)
            return;

        Transform exitPoint = null;

        if (currentHidingSpot != null)
        {
            exitPoint = currentHidingSpot.exitPoint;
        }

        if (exitPoint != null)
        {
            transform.position = exitPoint.position;
        }

        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.enabled = true;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (currentHidingSpot != null)
        {
            currentHidingSpot.OnPlayerExited();
        }

        currentHidingSpot = null;
        IsHiding = false;
    }
}