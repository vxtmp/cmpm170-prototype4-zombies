using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorBehavior : MonoBehaviour
{
    private int INIT_MAX_HP = 5;
    private int currentHealth;


    public int healthDelta = 0;
    public bool healthChanged = false;
    // Start is called before the first frame update
    void Start()
    {
        currentHealth = INIT_MAX_HP;
    }

    // Update is called once per frame
    void Update()
    {
        if (healthChanged)
        {
            currentHealth += healthDelta;
            healthDelta = 0;
            healthChanged = false;
            Debug.Log ("healthChanged: " + currentHealth);
        }
        if (currentHealth <= 0)
        {
            Debug.Log("door destroyed");
            Destroy(this.gameObject);
        }
    }

    public void takeDamage(int damageValue)
    {
        healthDelta -= damageValue;
        healthChanged = true;
    }
}
