using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image imgComponent;

    public Item item;
    private Button btn;
    [SerializeField] private PlayerBehavior playerBehavior;

    void Start()
    {
        btn = GetComponent<Button>();
        imgComponent.enabled = false;
    }
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

    public void TaskShoot()
    {
        playerBehavior.curItem = item;
        if(item.name == "Gun")
        {
            playerBehavior.Shoot();
        }
    }

    /*public void UseItem()
    {
        if(item != null)
        {
            item.Use();
        }
    }*/
}
