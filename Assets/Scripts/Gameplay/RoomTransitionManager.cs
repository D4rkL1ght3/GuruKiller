using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RoomTransitionManager : MonoBehaviour
{
    public static RoomTransitionManager Instance;

    [Header("References")]
    public Room startingRoom;
    public Transform player;
    public PlayerController playerController;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 0.35f;

    private Room currentRoom;
    private bool isTransitioning;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentRoom = startingRoom;

        Room[] allRooms = FindObjectsByType<Room>(FindObjectsSortMode.None);

        foreach (Room room in allRooms)
        {
            if (room == startingRoom)
            {
                room.ShowRoom();
            }
            else
            {
                room.HideRoom();
            }
        }

        ApplyRoomSettings(currentRoom);

        SetFadeAlpha(0f);
    }

    public void TransitionToRoom(Room targetRoom, Transform targetSpawnPoint)
    {
        if (isTransitioning)
            return;

        StartCoroutine(RoomTransitionRoutine(targetRoom, targetSpawnPoint));
    }

    private IEnumerator RoomTransitionRoutine(Room targetRoom, Transform targetSpawnPoint)
    {
        isTransitioning = true;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        yield return Fade(1f);

        if (currentRoom != null)
        {
            currentRoom.HideRoom();
        }

        currentRoom = targetRoom;
        currentRoom.ShowRoom();

        player.position = targetSpawnPoint.position;

        ApplyRoomSettings(currentRoom);

        yield return Fade(0f);

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        isTransitioning = false;
    }

    private void ApplyRoomSettings(Room room)
    {
        if (playerController != null)
        {
            playerController.minBounds = room.minBounds;
            playerController.maxBounds = room.maxBounds;
        }
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeImage.color.a;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                timer / fadeDuration
            );

            SetFadeAlpha(alpha);

            yield return null;
        }

        SetFadeAlpha(targetAlpha);
    }

    private void SetFadeAlpha(float alpha)
    {
        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
}