using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool isABomb;
    public Rigidbody2D rigidBody;
    public float moveSpeed;
    float arc = 25;
    public bool gotHit;
    public bool hitScoreCollider;

    private void Start()
    {
        
        Destroy(gameObject, 10);
    }

    void Update()
    {
        if (gotHit)
        {
            arc = Mathf.Lerp(arc, 0, 0.002f);
            rigidBody.velocity = new Vector2(moveSpeed * 2, arc);
        }
        else
        {
            rigidBody.velocity = new Vector2(-moveSpeed, 0);
        }
        
    }

    public void SetNewSpeed(float amount)
    {
        moveSpeed += amount;
    }

    public void HitProjectile(float chargePower)
    {
        if (isABomb)
        {
            Destroy(gameObject);
        }
        else
        {
            gotHit = true;
            moveSpeed *= (1 + chargePower);
        }
    }
}
