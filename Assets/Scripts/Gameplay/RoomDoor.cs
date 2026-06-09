using UnityEngine;

public class RoomDoor : MonoBehaviour, Interactable
{
    [Header("Transition")]
    public Room targetRoom;
    public Transform targetSpawnPoint;

    [Header("Highlight")]
    public SpriteRenderer spriteRenderer;
    public Color highlightColor = Color.yellow;

    private Color originalColor;

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
    }

    public void Interact()
    {
        RoomTransitionManager.Instance.TransitionToRoom(
            targetRoom,
            targetSpawnPoint
        );
    }

    public void CloseUI()
    {
        // Doors do not need UI.
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
}