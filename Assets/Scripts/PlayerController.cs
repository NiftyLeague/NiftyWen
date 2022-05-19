using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public AudioManager audioManager;
    public Rigidbody2D rigidBody;
    public ProjectileHitter projectileHitter;
    public float jumpForce;
    private InputState input = new InputState();
    private bool isInTheAir;

    void FixedUpdate()
    {
        if (gameplayManager.hasGameEnded)
        {
            return;
        }

        InputReader.GetInput(input);

        if (input.aButton && transform.position.y < -7)
        {
            rigidBody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            isInTheAir = true;
        }

        if (isInTheAir && Mathf.Abs(rigidBody.velocity.y) < 0.001f)
        {
            audioManager.PlaySound(AudioManager.SoundID.playerLand);
            isInTheAir = false;
        }

        if (input.PressedX)
        {
            projectileHitter.TurnOn();
        }
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
