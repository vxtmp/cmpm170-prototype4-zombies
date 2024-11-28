using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemInteractable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void PickUp()
    {
        Destroy(gameObject);
    }
}
