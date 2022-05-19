using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public Rigidbody2D rigidBody;
    public float speed;
    private float moveInput;

    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile"))
        {
            Projectile hitProjectile = collision.GetComponent<Projectile>();
            if (hitProjectile.gotHit)
            {
                return;
            }
            hitProjectile.HitProjectile();
            gameplayManager.Lose();
        }
    }
}
