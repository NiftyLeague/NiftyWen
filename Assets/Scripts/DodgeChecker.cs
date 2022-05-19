using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DodgeChecker : MonoBehaviour
{
    public GameplayManager gameplayManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile"))
        {
            Projectile hitProjectile = collision.GetComponent<Projectile>();
            gameplayManager.Dodge(hitProjectile.isABomb);
        }
    }
}
