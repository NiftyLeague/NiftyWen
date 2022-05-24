using UnityEngine;

public class ProjectileHitter : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public AudioManager audioManager;
    public Collider2D hitCollider;
    float hitterTimer;

    void Update()
    {
        hitterTimer += Time.deltaTime;
        if (hitterTimer >= 0.05f)
        {
            hitCollider.enabled = true;
        }
        if (hitterTimer >= 0.2f)
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
            hitProjectile.HitProjectile();
            if (hitProjectile.isABomb)
            {
                gameplayManager.Lose();
            }
            else
            {
                gameplayManager.ScorePoint();
                audioManager.PlaySound(AudioManager.SoundID.projectileHit);
            }
        }
    }
}
