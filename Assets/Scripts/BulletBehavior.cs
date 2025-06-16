using UnityEngine;
using UnityEngine.Rendering;

public class BulletBehavior : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Rigidbody2D _rb2d;

	[Header("Global Bullet Settings")]
	[SerializeField] private float _bulletForce = 15f;
	[SerializeField] private float _destroyTime = 1.25f;

	private void Start()
	{ 
		LaunchBullet();
		DestroyAfter(this._destroyTime);
	}

	private void LaunchBullet()
	{
		// Both do the same thing
		//this._rb2d.linearVelocity = transform.right * this._bulletSpeed;
		this._rb2d.AddForce(this.transform.right * this._bulletForce, ForceMode2D.Impulse);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Ground") || collision.CompareTag("Asteroid") ||
			collision.CompareTag("Enemy") || collision.CompareTag("Player"))
		{
			Destroy(gameObject);
		}
	}

	private void DestroyAfter(float time)
	{
		if (this.gameObject != null)
		{
			Destroy(gameObject, time);
		}
	}

}
