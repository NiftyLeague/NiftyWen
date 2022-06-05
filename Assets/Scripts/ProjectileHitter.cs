using UnityEngine;

public class ProjectileHitter : MonoBehaviour
{
	public GameplayManager gameplayManager;
	public AudioManager audioManager;
	public Character playerCharacter;
	public Collider2D hitCollider;
	float hitterTimer;

	void Update()
	{
		hitterTimer += Time.deltaTime;
		if (hitterTimer >= 0.05f)
		{
			hitCollider.enabled = true;
		}
		if (hitterTimer >= 0.11f)
		{
			hitterTimer = 0;
			TurnOff();
		}
	}

	public void TurnOn()
	{
		gameObject.SetActive(true);
		audioManager.PlaySound(AudioManager.SoundID.batSwing);
	}

	void TurnOff()
	{
		hitCollider.enabled = false;
		gameObject.SetActive(false);
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
			hitProjectile.HitProjectile(playerCharacter.attackChargeM);
			playerCharacter.HitBall(transform.position + new Vector3(0.4f, 0.4f, 0));
			if (hitProjectile.isABomb)
			{
				gameplayManager.Lose();
				gameplayManager.Explosion(collision.transform.position);
			}
			else
			{
				gameplayManager.cameraShake.Shake(0.2f + (playerCharacter.attackChargeM / 4f), 10);
				if (playerCharacter.attackChargeM >= 0.5f)
				{
					audioManager.PlaySound(AudioManager.SoundID.projectileChargeHit);
				}
				else
				{
					audioManager.PlaySound(AudioManager.SoundID.projectileHit);
				}

			}
		}
	}
}
