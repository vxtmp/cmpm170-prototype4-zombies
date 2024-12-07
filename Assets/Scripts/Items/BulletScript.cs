using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public int damage;
    public float bulletSpeed;
    public float lifetime = 3f;
    public Vector3 targetPos;
    private float stopPoint = 0.1f;

    private Vector2 direction;

    public Rigidbody2D rb;

    public void SetDirection(Vector2 shootDirection)
    {
        direction = shootDirection.normalized; // Normalize to ensure consistent speed
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, lifetime); // Destroy the bullet after `lifetime` seconds
    }

    private void Update()
    {
        //transform.Translate(direction * bulletSpeed * Time.deltaTime);
        if (Vector2.Distance(transform.position, targetPos) <= stopPoint)
        {
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Zombie")
        {

            Destroy(gameObject);
        }
    }
}
