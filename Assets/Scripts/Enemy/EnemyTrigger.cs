using System.Collections;
using UnityEngine;

public class EnemyTrigger : MonoBehaviour
{
	[SerializeField] private AsteroidBehavior _asteroid;
	[SerializeField] private Enemy _enemy;

	private bool _isShot;
	private IEnumerator OnShot()
	{
		this._isShot = true;
		yield return new WaitForSeconds(this._enemy.GetWaitTimeUntilIdle());
		this._isShot = false;
	}

	public bool GetIsEnemyShot()
	{
		return this._isShot;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		IDamageable iDamageable = this._enemy.GetComponent<IDamageable>();
		if (collision.CompareTag("Asteroid"))
		{
			iDamageable.OnDamaged(this._asteroid.getAsteroidDamage());
		}
		else if (collision.CompareTag("Bullet"))
		{
			StartCoroutine(OnShot());

			//Bullet damage is specific to THIS bullet/collision's character type
			iDamageable.OnDamaged(collision.GetComponent<BulletManager>().GetBulletDamage());
			Debug.Log("Enemy hit: " + collision.GetComponent<BulletManager>().GetBulletDamage());
		}
	}

}
