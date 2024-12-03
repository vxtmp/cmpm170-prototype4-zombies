using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public int damage;
    public float bulletSpeed;
    public float lifetime = 3f;

    private Vector2 direction;

    public void SetDirection(Vector2 shootDirection)
    {
        direction = shootDirection.normalized; // Normalize to ensure consistent speed
    }

    private void Start()
    {
        Destroy(gameObject, lifetime); // Destroy the bullet after `lifetime` seconds
    }

    private void Update()
    {
        transform.Translate(direction * bulletSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }
}
