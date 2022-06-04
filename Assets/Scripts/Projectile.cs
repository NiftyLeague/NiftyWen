using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
	public bool isABomb;
	public Rigidbody2D rigidBody;
	public float moveSpeed;
	float arc = 25;
	public bool gotHit;
	public bool hitScoreCollider;

	private void Start()
	{

		Destroy(gameObject, 10);
	}

	void Update()
	{
		if (gotHit)
		{
			arc = Mathf.Lerp(arc, 0, 0.002f);
			rigidBody.velocity = new Vector2(moveSpeed * 2, arc);
		}
		else
		{
			rigidBody.velocity = new Vector2(-moveSpeed, 0);
		}

		if (transform.position.x > GameplayManager.I.worldRightLimit.position.x || transform.position.x < GameplayManager.I.worldLeftLimit.position.x)
		{
			Destroy(gameObject);
		}
	}

	public void SetNewSpeed(float amount)
	{
		moveSpeed += amount;
		if (moveSpeed > 50)
		{
			moveSpeed = 50;
		}

		if (Random.value < 0.01f)
		{
			moveSpeed = 20;
		}
	}

	public void HitProjectile(float chargePower)
	{
		if (isABomb)
		{
			Destroy(gameObject);
		}
		else
		{
			gotHit = true;
			moveSpeed *= (1 + chargePower);
			transform.localScale = new Vector3(1 + (chargePower / 2), 1 - (chargePower / 2), 1);
		}
	}
}
