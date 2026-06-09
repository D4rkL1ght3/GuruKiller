using UnityEngine;

public class TeacherRoomEntrance : MonoBehaviour
{
    [Header("Rooms")]
    public Room hallwayRoom;
    public Room targetRoom;

    [Header("Teacher Door Target")]
    public Transform teacherDoorTargetPoint;

    [Header("Teacher Spawn Points")]
    public Transform teacherInsideSpawnPoint;
    public Transform teacherInsideExitPoint;
    public Transform teacherOutsideSpawnPoint;

    [Header("Return Patrol")]
    [Tooltip("After leaving this room, which hallway patrol point should the teacher continue toward?")]
    public int hallwayReturnPatrolIndex = 0;

    [Header("Entry Rules")]
    public bool teacherCanEnter = true;

    public Transform GetDoorTarget()
    {
        if (teacherDoorTargetPoint != null)
        {
            return teacherDoorTargetPoint;
        }

        return transform;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!teacherCanEnter)
            return;

        TeacherRoomController teacherRoomController =
            other.GetComponent<TeacherRoomController>();

        if (teacherRoomController == null)
            return;

        teacherRoomController.TryEnterRoom(this);
    }
}