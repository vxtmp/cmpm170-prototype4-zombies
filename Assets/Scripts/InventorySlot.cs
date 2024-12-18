using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image imgComponent;

    public Item item;

    public bool selected;
    private Button btn;
    private Color pressed;
    private string pressHex = "#C8C8C8";
    public int numkey;

    [SerializeField] private PlayerBehavior playerBehavior;

    void Start()
    {
        btn = GetComponent<Button>();
        imgComponent.enabled = false;
        ColorUtility.TryParseHtmlString(pressHex, out Color color);
        pressed = color;
    }

    void Update()
    {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                if (key >= KeyCode.Alpha0 && key <= KeyCode.Alpha9)
                {
                    if (Input.GetKeyDown(numkey.ToString()))
                    {
                        selected = true;

                    }
                    else
                    {
                        selected = false;
                    }
                }
                break; // Stop after the first keypress is handled
            }

        }
        if (selected == true)
        {
            ColorBlock colorBlock = btn.colors;
            colorBlock.normalColor = pressed;
            btn.colors = colorBlock;
            playerBehavior.curItem = item;
        }
        else
        {
            ColorBlock colorBlock = btn.colors;
            colorBlock.normalColor = Color.white;
            btn.colors = colorBlock;
        }
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

    public void TaskOnClick()
    {
        selected = !selected;
        if(selected == true)
        {
            ColorBlock colorBlock = btn.colors;
            colorBlock.normalColor = pressed;
            btn.colors = colorBlock;
            playerBehavior.curItem = item;
        }
        else
        {
            ColorBlock colorBlock = btn.colors;
            colorBlock.normalColor = Color.white;
            btn.colors = colorBlock;
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
