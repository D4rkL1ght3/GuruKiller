using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Player Bounds")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    public void ShowRoom()
    {
        gameObject.SetActive(true);
    }

    public void HideRoom()
    {
        gameObject.SetActive(false);
    }
}