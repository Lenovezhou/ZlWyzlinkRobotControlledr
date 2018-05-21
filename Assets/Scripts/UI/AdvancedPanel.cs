using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedPanel : MonoBehaviour
{
    public ItemPanel itemPanelTemplate;
    public Transform ItemList;

    public Button AddItem0;
    public Button AddItem1;
    public Button AddItem2;

    private int ITEM_MASK = 0xC7;
    private int WORLD_MASK = 0x57;

    private void Start()
    {
        Debug.Assert(ItemList != null);
        Debug.Assert(itemPanelTemplate != null);
        AddItem0.onClick.AddListener(() => CreateItem(ItemManager.ItemType.target));
        AddItem1.onClick.AddListener(() => CreateItem(ItemManager.ItemType.bottle));
        AddItem2.onClick.AddListener(() => CreateItem(ItemManager.ItemType.target));
    }

    private void OnEnable()
    {
        StartCoroutine(LoadPanelCoroutine());
    }

    private IEnumerator LoadPanelCoroutine()
    {
        ClearList(ItemList);
        yield return -1;

        // Add world item
        if (WorldPositionTracker.Instance != null)
        {
            AddItem(WorldPositionTracker.Instance.transform, WORLD_MASK, "World");
            yield return -1;
        }

        foreach (var item in ItemManager.Instance.GetItems())
        {
            AddItem(item, ITEM_MASK, "Placer");        // Enable the positoin x-y-z and two buttons
            yield return -1;
        }
    }

    private void AddItem(Transform itemTransform, int enableMask, string caption)
    {
        var item = Instantiate<ItemPanel>(itemPanelTemplate, this.ItemList);
        item.gameObject.SetActive(true);
        item.InitItem(itemTransform, enableMask, caption);
    }

    private void ClearList(Transform itemList)
    {
        foreach (Transform item in itemList)
        {
            Destroy(item.gameObject);
        }
    }

    public void CreateItem(ItemManager.ItemType item)
    {
        var itemTransform = ItemManager.Instance.CreateItem(item);
        AddItem(itemTransform, ITEM_MASK, "Placer");
    }
}
