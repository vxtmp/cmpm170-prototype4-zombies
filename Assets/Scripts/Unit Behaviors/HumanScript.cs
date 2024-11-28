using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class HumanScript : MonoBehaviour
{

    // human inventory
    public List<Item> items = new List<Item>();
    [SerializeField] private GameObject ItemPrefab;
    public int health = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // zombie attack collision or bullet collisiom
        if (collision.gameObject.CompareTag("Attack"))
        {
            health -= collision.gameObject.GetComponent<BulletScript>().damage;
            if(health < 0)
            {
                DropItems();
            }
            
        }
    }

    public void DropItems()
    {
        foreach (Item item in items)
        {
            Debug.Log(item);
            Vector3 randomPos = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
            GameObject itemInstance = Instantiate(ItemPrefab, transform.position + randomPos, new Quaternion(0, 0, 0, 0));
            itemInstance.GetComponent<ItemInteractable>().SetUp(item);
        }
        Destroy(gameObject);
    }
}
