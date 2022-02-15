using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryInfoPanel : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI Stat;
    public void SetData(InventoryItemData itemData)
    {
        if (itemData == null)
            return;

        Icon.sprite = InventoryManager.Instance.IconAtlas.GetSprite(itemData.IconName);
        Name.text = itemData.Name;
        Description.text = itemData.Description;
        Stat.text = itemData.Stat.ToString();
    }
}
