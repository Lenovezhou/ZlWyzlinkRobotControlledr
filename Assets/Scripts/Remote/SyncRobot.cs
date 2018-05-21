using HoloToolkit.Sharing.Spawning;
using HoloToolkit.Sharing.SyncModel;

[SyncDataClass]
public class SyncRobot : SyncSpawnedObject {
    [SyncData]
    public SyncString syncProgramLine;

    // syncCounter is used to sync the steps for IKMoveTargetArray
    //   -1: Invalid. No server exists
    //    0: Ready to start
    //    1-n: The current point the Console reached
    [SyncData]
    public SyncInteger syncCounter;

    [SyncData]
    public SyncBool isSystemRunning;
    [SyncData]
    public SyncBool isSystemDancing;
}
