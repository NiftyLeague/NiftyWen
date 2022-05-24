using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public PlayerSpriteManager playerSpriteManager;
    public AudioManager audioManager;
    [Space]
    public SpriteRenderer spriteRenderer;
    public Sprite[] playerSprites;
    //private InputState input = new InputState();

    //void FixedUpdate()
    //{
    //    if (gameplayManager.hasGameEnded)
    //    {
    //        return;
    //    }

    //    InputReader.GetInput(input);

    //    if (input.PressedY)
    //    {
    //        audioManager.PlaySound(AudioManager.SoundID.menuOptionSelect);
    //        playerSpriteManager.ChangeCharacter();
    //    }
    //}

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
