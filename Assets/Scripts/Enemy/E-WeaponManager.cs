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

	private GameObject _bulletInstance;

	private float _timeBetweenEnemyShots;
	private float _nextShootTime = 0f;

	private void Update()
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
		this._nextShootTime += Time.deltaTime;
	
		if (this._nextShootTime >= this._timeBetweenEnemyShots)
		{
			this._bulletInstance = Instantiate(this._bulletPrefab, this._bulletSpawnPoint.position, this.transform.rotation);
			this._nextShootTime = 0f; // Reset timer after shooting
		}
	}

	public void HandleEnemyWeaponType()
	{
		switch(this._enemy.GetEnemyType())
		{
			case Enemy.EnemyType.Guard: Guard(); break;
			case Enemy.EnemyType.Cop: Cop(); break;	
			case Enemy.EnemyType.LilGuard: LilGuard(); break;
		}
	}

	#region Getters & Setters
	public void SetSlowModeTimeBetweenShots(float slowAmount)
	{
		this._timeBetweenEnemyShots += slowAmount; // Increase the time between enemy shots when slow mode is active
	}

	public void SetTimeBetweenShots(float newTimeBetweenShots)
	{
		this._timeBetweenEnemyShots = newTimeBetweenShots; // Set the time between enemy shots
	}

	public float GetCurrentTimeBetweenShots()
	{
		return this._timeBetweenEnemyShots;
	}

	public float GetBaseTimeBetweenShots()
	{
		switch (this._enemy.GetEnemyType())
		{
			case Enemy.EnemyType.Guard: return this._guardTimeBetweenEnemyShots;
			case Enemy.EnemyType.Cop: return this._copTimeBetweenEnemyShots;
			case Enemy.EnemyType.LilGuard: return this._lilGuardTimeBetweenEnemyShots;
			default: return 0f; // Default case if no enemy type matches
		}
	}

	public float GetTimeBetweenShotsThreshold()
	{
		return this._timeBetweenEnemyShots + 0.13f; // Returns the threshold for the time between shots
	}
	#endregion
}
