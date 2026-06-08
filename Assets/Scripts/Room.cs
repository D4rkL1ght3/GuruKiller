using UnityEngine;

public class Room : MonoBehaviour
{
    public enum RoomType
    {
        Hallway,
        Classroom,
        Other
    }

    [Header("Room Info")]
    public RoomType roomType;

    [Header("Player Bounds")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    [Header("Teacher")]
    public Transform[] teacherPatrolPoints;
    public Transform teacherEntryPoint;

    public void ShowRoom()
    {
        gameObject.SetActive(true);
    }

    public void HideRoom()
    {
        gameObject.SetActive(false);
    }
}