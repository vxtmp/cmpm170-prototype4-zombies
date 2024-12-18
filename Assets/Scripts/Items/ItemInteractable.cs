using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractable : MonoBehaviour
{
    public Item item;
    public SpriteRenderer spriteRenderer;

    public void SetUp(Item newItem)
    {
        item = newItem;
        spriteRenderer.sprite = item.iconSprite;
    }
}
