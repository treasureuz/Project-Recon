using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public class PlayerClone : MonoBehaviour, IDamageable
{
	[Header("References")]
	[SerializeField] private Rigidbody2D _rb2d;
	[SerializeField] private Transform _bulletSpawnPoint;
	[SerializeField] private GameObject _bulletPrefab;

	private static float _rotationDuration = 0.072f; //How long to rotate towards mouse position

	private Player _player;
	private EnemyHelper _enemyHelper;
	private PWeaponManager _playerWeaponManager;

	private Enemy _closestEnemy;
	private List<Enemy> _enemies = new List<Enemy>(); // List of all enemies in the scene

	private GameObject _bulletInstance;

	private float _currentHealth; // Player clone's health
	private float _moveSpeed;

	private float _closestDistance = Mathf.Infinity; // Start with the maximum possible distance

	private float _timeBetweenShots;
	private float _nextShootTime = 0f;

	private void Start()
	{
		this._player = GameObject.Find("Player").GetComponent<Player>(); // Finds the active Player in the scene
		this._enemyHelper = FindAnyObjectByType<EnemyHelper>(); // Finds the active EnemyHelper in the scene

		this._player.AddPlayerToList(this.gameObject); // Add the player clone to the list

		HandleStartLocalScale(); // Handle the local scale of the player clone

		this._currentHealth = this._player.GetMaxHealth();
		this._moveSpeed = this._player.GetMoveSpeed();

		this._playerWeaponManager = FindAnyObjectByType<PWeaponManager>();
		this._timeBetweenShots = this._playerWeaponManager.GetTimeBetweenShots();
	}

	private void Update()
	{
		FindClosestEnemy();
	}

	private void FixedUpdate()
	{
		if (this._closestEnemy != null)
		{
			HandlePlayerCloneRotation();
			HandlePlayerCloneMovement();
			HandlePlayerCloneShoot();
		}
	}

	private void FindClosestEnemy()
	{
		this._closestDistance = Mathf.Infinity; // Reset every frame (Guarantees closestEnemy exists)
		this._enemies = this._enemyHelper.GetEnemyList(); // Get the list of enemies from the EnemyHelper

		foreach (Enemy enemy in this._enemies)
		{
			float distance = Vector2.Distance(this.transform.position, enemy.transform.position);
			if (distance < this._closestDistance)
			{
				this._closestDistance = distance;
				this._closestEnemy = enemy;
			}
		}
	}

	#region Player Clone Handlers
	private void HandleStartLocalScale()
	{
		this.transform.localScale = this._player.transform.localScale; // Match player scale

		if (this._player.transform.localScale.y == -Mathf.Abs(this._player.transform.localScale.y))
			this.transform.eulerAngles = new Vector3(0, 0, 180); // Flip clone if player is facing left
		else this.transform.eulerAngles = new Vector3(0, 0, 0); // Keep clone upright if player is facing right
	}

	private void HandlePlayerCloneMovement()
	{
		//Offsets enemy position to avoid collision with the enemy  
		Vector2 closestEnemyPos = new Vector2(this._closestEnemy.transform.position.x - 3f, this._closestEnemy.transform.position.y);
		Vector2 direction = (closestEnemyPos - this._rb2d.position).normalized;

		//This allows for interpolation
		Vector2 newPosition = this._rb2d.position + (direction * this._moveSpeed * Time.fixedDeltaTime);

		//Move towards enemy
		this._rb2d.MovePosition(newPosition);
	}

	private void HandlePlayerCloneRotation()
	{
		Vector2 directionToEnemy = (this._closestEnemy.transform.position - this.transform.position);
		float angle = Vector3.SignedAngle(this.transform.right, directionToEnemy, Vector3.forward);

		//Rotate smoothly/step by step
		float t = Time.deltaTime / _rotationDuration; // a fraction of the total angle/rotation (THIS frame)
		this.transform.Rotate(Vector3.forward, angle * t);

		//Normalizes angle for scale flipping (eulerAngles.z can't be negative -- 0 to 360 degrees)
		float zAngle = this.transform.eulerAngles.z;
		if (zAngle > 180f) zAngle -= 360f;

		Vector3 localScale = this.transform.localScale;

		// Flip the player sprite vertically when facing left
		if (zAngle > 120f || zAngle < -100f) localScale.y = -Mathf.Abs(localScale.y);
		else localScale.y = Mathf.Abs(localScale.y); // Keep the player clone sprite upright when facing right

		this.transform.localScale = localScale; // Apply the scale to the player clone
	}

	#region Weapon Manager
	private void HandlePlayerCloneShoot()
	{
		//Time.time is the actual time accumulated every single frame since the game started
		//Different from Time.deltaTime which is a static time of 0.0167 seconds for every single frame (60 FPS)
		if (this._closestEnemy.GetIsWithinRadius() && Time.time >= this._nextShootTime)
		{
			this._bulletInstance = Instantiate(this._bulletPrefab, this._bulletSpawnPoint.position, this.transform.rotation);
			this._nextShootTime = Time.time + this._timeBetweenShots;
		}
	}
	#endregion
	#endregion

	private void OnTriggerEnter2D(Collider2D collision)
	{
		IDamageable iDamageable = GetComponent<IDamageable>();
		if (collision.CompareTag("Asteroid"))
		{
			iDamageable.OnDamaged(collision.GetComponent<AsteroidBehavior>().GetAsteroidDamage());
		}
		else if (collision.CompareTag("EnemyBullet"))
		{
			//Bullet damage is specific to THIS bullet/collision's character type
			iDamageable.OnDamaged(collision.GetComponent<EBulletManager>().GetEnemyBulletDamage());
		}
	}

	#region IDamageable Interface Implementation
	public void OnDamaged(float damageAmount)
	{
		this._currentHealth -= damageAmount;

		if (this._currentHealth <= 0)
		{
			this._player.RemovePlayerFromList(gameObject); // Remove this player clone from the player's list
			Destroy(gameObject); // Destroy the player clone when health is 0 or less
		}
	}
	#endregion
}
