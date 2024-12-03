using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image imgComponent;

    public Item item;

    public void AddItem(Item newItem)
    {
        item = newItem;
        imgComponent.sprite = item.iconSprite;
        imgComponent.enabled = true;
    }

    public void ClearSlot()
    {
        item = null;
        imgComponent.sprite = null;
        imgComponent.enabled = false;
    }

    /*public void UseItem()
    {
        if(item != null)
        {
            item.Use();
        }
    }*/
}
