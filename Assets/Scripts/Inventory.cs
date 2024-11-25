using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    void Awake ()
    {
        instance = this;
    }


    public delegate void OnItemChanged();
    public OnItemChanged onItemChangedCallback;


    public List<GameObject> items = new List<GameObject>();
    private int maxSpace = 6;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool Add(GameObject item)
    {
        if (items.Count < maxSpace)
        { 
            items.Add(item);
            onItemChangedCallback.Invoke();
            return true;
        } else
        {
            return false;
        }
    }

    public void Remove(GameObject item)
    {
        items.Remove(item);
        onItemChangedCallback.Invoke();
    }

}
