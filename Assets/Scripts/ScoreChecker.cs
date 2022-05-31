using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreChecker : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public bool isBallMachine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile"))
        {
            Projectile hitProjectile = collision.GetComponent<Projectile>();
            if (hitProjectile.hitScoreCollider)
            {
                return;
            }
            if (isBallMachine)
            {
                gameplayManager.HitBallMachine();
            }
            gameplayManager.ScorePoint(isBallMachine);
            hitProjectile.hitScoreCollider = true;
        }
    }
}
