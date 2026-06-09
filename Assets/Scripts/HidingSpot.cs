using UnityEngine;

public class HidingSpot : MonoBehaviour, Interactable
{
    [Header("Hiding Points")]
    public Transform hidePoint;
    public Transform exitPoint;

    [Header("Player")]
    public PlayerHidingController playerHidingController;

    [Header("Highlight")]
    public SpriteRenderer spriteRenderer;
    public Color highlightColor = Color.yellow;

    [Header("Optional Visuals")]
    public Sprite openVisual;
    public Sprite closedVisual;

    private Color originalColor;
    private bool isOccupied;

    void Start()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        SetDoorVisual(true);
    }

    public void Interact()
    {
        if (playerHidingController == null)
            return;

        if (isOccupied)
        {
            playerHidingController.ExitHidingSpot();
            return;
        }

        isOccupied = true;

        SetDoorVisual(false);

        playerHidingController.HideInside(this, hidePoint);
    }

    public void CloseUI()
    {
        if (playerHidingController == null)
            return;

        if (isOccupied)
        {
            playerHidingController.ExitHidingSpot();
        }
    }

    public void Highlight()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = highlightColor;
        }
    }

    public void RemoveHighlight()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    public void OnPlayerExited()
    {
        isOccupied = false;
        SetDoorVisual(true);
    }

    private void SetDoorVisual(bool open)
    {
        if (openVisual != null && closedVisual != null)
        {
            spriteRenderer.sprite = open ? openVisual : closedVisual;
        }
    }
}