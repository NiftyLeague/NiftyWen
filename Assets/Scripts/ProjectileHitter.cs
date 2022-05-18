using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileHitter : MonoBehaviour
{
    public GameplayManager gameplayManager;
    float turnOffTimer;

    void Update()
    {
        turnOffTimer += Time.deltaTime;
        if (turnOffTimer >= 0.2f)
        {
            turnOffTimer = 0;
            gameObject.SetActive(false);
        }
    }

    public void TurnOn()
    {
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile"))
        {
            Projectile hitProjectile = collision.GetComponent<Projectile>();
            hitProjectile.HitProjectile();
            if (hitProjectile.isABomb)
            {
                gameplayManager.Lose();
            }
            else
            {
                gameplayManager.ScorePoint();
            }
        }
    }
}
