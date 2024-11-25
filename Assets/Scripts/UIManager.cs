using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image hungerBar;
    public float healthAmt = 100f;
    public float hungerAmt = 100f;
    public float totalHealth = 100f;
    public float totalHunger = 100f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDmg(float dmg)
    {
        healthAmt -= dmg;
        healthBar.fillAmount = healthAmt / totalHealth;
    }
    public void TakeHngr(float hngr)
    {
        hungerAmt -= hngr;
        hungerBar.fillAmount = hungerAmt / totalHunger;
    }
    public void Heal(float health)
    {
        healthAmt += health;
        healthAmt = Mathf.Clamp(healthAmt, 0, 100);

        healthBar.fillAmount = healthAmt / totalHealth;
    }
    public void Eat(float food)
    {
        hungerAmt += food;
        hungerAmt = Mathf.Clamp(hungerAmt, 0, 100);

        hungerBar.fillAmount = hungerAmt / totalHunger;
    }
}
