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

    [Header("Classroom Search")]
    public float classroomSearchDuration = 6f;
    public float exitDelayAfterQuiz = 1.5f;

    private Room currentTeacherRoom;
    private Room currentPlayerRoom;

    private TeacherRoomEntrance currentEntrance;

    private Coroutine searchRoutine;

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
        UpdateTeacherVisibility();
    }

    public bool IsTeacherInSameRoomAsPlayer()
    {
        return currentTeacherRoom != null &&
               currentPlayerRoom != null &&
               currentTeacherRoom == currentPlayerRoom;
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
        isInsideClassroom = true;

        currentTeacherRoom = entrance.targetRoom;

        if (entrance.teacherInsideSpawnPoint != null)
        {
            teacherAI.TeleportTo(entrance.teacherInsideSpawnPoint.position);
        }

        ApplyRoomToTeacher(currentTeacherRoom);
        UpdateTeacherVisibility();

        teacherAI.ReturnToPatrol();

        if (searchRoutine != null)
        {
            StopCoroutine(searchRoutine);
        }

        searchRoutine = StartCoroutine(ClassroomSearchRoutine());
    }

    private IEnumerator ClassroomSearchRoutine()
    {
        float timer = classroomSearchDuration;

        while (timer > 0f)
        {
            if (teacherAI != null && teacherAI.IsInQuiz())
            {
                yield break;
            }

            timer -= Time.deltaTime;
            yield return null;
        }

        ExitCurrentRoom();
    }

    public void ExitCurrentRoom()
    {
        if (!isInsideClassroom)
            return;

        if (currentEntrance == null)
            return;

        Room returnRoom = currentEntrance.hallwayRoom;

        if (currentEntrance.teacherOutsideSpawnPoint != null)
        {
            teacherAI.TeleportTo(currentEntrance.teacherOutsideSpawnPoint.position);
        }

        currentTeacherRoom = returnRoom;
        isInsideClassroom = false;

        ApplyRoomToTeacher(currentTeacherRoom);
        UpdateTeacherVisibility();

        teacherAI.ReturnToPatrol();

        currentEntrance = null;
    }

    public void ExitRoomAfterDelay()
    {
        StartCoroutine(ExitRoomAfterDelayRoutine());
    }

    private IEnumerator ExitRoomAfterDelayRoutine()
    {
        yield return new WaitForSeconds(exitDelayAfterQuiz);

        ExitCurrentRoom();
    }

    private void ApplyRoomToTeacher(Room room)
    {
        if (teacherAI == null || room == null)
            return;

        teacherAI.SetPatrolPoints(room.teacherPatrolPoints);
    }

    private void UpdateTeacherVisibility()
    {
        if (visibilityController == null)
            return;

        bool shouldBeVisible = IsTeacherInSameRoomAsPlayer();

        visibilityController.SetVisible(shouldBeVisible);
    }
}