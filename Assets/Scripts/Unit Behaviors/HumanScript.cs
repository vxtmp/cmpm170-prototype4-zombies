using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanScript : MonoBehaviour
{

    // human inventory
    public List<GameObject> item = new List<GameObject>();
    public int health = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(health <= 0)
        {
            // drop item
            Destroy(this);
        }
    }
}
