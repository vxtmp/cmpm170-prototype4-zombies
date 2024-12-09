using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockScript : MonoBehaviour
{
    public int damage;
    public float rockSpeed;
    //public float lifetime = 3f;
    private bool hitWall = false;

    private Vector2 direction;
    public Vector3 targetPos;
    private float stopPoint = 0.1f;
    public Rigidbody2D rb;
    public Item item;

    public GridManager grid;

    public void SetDirection(Vector2 shootDirection)
    {
        direction = shootDirection.normalized; // Normalize to ensure consistent speed
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GetComponent<ItemInteractable>().SetUp(item);
        //Destroy(gameObject, lifetime); // Destroy the bullet after `lifetime` seconds
    }

    private void Update()
    {
        if (!hitWall)
        {
            if (Vector2.Distance(transform.position, targetPos) <= stopPoint)
            {
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    GetComponent<CircleCollider2D>().isTrigger = false;
                    grid.recalcPathing(transform.position, 100);
                }

            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Wall")
        {
            hitWall = true; // Stop movement
            rb.velocity = Vector2.zero; // Stop physics-based movement, if any
            rb.isKinematic = true; // Disable physics interactions
        }
    }
}
