using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
	public GameplayManager gameplayManager;
	public AudioManager audioManager;
	[Space]
	public Transform playerTransform;
	public RectTransform wenTextTransform;
	public TextMeshProUGUI wenText;
	[Space]
	public LineRenderer wenMessageLineRenderer;

	float currentWenTextPosition;
	float targetWenTextPosition;
	float minWenTextPosition = -8.4f;
	float maxWenTextPosition = 4.2f;

	private InputState input = new InputState();

	private void Start()
	{
		PlayerSpriteManager.I.SetCharacterSprites();
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
			if (!PlayerSpriteManager.I.CanChangeCharacters())
			{
				return;
			}
			audioManager.PlaySound(AudioManager.SoundID.menuOptionSelect);
			PlayerSpriteManager.I.ChangeCharacter();
		}
	}

    private void Update()
    {
		targetWenTextPosition = playerTransform.position.x;
		targetWenTextPosition = Mathf.Clamp(targetWenTextPosition, minWenTextPosition, maxWenTextPosition);
		currentWenTextPosition = Mathf.Lerp(currentWenTextPosition, targetWenTextPosition, 0.01f);
		wenTextTransform.anchoredPosition = new Vector2(currentWenTextPosition, 10f);

		wenMessageLineRenderer.SetPosition(0, new Vector3(wenTextTransform.anchoredPosition.x, -3, 0));
		wenMessageLineRenderer.SetPosition(1, new Vector3(transform.position.x, transform.position.y + 1, 0));
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
