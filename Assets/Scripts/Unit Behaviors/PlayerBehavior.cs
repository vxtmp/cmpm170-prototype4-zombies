using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    private GameObject objectToMove;
    public Item curItem;
    [SerializeField] private GameObject ItemPrefab;

    public float health;
    [SerializeField] private float maxHealth;
    public float hunger;
    [SerializeField] private float maxHunger;
    [SerializeField] private float hungerDecay;
    [SerializeField] private float healthDecay;
    private bool hurt = false;

    public const float PLAYER_SPEED = 5.0f;

    public float GLOBAL_AGGRO_INTERVAL = 5.0f;
    private float global_aggro_timer = 0.0f;

    public int healthDelta = 0;
    public bool healthChanged = false;
    public bool dead = false;

    public UIManager UIMan;
    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        hunger = maxHunger;
        StartCoroutine(HungerLower());
    }

    // Update is called once per frame
    void Update()
    {
        if (global_aggro_timer < GLOBAL_AGGRO_INTERVAL)
        {
            global_aggro_timer += Time.deltaTime;
        }
        else
        {
            global_aggro_timer = 0.0f;
            GridManager.Instance.recalcPathing(this.transform.position, 50.0f);
        }
        // move
        parseWASD();

        // use/throw item
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(curItem.name);
            if(curItem)
            {
                if (curItem.consumable)
                {
                    changeHealth(curItem.hungerSatisfaction);
                    changeHunger(curItem.hungerSatisfaction);
                    curItem.RemoveFromInventory();
                }
                else if (curItem.weapon)
                {
                    Shoot();
                }
                else
                {
                    Throw();
                }
            }
        }
    }

    public void Shoot()
    {
        if (curItem.health > 0)
        {
            GameObject bullet = Instantiate(curItem.bullet, transform.position, Quaternion.identity);
            BulletScript script = bullet.GetComponent<BulletScript>();
            bullet.GetComponent<Rigidbody2D>().velocity = transform.up * script.bulletSpeed;
            script.damage = curItem.damage;
            curItem.health--;
        }
        
        if(curItem.health <= 0)
        {
            curItem.RemoveFromInventory();
        }
    }
    private void Throw()
    {
        if (curItem.health > 0)
        {
            GameObject rock = Instantiate(curItem.bullet, transform.position, Quaternion.identity);
            RockScript script = rock.GetComponent<RockScript>();
            rock.GetComponent<Rigidbody2D>().velocity = transform.up * script.rockSpeed;
            script.damage = curItem.damage;
            curItem.health--;
        }

        if (curItem.health <= 0)
        {
            curItem.RemoveFromInventory();
        }
    }

    public void parseWASD()
    {
        float upDirection = 0.0f;
        float rightDirection = 0.0f;

        if (Input.GetKey(KeyCode.W)) upDirection++;
        if (Input.GetKey(KeyCode.S)) upDirection--;
        if (Input.GetKey(KeyCode.A)) rightDirection--;
        if (Input.GetKey(KeyCode.D)) rightDirection++;

        Vector3 direction = new Vector3(rightDirection, upDirection, 0.0f);
        direction.Normalize();
        moveSelf(direction);
    }
    private void moveSelf(Vector3 direction)
    {
        this.transform.position += direction * Time.deltaTime * PLAYER_SPEED;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Item"))
        {
            Debug.Log("pick up item");
            if (Inventory.instance.Add(collision.gameObject.GetComponent<ItemInteractable>().item))
            {
                Destroy(collision.gameObject);
            }
        }
    }

    public void changeHealth(float value)
    {
        Debug.Log("hurt " + hurt + " " +value + " " + health);
        if(value < 0 && !hurt)
        {
            UIMan.healthChange(value);
            health += value;
            StartCoroutine(Immune());
        } else if (value > 0)
        {
            UIMan.healthChange(value);
            health += value;
        }
        Debug.Log("health after " + health);
        if (health < 0)
        {
            dead = true;
            Destroy(gameObject);
        }
    }

    public void changeHunger(float value)
    {
        UIMan.hungerChange(value);
        //Debug.Log("player takeDmg:" + value);
        hunger += value;
    }

    IEnumerator Immune()
    {
        Debug.Log("immune");
        hurt = true;
        yield return new WaitForSeconds(1f);
        hurt = false;
    }
    IEnumerator HungerLower()
    {
        yield return new WaitForSeconds(hungerDecay);
        changeHunger(-0.5f);
        if (hunger <= 0)
        {
            StartCoroutine(HealthLower());
        }
        else
        {
            StartCoroutine(HungerLower());
        }

    }

    IEnumerator HealthLower()
    {
        yield return new WaitForSeconds(healthDecay);
        changeHealth(-0.5f);
        if (health <= 0)
        {
            StartCoroutine(HealthLower());
        }
        else
        {
            StartCoroutine(HungerLower());
        }
    }
}
