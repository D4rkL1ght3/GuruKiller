using UnityEngine;

public class TeacherVisibilityController : MonoBehaviour
{
    [Header("References")]
    public RoomTransitionManager roomTransitionManager;

    [Header("Teacher Visuals")]
    public SpriteRenderer[] spriteRenderers;
    public Animator animator;

    [Header("Teacher Area")]
    public Room hallwayRoom;
    public Room currentTeacherRoom;

    void Start()
    {
        if (roomTransitionManager == null)
        {
            roomTransitionManager = RoomTransitionManager.Instance;
        }

        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        currentTeacherRoom = hallwayRoom;

        if (roomTransitionManager != null)
        {
            roomTransitionManager.OnRoomChanged += HandlePlayerRoomChanged;
            UpdateVisibility(roomTransitionManager.CurrentRoom);
        }
    }

    void OnDestroy()
    {
        if (roomTransitionManager != null)
        {
            roomTransitionManager.OnRoomChanged -= HandlePlayerRoomChanged;
        }
    }

    private void HandlePlayerRoomChanged(Room previousRoom, Room newRoom)
    {
        UpdateVisibility(newRoom);
    }

    public void SetTeacherRoom(Room newTeacherRoom)
    {
        currentTeacherRoom = newTeacherRoom;

        if (roomTransitionManager != null)
        {
            UpdateVisibility(roomTransitionManager.CurrentRoom);
        }
    }

    private void UpdateVisibility(Room playerRoom)
    {
        bool shouldBeVisible = currentTeacherRoom == playerRoom;

        SetVisible(shouldBeVisible);
    }

    private void SetVisible(bool visible)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = visible;
            }
        }

        if (animator != null)
        {
            animator.enabled = visible;
        }
    }

    public bool IsTeacherVisible()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            return true;

        return spriteRenderers[0] != null && spriteRenderers[0].enabled;
    }
}