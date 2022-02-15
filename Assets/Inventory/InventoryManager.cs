using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class InventoryManager : MonoSingleton<InventoryManager>
{
    public InventoryInfoPanel InfoPanel;

    public GameObject Container;

    [Tooltip(tooltip:"Loads the list using this format.")]
    [Multiline]
    public string ItemJson;

    [Tooltip(tooltip:"This is used in generating the items list. The number of additional copies to concat the list parsed from ItemJson.")]
    public int ItemGenerateScale = 10;

    [Tooltip(tooltip:"Icons referenced by ItemData.IconIndex when instantiating new items.")]
    public Sprite[] Icons;

    public SpriteAtlas IconAtlas;

    [Serializable]
    private class InventoryItemDatas
    {
        public InventoryItemData[] ItemDatas;
    }

    private InventoryItemData[] ItemDatas;

    //private List<InventoryItem> Items;


    public LoopListView mLoopListView;

    void Start()
    {
        // Clear existing items already in the list.
        //var items = Container.GetComponentsInChildren<InventoryItem>();
        //foreach (InventoryItem item in items) {
        //    item.gameObject.transform.SetParent(null);
        //    Destroy(item.gameObject);
        //}

        ItemDatas = GenerateItemDatas(ItemJson, ItemGenerateScale);

        // Instantiate items in the Scroll View.
        //Items = new List<InventoryItem>();
        //foreach (InventoryItemData itemData in ItemDatas) {
        //    var newItem = GameObject.Instantiate<InventoryItem>(InventoryItemPrefab);
        //    newItem.Icon.sprite = Icons[itemData.IconIndex];
        //    newItem.Name.text = itemData.Name;
        //    newItem.transform.SetParent(Container.transform);
        //    newItem.Button.onClick.AddListener(() => { InventoryItemOnClick(newItem, itemData); });
        //    Items.Add(newItem);       
        //}

        //// Select the first item.
        //InventoryItemOnClick(Items[0], ItemDatas[0]);

        mLoopListView.InitListView(ItemDatas.Length, OnGetItemByIndex);
        mLoopListView.CurrentSelectIndex = 0;

    }

    LoopListViewItem OnGetItemByIndex(LoopListView listView, int index)
    {
        if (index < 0 || index >= ItemDatas.Length)
        {
            return null;
        }

        InventoryItemData itemData = ItemDatas[index];
        if (itemData == null)
        {
            return null;
        }
        //get a new item. Every item can use a different prefab, the parameter of the NewListViewItem is the prefab��name. 
        //And all the prefabs should be listed in ItemPrefabList in LoopListView Inspector Setting
        LoopListViewItem item = listView.NewListViewItem();
        InventoryItem itemScript = item.GetComponent<InventoryItem>();

        itemScript.SetItemData(itemData, index);

        return item;
    }

    /// <summary>
    /// Generates an item list.
    /// </summary>
    /// <param name="json">JSON to generate items from. JSON must be an array of InventoryItemData.</param>
    /// <param name="scale">Concats additional copies of the array parsed from json.</param>
    /// <returns>An array of InventoryItemData</returns>
    private InventoryItemData[] GenerateItemDatas(string json, int scale) 
    {
        var itemDatas = JsonUtility.FromJson<InventoryItemDatas>(json).ItemDatas;
        var finalItemDatas = new InventoryItemData[itemDatas.Length * scale];
        for (var i = 0; i < itemDatas.Length; i++) {
            for (var j = 0; j < scale; j++) {
                finalItemDatas[i + j*itemDatas.Length] = itemDatas[i];
                finalItemDatas[i + j * itemDatas.Length].IconName = Icons[itemDatas[i].IconIndex].name;
            }
        }

        return finalItemDatas;
    }


}
