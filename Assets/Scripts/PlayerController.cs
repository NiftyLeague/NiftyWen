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
	[Space]
	public float wenTextTransformYOffset;
	public Vector2 wenTextTransformCharacterOffset;
	public float wenTextLineRenderPointOffset;

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
		currentWenTextPosition = Mathf.Lerp(currentWenTextPosition, targetWenTextPosition, 0.03f);
		wenTextTransform.anchoredPosition = new Vector2(currentWenTextPosition, transform.position.y + wenTextTransformYOffset);

		wenMessageLineRenderer.SetPosition(0, new Vector3(wenTextTransform.anchoredPosition.x, wenTextTransform.anchoredPosition.y + wenTextLineRenderPointOffset, 0f));
		wenMessageLineRenderer.SetPosition(1, new Vector3(transform.position.x + wenTextTransformCharacterOffset.x, transform.position.y + wenTextTransformCharacterOffset.y, 0f));
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
