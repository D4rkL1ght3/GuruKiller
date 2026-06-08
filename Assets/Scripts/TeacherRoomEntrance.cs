using UnityEngine;

public class TeacherRoomEntrance : MonoBehaviour
{
    [Header("Rooms")]
    public Room hallwayRoom;
    public Room targetRoom;

    [Header("Teacher Spawn Points")]
    public Transform teacherInsideSpawnPoint;
    public Transform teacherOutsideSpawnPoint;

    [Header("Entry Rules")]
    public bool teacherCanEnter = true;

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