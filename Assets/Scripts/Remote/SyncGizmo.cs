using HoloToolkit.Sharing.Spawning;
using HoloToolkit.Sharing.SyncModel;

[SyncDataClass]
public class SyncGizmo : SyncSpawnedObject {
    [SyncData]
    public SyncInteger index;
    [SyncData]
    public SyncInteger postAction;
    [SyncData]
    public SyncFloat duration;
}
