using System.Collections;
using UnityEngine;

public class TeacherRoomController : MonoBehaviour
{
    [Header("References")]
    public TeacherAI teacherAI;
    public TeacherVisibilityController visibilityController;
    public RoomTransitionManager roomTransitionManager;

    [Header("Starting Room")]
    public Room startingTeacherRoom;

    [Header("Room Entrances")]
    public TeacherRoomEntrance[] roomEntrances;

    [Header("Exit Settings")]
    public float exitDelayAfterQuiz = 1.5f;

    private Room currentTeacherRoom;
    private Room currentPlayerRoom;

    private TeacherRoomEntrance currentEntrance;
    private TeacherRoomEntrance targetEntrance;

    private bool isInsideClassroom;

    void Start()
    {
        if (teacherAI == null)
        {
            teacherAI = GetComponent<TeacherAI>();
        }

        if (visibilityController == null)
        {
            visibilityController = GetComponent<TeacherVisibilityController>();
        }

        if (roomTransitionManager == null)
        {
            roomTransitionManager = RoomTransitionManager.Instance;
        }

        currentTeacherRoom = startingTeacherRoom;

        if (roomTransitionManager != null)
        {
            currentPlayerRoom = roomTransitionManager.CurrentRoom;
            roomTransitionManager.OnRoomChanged += HandlePlayerRoomChanged;
        }

        ApplyRoomToTeacher(currentTeacherRoom);
        UpdateTeacherVisibility();
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
        currentPlayerRoom = newRoom;

        if (!IsTeacherInSameRoomAsPlayer() && !isInsideClassroom)
        {
            targetEntrance = FindEntranceForRoom(currentPlayerRoom);

            if (targetEntrance != null &&
                teacherAI != null &&
                teacherAI.IsChasing())
            {
                teacherAI.ForceChase();
            }
        }
        else
        {
            targetEntrance = null;
        }

        UpdateTeacherVisibility();
    }

    public bool IsTeacherInSameRoomAsPlayer()
    {
        return currentTeacherRoom != null &&
               currentPlayerRoom != null &&
               currentTeacherRoom == currentPlayerRoom;
    }

    public bool HasEntranceTarget()
    {
        return targetEntrance != null;
    }

    public Transform GetEntranceTarget()
    {
        if (targetEntrance == null)
            return null;

        return targetEntrance.GetDoorTarget();
    }

    public Room GetCurrentPlayerRoom()
    {
        return currentPlayerRoom;
    }

    public Room GetCurrentTeacherRoom()
    {
        return currentTeacherRoom;
    }

    public void TryEnterRoom(TeacherRoomEntrance entrance)
    {
        if (entrance == null)
            return;

        if (teacherAI != null && teacherAI.IsInQuiz())
            return;

        if (isInsideClassroom)
            return;

        if (currentPlayerRoom != entrance.targetRoom)
            return;

        EnterRoom(entrance);
    }

    private void EnterRoom(TeacherRoomEntrance entrance)
    {
        currentEntrance = entrance;
        targetEntrance = null;

        isInsideClassroom = true;
        currentTeacherRoom = entrance.targetRoom;

        if (entrance.teacherInsideSpawnPoint != null)
        {
            teacherAI.TeleportTo(entrance.teacherInsideSpawnPoint.position);
        }

        ApplyRoomToTeacher(currentTeacherRoom);
        UpdateTeacherVisibility();

        teacherAI.PatrolOnce(
            currentTeacherRoom.teacherPatrolPoints,
            BeginExitCurrentRoom
        );
    }

    private void BeginExitCurrentRoom()
    {
        if (!isInsideClassroom)
            return;

        if (currentEntrance == null)
            return;

        if (teacherAI != null && teacherAI.IsInQuiz())
            return;

        if (currentEntrance.teacherInsideExitPoint != null)
        {
            teacherAI.MoveToTarget(
                currentEntrance.teacherInsideExitPoint,
                teacherAI.patrolSpeed,
                CompleteExitCurrentRoom
            );
        }
        else
        {
            CompleteExitCurrentRoom();
        }
    }

    private void CompleteExitCurrentRoom()
    {
        if (!isInsideClassroom)
            return;

        if (currentEntrance == null)
            return;

        Room returnRoom = currentEntrance.hallwayRoom;

        if (currentEntrance.teacherOutsideSpawnPoint != null)
        {
            teacherAI.TeleportTo(
                currentEntrance.teacherOutsideSpawnPoint.position
            );
        }

        currentTeacherRoom = returnRoom;
        isInsideClassroom = false;

        ApplyRoomToTeacher(
            currentTeacherRoom,
            currentEntrance.hallwayReturnPatrolIndex
        );

        UpdateTeacherVisibility();

        teacherAI.ReturnToPatrol();

        currentEntrance = null;
    }

    public void ExitRoomAfterQuiz()
    {
        if (!isInsideClassroom)
            return;

        StartCoroutine(ExitRoomAfterQuizRoutine());
    }

    private IEnumerator ExitRoomAfterQuizRoutine()
    {
        yield return new WaitForSeconds(exitDelayAfterQuiz);

        BeginExitCurrentRoom();
    }

    private TeacherRoomEntrance FindEntranceForRoom(Room room)
    {
        if (room == null || roomEntrances == null)
            return null;

        foreach (TeacherRoomEntrance entrance in roomEntrances)
        {
            if (entrance == null)
                continue;

            if (entrance.targetRoom == room)
            {
                return entrance;
            }
        }

        return null;
    }

    private void ApplyRoomToTeacher(Room room)
    {
        if (teacherAI == null || room == null)
            return;

        teacherAI.SetPatrolPoints(room.teacherPatrolPoints);
    }

    private void ApplyRoomToTeacher(Room room, int startPatrolIndex)
    {
        if (teacherAI == null || room == null)
            return;

        teacherAI.SetPatrolPoints(
            room.teacherPatrolPoints,
            startPatrolIndex
        );
    }

    private void UpdateTeacherVisibility()
    {
        if (visibilityController == null)
            return;

        bool shouldBeVisible = IsTeacherInSameRoomAsPlayer();

        visibilityController.SetVisible(shouldBeVisible);
    }
}