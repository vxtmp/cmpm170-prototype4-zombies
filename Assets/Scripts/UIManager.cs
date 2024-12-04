using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image hungerBar;
    public float healthAmt = 10f;
    public float hungerAmt = 10f;
    public float totalHealth = 10f;
    public float totalHunger = 10f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void healthChange(int change)
    {
        healthAmt += change;
        healthAmt = Mathf.Clamp(healthAmt, 0, 10);
        healthBar.fillAmount = healthAmt / totalHealth;
    }
    public void TakeHngr(float hngr)
    {
        hungerAmt += hngr;
        hungerAmt = Mathf.Clamp(hungerAmt, 0, 10);
        hungerBar.fillAmount = hungerAmt / totalHunger;
    }
}
