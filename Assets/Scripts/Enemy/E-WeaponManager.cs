using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EWeaponManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject _bulletPrefab;
	[SerializeField] private Transform _bulletSpawnPoint;
	[SerializeField] private Enemy _enemy;
	[SerializeField] private EBulletManager _enemyBulletManager;

	#region Enemy Weapon Settings
	[Header("Guard Weapon Settings")]
	[SerializeField] private float _guardTimeBetweenEnemyShots = 0.6f;
	[SerializeField] private float _guardBulletDamage = 20f;

	[Header("Cop Weapon Settings")]
	[SerializeField] private float _copTimeBetweenEnemyShots = 0.72f;
	[SerializeField] private float _copBulletDamage = 33f;

	[Header("Lil Guard Weapon Settings")]
	[SerializeField] private float _lilGuardTimeBetweenEnemyShots = 0.25f;
	[SerializeField] private float _lilGuardBulletDamage = 8f;
	#endregion

	private float _timeBetweenEnemyShots;
	private float _nextShootTime = 0f;

	private void FixedUpdate()
	{
		if (this._enemy.GetClosestPlayer() != null && (this._enemy.GetEnemyState() == Enemy.EnemyState.Attack))
		{
			HandleEnemyShoot();
		}
	}

	#region EnemyType Weapon Settings
	private void Guard()
	{
		this._timeBetweenEnemyShots = this._guardTimeBetweenEnemyShots;
		this._enemyBulletManager.SetEnemyBulletDamage(this._guardBulletDamage);
	}

	private void Cop()
	{
		this._timeBetweenEnemyShots = this._copTimeBetweenEnemyShots;
		this._enemyBulletManager.SetEnemyBulletDamage(this._copBulletDamage);
	}

	private void LilGuard()
	{
		this._timeBetweenEnemyShots = this._lilGuardTimeBetweenEnemyShots;
		this._enemyBulletManager.SetEnemyBulletDamage(this._lilGuardBulletDamage);
	}
	#endregion

	private void HandleEnemyShoot()
	{
		//Time.time is the actual time accumulated every single frame since the game started
		//Different from Time.deltaTime which is a static time of 0.0167 seconds for every single frame (60 FPS)
		if (Time.time >= this._nextShootTime)
		{
			GameObject bulletInstance = Instantiate(this._bulletPrefab, this._bulletSpawnPoint.position, this.transform.rotation);
			this._nextShootTime = Time.time + this._timeBetweenEnemyShots;
		}
	}

	public void HandleBulletDamage()
	{
		switch(this._enemy.GetEnemyType())
		{
			case Enemy.EnemyType.Guard: Guard(); break;
			case Enemy.EnemyType.Cop: Cop(); break;	
			case Enemy.EnemyType.LilGuard: LilGuard(); break;
		}
	}
}
