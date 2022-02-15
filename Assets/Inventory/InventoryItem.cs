using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class InventoryItem : LoopListViewItem
{
    public Image Background;
    public Image Icon;
    public TextMeshProUGUI Name;
    public Button Button;
    private InventoryItemData data;

    internal void SetItemData(InventoryItemData itemData, int index)
    {
        if(index == ParentListView.CurrentSelectIndex)
        {
            Background.color = Color.red;
            InventoryManager.Instance.InfoPanel.SetData(itemData);
        }
        else
        {
            Background.color = Color.white;
        }

        data = itemData;
        ItemIndex = index;

        Icon.sprite = InventoryManager.Instance.IconAtlas.GetSprite(itemData.IconName);
        Name.text = itemData.Name;
    }

    internal void InventoryItemOnClick()
    {
        ParentListView.CurrentSelectIndex = ItemIndex;
        for(int i = 0; i < ParentListView.ItemList.Count; i++)
        {
            var item = ParentListView.ItemList[i] as InventoryItem;

            if(item.ItemIndex == ParentListView.CurrentSelectIndex)
            {
                Background.color = Color.red;
                InventoryManager.Instance.InfoPanel.SetData(data);
            }
            else
            {
                item.Background.color = Color.white;
            }
        }
        

    }

    private void Reset()
    {
        Background.color = Color.white;
    }
}
