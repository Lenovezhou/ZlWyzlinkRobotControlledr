using HoloToolkit.Sharing;
using HoloToolkit.Sharing.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemManager : Singleton<ItemManager>
{
    private PrefabSpawnManager spawnManager;
    public enum ItemType
    {
        target = 0,
        bottle,
        table,
    };
    private Vector3 optimizedGenerationPoint = new Vector3(-0.5f, 0.5f, 0.3f);

    private void Start()
    {
    }

    public IEnumerable<Transform> GetItems()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            yield return this.transform.GetChild(i);
        }
    }

    public Transform CreateItem(ItemType type)
    {
        return CreateItem(type, UnityEngine.Random.insideUnitSphere * 0.05f + optimizedGenerationPoint);
    }

    public Transform CreateItem(ItemType type, Vector3 position)
    {
        var syncedItem = CreateSyncedItem(type);
        spawnManager.Spawn(syncedItem,
            position,
            Quaternion.identity, this.gameObject, "Item", false);
        return syncedItem.GameObject.transform;
    }

    private SyncSpawnedObject CreateSyncedItem(ItemType type)
    {
        switch (type)
        {
            case ItemType.target:
                return new SyncItem0();
            case ItemType.bottle:
                return new SyncItem1();
            case ItemType.table:
                return new SyncItem2();
        }
        return null;
    }

    public bool RemoveItem(Transform item)
    {
        var accessor = item.GetComponent<DefaultSyncModelAccessor>();
        Debug.Assert(accessor != null);
        if (accessor == null || spawnManager == null)
        {
            Debug.Log("accessor or spawnManager is null");
            return false;
        }
        spawnManager.Delete((SyncSpawnedObject)accessor.SyncModel);
        return true;
    }
}
