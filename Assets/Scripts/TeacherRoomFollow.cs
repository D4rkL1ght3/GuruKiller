using System.Collections;
using UnityEngine;

public class TeacherRoomFollower : MonoBehaviour
{
    [Header("References")]
    public TeacherAI teacherAI;
    public RoomTransitionManager roomTransitionManager;

    [Header("Room Following")]
    public bool followPlayerBetweenRooms = true;
    public float followDelay = 1.5f;

    [Header("Behavior After Entering Room")]
    public bool chaseAfterFollowing = false;

    private Coroutine followRoutine;

    void Start()
    {
        if (teacherAI == null)
        {
            teacherAI = GetComponent<TeacherAI>();
        }

        if (roomTransitionManager == null)
        {
            roomTransitionManager = RoomTransitionManager.Instance;
        }

        if (roomTransitionManager != null)
        {
            roomTransitionManager.OnRoomChanged += HandleRoomChanged;
        }

        Room startingRoom = roomTransitionManager.CurrentRoom;

        if (startingRoom != null)
        {
            ApplyRoomData(startingRoom);
        }
    }

    void OnDestroy()
    {
        if (roomTransitionManager != null)
        {
            roomTransitionManager.OnRoomChanged -= HandleRoomChanged;
        }
    }

    private void HandleRoomChanged(Room previousRoom, Room newRoom)
    {
        if (!followPlayerBetweenRooms)
            return;

        if (newRoom == null)
            return;

        if (followRoutine != null)
        {
            StopCoroutine(followRoutine);
        }

        followRoutine = StartCoroutine(FollowPlayerIntoRoomRoutine(newRoom));
    }

    private IEnumerator FollowPlayerIntoRoomRoutine(Room targetRoom)
    {
        yield return new WaitForSeconds(followDelay);

        ApplyRoomData(targetRoom);

        if (targetRoom.teacherEntryPoint != null)
        {
            teacherAI.TeleportTo(targetRoom.teacherEntryPoint.position);
        }

        teacherAI.ReturnToPatrol();
    }

    private void ApplyRoomData(Room room)
    {
        if (teacherAI == null || room == null)
            return;

        teacherAI.SetPatrolPoints(room.teacherPatrolPoints);
    }
}