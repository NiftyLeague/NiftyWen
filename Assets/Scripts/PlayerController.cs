using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public PlayerSpriteManager playerSpriteManager;
    public AudioManager audioManager;
    [Space]
    public Transform playerTransform;
    public RectTransform wenTextTransform;
    public TextMeshProUGUI wenText;
    [Space]
    private InputState input = new InputState();

    private void Start()
    {
        playerSpriteManager.SetCharacterSprites();
    }

    void FixedUpdate()
    {
        if (gameplayManager.hasGameEnded)
        {
            return;
        }

        InputReader.GetInput(input);

        if (input.PressedY)
        {
            if (!playerSpriteManager.CanChangeCharacters())
            {
                return;
            }
            audioManager.PlaySound(AudioManager.SoundID.menuOptionSelect);
            playerSpriteManager.ChangeCharacter();
        }

        if (playerTransform.position.x > 0)
        {
            wenTextTransform.anchoredPosition = new Vector2(-19.5f, 10);
            wenText.alignment = TextAlignmentOptions.TopRight;
        }
        else
        {
            wenTextTransform.anchoredPosition = new Vector2(19.5f, 10);
            wenText.alignment = TextAlignmentOptions.TopLeft;
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
            hitProjectile.hitScoreCollider = true;
            //hitProjectile.HitProjectile(0);
            if (hitProjectile.isABomb)
            {
                Destroy(hitProjectile.gameObject);
                gameplayManager.Explosion(collision.transform.position);
            }
            gameplayManager.Lose();
        }

        if (collision.CompareTag("LoseSquare"))
        {
            gameplayManager.Lose();
        }
    }
}
