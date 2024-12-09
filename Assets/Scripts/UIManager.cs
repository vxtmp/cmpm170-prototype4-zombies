using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private Image healthBar;
    [SerializeField] private Image hungerBar;
    public float healthAmt = 10f;
    public float hungerAmt = 10f;
    public float totalHealth = 10f;
    public float totalHunger = 10f;
    [SerializeField] private TMP_Text gameoverText;

    private void Start()
    {
    }
    public void healthChange(float change)
    {
        healthAmt += change;
        healthAmt = Mathf.Clamp(healthAmt, 0, 10);
        healthBar.fillAmount = healthAmt / totalHealth;
    }
    public void hungerChange(float hngr)
    {
        hungerAmt += hngr;
        hungerAmt = Mathf.Clamp(hungerAmt, 0, 10);
        hungerBar.fillAmount = hungerAmt / totalHunger;
    }

    public void gameOver()
    {
        gameoverText.enabled = true;
    }
}
