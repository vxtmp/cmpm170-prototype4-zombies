using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanBulletBehavior : MonoBehaviour
{
    [SerializeField] private float bulletDamage = 1.0f;

    private float bulletTimer = 0.0f;
    private const float BULLET_DURATION = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        bulletTimer = BULLET_DURATION;
    }

    // Update is called once per frame
    void Update()
    {
        // set own rotation parallel to velocity vector
        Vector2 direction = this.GetComponent<Rigidbody2D>().velocity;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        this.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        bulletTimer -= Time.deltaTime;
        if (bulletTimer <= 0)
        {
            Destroy(this.gameObject);
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            collision.gameObject.GetComponent<PlayerBehavior>().changeHealth(-bulletDamage);
            collision.gameObject.GetComponent<PlayerBehavior>().healthChanged = true;
            Destroy(this.gameObject);
        }
        if (collision.gameObject.tag == "Zombie")
        {
            collision.gameObject.GetComponent<ZombieScript>().takeDamage(bulletDamage);
            Destroy(this.gameObject);
        }
    }

}
