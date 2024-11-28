using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Item : ScriptableObject
{
    new public string name = "Item";
    public Sprite icon;

    // consumable items
    public bool consumable = true;
    public int hungerSatisfaction = 5;

    // weapon/throwable
    public bool weapon = false;
    public GameObject bullet;
    public int damage = 0;
    public int health = 2;      // usage amount for weapons, health for thrown object

    public void RemoveFromInventory()
    {
        Inventory.instance.Remove(this);
    }
}
