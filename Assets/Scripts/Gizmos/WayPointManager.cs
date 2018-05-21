using HoloToolkit.Sharing;
using HoloToolkit.Sharing.Spawning;
using SpectatorView;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WayPointManager : Singleton<WayPointManager>
{
    private PrefabSpawnManager spawnManager;
    private Material lineRendererMaterial;
    private Vector3 Attachto;
    //private Transform parent;
    private ParticleSystem inputburst_ps;

    private void Start()
    {
        inputburst_ps = GameObject.Find("inputburst_ps").GetComponent<ParticleSystem>();
        lineRendererMaterial = GlobalConfiguration.Instance == null ? null : GlobalConfiguration.Instance.LineRendererMaterial;
    }

    public IEnumerable<PositionRotation> wayPoints()
    {
        var list = from wayPoint in this.transform.Cast<Transform>()
                   where wayPoint.GetComponent<GizmoController>().IsInSafeRange()
                   orderby wayPoint.GetComponent<GizmoController>().syncIndex
                   select new PositionRotation(wayPoint.position, -wayPoint.right,
                        wayPoint.GetComponent<GizmoController>()
                        );
        return list;
    }

    //实例出单个waypoint
    public void CreateWayPoint(Vector3 startPosition, Transform trans = null)
    {
        var _targetpos = new Vector3(-0.5f + this.transform.childCount * -0.03f, 0.5f, this.transform.childCount * 0.1f - 0.2f);
        var targetPosition = Vector3.zero;
        if (trans)
        {
            targetPosition = trans.TransformPoint(_targetpos);
        }
        else
        {
            targetPosition = _targetpos;
        }
        var syncGizmo = new SyncGizmo();
        syncGizmo.index.Value = this.transform.childCount + 1;
        spawnManager.Spawn(syncGizmo, startPosition, Quaternion.identity, this.gameObject, "Gizmo", false);

        syncGizmo.GameObject.transform.forward = trans.forward;

        Run.Lerp(0.5f, (t) => syncGizmo.GameObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t));
    }

    /// 点击按钮实例出已经保存的路径点

    public void CreateWayPoint(Vector3 startPosition, Vector3 endpos, GizmoController.GizmoPostAction gizmoPostAction, float duration, float speed, Vector3 forward, bool isdancing = false)
    {
        var _targetpos = endpos;
        var targetPosition = Vector3.zero;

        targetPosition = _targetpos;
        var syncGizmo = new SyncGizmo();
        syncGizmo.index.Value = this.transform.childCount + 1;
        syncGizmo.duration.Value = duration;
        spawnManager.Spawn(syncGizmo, startPosition, Quaternion.identity, this.gameObject, "Gizmo", false);
        if (isdancing)
        {
            syncGizmo.GameObject.transform.localEulerAngles = forward;
        }
        else
        {
            syncGizmo.GameObject.transform.forward = forward;
        }

        GizmoController gc = syncGizmo.GameObject.GetComponent<GizmoController>();

        Run.After(1, () => gc.SetPostAction(gizmoPostAction, speed, isdancing));
        Run.Lerp(0.5f,
            (t) =>
            {
                if (syncGizmo.GameObject)
                {
                    syncGizmo.GameObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                }
            });

    }

    /// <summary>
    /// 删除单个pointer
    /// </summary>
    /// <param name="wayPoint"></param>
    /// <returns></returns>
    public bool RemoveWayPoint(GizmoController wayPoint)
    {
        var currentIndex = wayPoint.syncIndex;

        //Destroy(wayPoint.gameObject);
        PlayerController.Instance.CmdDestroySync(wayPoint.gameObject);
        // Update the gizmos to reduce the count
        foreach (Transform wp in this.transform)
        {
            var gizmo = wp.GetComponent<GizmoController>();
            if (gizmo.syncIndex > currentIndex)
            {
                gizmo.UpdateOrderText(gizmo.syncIndex - 1);
            }
        }
        return true;
    }


    public System.Collections.IEnumerator DeletSyncRobot(DefaultSyncModelAccessor accessor)
    {
        yield return (WayPointManager.Instance.RemoveAllWayPoint());
        if (accessor == null || spawnManager == null)
        {
            Debug.Log("accessor or spawnManager is null");
        }
        spawnManager.Delete((SyncSpawnedObject)accessor.SyncModel);

    }

    /// <summary>
    /// 删除所有pointer点
    /// </summary>
    /// <returns></returns>
    public System.Collections.IEnumerator RemoveAllWayPoint()
    {
        while (this.transform.childCount > 0)
        {
            yield return RemoveWayPoint(this.transform.GetChild(0).GetComponent<GizmoController>());
        }
        //yield return new WaitForSeconds(3);
    }

    public bool MoveForwardInOrder(GizmoController wayPoint)
    {
        if (wayPoint.Index > 1)
        {
            var wayPointOther = GetWayPoint(wayPoint.Index - 1);
            if (wayPointOther != null)
            {
                wayPointOther.UpdateOrderText(wayPoint.Index);
                wayPoint.UpdateOrderText(wayPoint.Index - 1);
                return true;
            }
        }
        return false;
    }

    public bool MoveBackwardInOrder(GizmoController wayPoint)
    {
        if (wayPoint.Index < this.transform.childCount)
        {
            var wayPointOther = GetWayPoint(wayPoint.Index + 1);
            if (wayPointOther != null)
            {
                wayPointOther.UpdateOrderText(wayPoint.Index);
                wayPoint.UpdateOrderText(wayPoint.Index + 1);
                return true;
            }
        }
        return false;
    }

    /*根据运行到第几个点刷新所有点，只留接下来的三个*/
    internal void RefreshIfDancing(int index, bool alldone)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).GetComponent<GizmoController>().RefreshSelfRenderer(index, alldone, PlayParticleEffect);
        }
    }

    /*到达后播放粒子特效*/
    private void PlayParticleEffect(Vector3 position)
    {
        inputburst_ps.transform.position = position;
        inputburst_ps.Play();
    }

    private GizmoController GetWayPoint(int index)
    {
        foreach (Transform child in this.transform)
        {
            var wayPoint = child.GetComponent<GizmoController>();
            if (wayPoint.Index == index)
            {
                return wayPoint;
            }
        }
        return null;
    }

    internal void RenderPath(RobotControllerSimple robot)

    {

        StartCoroutine(_PathRendering(robot));

    }

    private IEnumerator _PathRendering(RobotControllerSimple robot)
    {
        float changeThreshold = 0.005f;
        List<RotationPosition> wayPointsPositionStorage = new List<RotationPosition>() { robot.GetEndEffectorRotationPosition() };
        List<PathDrawer> pathList = new List<PathDrawer>();

        while (robot != null && this != null)
        {
            if (!robot.IsRunning)
            {
                bool isDirty = false;
                if (Vector3.Distance(robot.GetEndEffectorRotationPosition().Position, wayPointsPositionStorage[0].Position) > changeThreshold)
                {
                    wayPointsPositionStorage[0] = robot.GetEndEffectorRotationPosition();
                    isDirty = true;
                }
                int i = 1;
                foreach (var wayPoint in this.wayPoints())
                {
                    if (i == wayPointsPositionStorage.Count)
                    {
                        // Insert one point here and update the line
                        wayPointsPositionStorage.Add(new RotationPosition(wayPoint.Rotation, wayPoint.Position));
                        pathList.Add(PathDrawer.Create(this.lineRendererMaterial, robot.GetPath(wayPointsPositionStorage[i - 1], wayPointsPositionStorage[i])));
                        isDirty = false;
                    }
                    else
                    {
                        Debug.Assert(i < wayPointsPositionStorage.Count);
                        isDirty = isDirty || Vector3.Distance(wayPointsPositionStorage[i].Position, wayPoint.Position) > changeThreshold;
                        if (isDirty)
                        {
                            wayPointsPositionStorage[i] = new RotationPosition(wayPoint.Rotation, wayPoint.Position);
                            // Update the path
                            pathList[i - 1].UpdateData(robot.GetPath(wayPointsPositionStorage[i - 1], wayPointsPositionStorage[i]));
                        }
                    }
                    i++;
                }
                while (i < wayPointsPositionStorage.Count)
                {
                    wayPointsPositionStorage.RemoveAt(i);
                    // Update the path (remove)
                    pathList[i - 1].CloseUp();
                    pathList.RemoveAt(i - 1);
                }
            }
            yield return new WaitForSeconds(1.0f);
        }

    }
}

