using UnityEngine;

public class PlayerHidingController : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public SpriteRenderer playerSpriteRenderer;
    public Collider2D playerCollider;
    public TeacherAI teacherAI;

    [Header("Hiding State")]
    public bool IsHiding { get; private set; }

    private HidingSpot currentHidingSpot;
    private bool canExitHidingSpot = true;

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

        if (!canExitHidingSpot)
            return;
    }

    public void HideInside(HidingSpot hidingSpot, Transform hidePoint)
    {
        if (IsHiding)
            return;

        currentHidingSpot = hidingSpot;
        IsHiding = true;
        canExitHidingSpot = true;

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
            playerCollider.enabled = false;
        }

        if (teacherAI != null)
        {
            teacherAI.HandlePlayerHid(hidingSpot);
        }
    }

    public void ExitHidingSpot()
    {
        if (!IsHiding)
            return;

        ExitHidingSpotInternal();
    }

    public void ForceExitHidingSpot()
    {
        if (!IsHiding)
            return;

        ExitHidingSpotInternal();
    }

    public void SetCanExitHidingSpot(bool canExit)
    {
        canExitHidingSpot = canExit;
    }

    private void ExitHidingSpotInternal()
    {
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

        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        if (currentHidingSpot != null)
        {
            currentHidingSpot.OnPlayerExited();
        }

        currentHidingSpot = null;
        IsHiding = false;
        canExitHidingSpot = true;
    }
}