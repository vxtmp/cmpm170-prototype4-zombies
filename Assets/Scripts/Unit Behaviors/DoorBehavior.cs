using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehavior : MonoBehaviour
{
    [SerializeField] private int INIT_MAX_HP = 1; // set in inspector. 1 is default.
    private int currentHealth;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = INIT_MAX_HP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
