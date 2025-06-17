using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
	[Header("References")]
	[SerializeField] private Rigidbody2D _rb2d;
	[SerializeField] private Transform _player;
	[SerializeField] private AsteroidBehavior _asteroid;
	[SerializeField] private RadiusManager _radiusManager;
	[SerializeField] private EWeaponManager _enemyWeaponManager;

	[Header("Move Bounds")]
	[SerializeField] private Vector2 _minBounds;
	[SerializeField] private Vector2 _maxBounds;

	[Header("Global Enemy Settings")]
	[SerializeField] private float _rotationDuration = 0.2f; //How long to rotate towards player position
	[SerializeField] private float _moveDuration = 1f; //How long to move towards random position

	#region Enemy Settings
	[Header("Guard Enemy Settings")]
	[SerializeField] private float _guardTimeBetweenMoves = 2f;
	[SerializeField] private float _guardMoveSpeed = 1.55f;
	[SerializeField] private float _guardMaxHealth = 120f;
	[SerializeField] private float _guardWaitTimeUntilPatrol = 4f;

	[Header("Cop Enemy Settings")]
	[SerializeField] private float _copTimeBetweenMoves = 1.65f;
	[SerializeField] private float _copMoveSpeed = 2f;
	[SerializeField] private float _copMaxHealth = 180f; 
	[SerializeField] private float _copWaitTimeUntilPatrol = 7.5f;

	[Header("Lil Guard Enemy Settings")]
	[SerializeField] private Enemy _lilGuardPrefab;
	[SerializeField] private Transform _lilGuardTopSpawnPoint;
	[SerializeField] private Transform _lilGuardBottomSpawnPoint;
	[SerializeField] private float _lilGuardMoveSpeed = 1.75f;
	[SerializeField] private float _lilGuardMaxHealth = 180f;
	#endregion

	private Enemy _lilGuardTopInstance;
	private Enemy _lilGuardBottomInstance;

	// Forces z axis to be 0
	private Vector2 _directionToPlayer;
	private Vector2 _targetPosition;

	private float _timeBetweenMoves;
	private float _currentHealth;
	private float _moveSpeed;
	private float _waitTimeUntilPatrol;

	private float _elapsedMoveTime;
	private float offset = 0.75f; // How far enemy is allowed to move each time

	private bool _isWithinRadius;
	private bool _isShot;

	#region Enemy Enums
	public enum EnemyType
	{
		Guard,
		Cop, 
		LilGuard
	}

	public enum EnemyState
	{
		Patrol,
		Attack,
	}
	#endregion

	[Header("Enemy Type & State")]
	public EnemyType enemyType;
	public EnemyState enemyState;

	private void Awake()
	{
		this._player = GameObject.FindWithTag("Player").transform; // Fixes broken prefab reference issue
	}

	private void Start()
	{
		GameObject lilGuardSpawnPoints = GameObject.Find("LilGuardSpawnPoints");
		this._lilGuardTopSpawnPoint = lilGuardSpawnPoints.transform.Find("TopSpawnPoint").transform;
		this._lilGuardBottomSpawnPoint = lilGuardSpawnPoints.transform.Find("BottomSpawnPoint").transform;

		if (enemyType != EnemyType.LilGuard)
		{
			//Dodging/Moving Bounds based off radius 
			this._minBounds = new Vector2(this.transform.position.x - 5.2f, -2.42f);
			this._maxBounds = new Vector2(this.transform.position.x + 5.2f, 3.42f);

			//Generate position to move to on start
			this._targetPosition = GenerateRandomPosition();
		}

		HandleEnemyType();
		this._enemyWeaponManager.HandleBulletDamage(); // Sets enemy weapon type
	}

	private void Update()
	{
		//Not needed but adds clarity as Enemy HAS the radius
		if (this._radiusManager.IsWithinRadius()) StartCoroutine(IsWithinRadius());

		UpdateEnemyState();
	}

	private void FixedUpdate()
	{
		if (this._player != null) PerformEnemyAction();
	}

	private void PerformEnemyAction()
	{
		switch (enemyState)
		{
			case EnemyState.Attack:
				HandleEnemyRotation();
				HandleEnemyMovement();
				break;
			case EnemyState.Patrol: break;
		}
	}

	#region Enemy Type Settings
	private void Guard()
	{
		this._timeBetweenMoves = this._guardTimeBetweenMoves;
		this._moveSpeed = this._guardMoveSpeed;
		this._currentHealth = this._guardMaxHealth;
		this._waitTimeUntilPatrol = this._guardWaitTimeUntilPatrol;
	}
	
	private void Cop()
	{
		this._timeBetweenMoves = this._copTimeBetweenMoves;
		this._moveSpeed = this._copMoveSpeed;
		this._currentHealth = this._copMaxHealth;
		this._waitTimeUntilPatrol = this._copWaitTimeUntilPatrol;
	}

	private void LilGuard()
	{
		this._moveSpeed = this._lilGuardMoveSpeed;
		this._currentHealth = this._lilGuardMaxHealth;
	}
	#endregion

	#region Enemy Actions
	private void HandleEnemyType()
	{
		switch (enemyType)
		{
			case EnemyType.Guard: Guard(); break;
			case EnemyType.Cop: Cop(); break;
			case EnemyType.LilGuard: LilGuard(); break;
		}
	}

	private void HandleEnemyRotation()
	{
		// Checks if direction is positive (player is to the left) or negative (player is to the right)
		this._directionToPlayer = (this.transform.position - this._player.position);

		float angle = Vector3.SignedAngle(this.transform.right, this._directionToPlayer, Vector3.forward);

		//Rotate smoothly/step by step
		float t = Time.fixedDeltaTime / this._rotationDuration; // a fraction of the total angle you want to rotate THIS frame
		this.transform.Rotate(Vector3.forward, angle * t);

		//Normalize angle for scale flipping (eulerAngles.z can't be negative -- 0 to 360 degrees)
		float zAngle = this.transform.eulerAngles.z;
		if (zAngle > 180f) zAngle -= 360f;

		Vector3 localScale = this.transform.localScale;

		// Flip the enemy sprite vertically when facing left
		if (zAngle > 120f || zAngle < -100f) localScale.y = -Mathf.Abs(localScale.y);
		else localScale.y = Mathf.Abs(localScale.y); // Keep the enemy sprite upright when facing right

		this.transform.localScale = localScale; // Apply the scale to the enemy
	}

	private void HandleEnemyMovement()
	{
		MoveTowardsPlayer();
		if (enemyType != EnemyType.LilGuard) HandleDodging();
	}

	private void MoveTowardsPlayer()
	{
		//This allows for interpolation
		Vector2 playerPos = new Vector2(this._player.position.x + 2f, this._player.position.y);
		Vector2 direction = (playerPos - this._rb2d.position).normalized;
		Vector2 newPosition = this._rb2d.position + (direction * this._moveSpeed * Time.fixedDeltaTime);
		//Move towards player
		this._rb2d.MovePosition(newPosition);
	}
	private void HandleDodging()
	{
		this._elapsedMoveTime += Time.fixedDeltaTime;
		// "Dodge"/"Move" to the generated position. **Takes 1 sec for move interpolation to finish**
		if (this._elapsedMoveTime < this._moveDuration)
		{
			//Reposition smoothly
			float t = Mathf.Clamp01(this._elapsedMoveTime / this._moveDuration);
			this.transform.position = Vector3.Slerp(this.transform.position, this._targetPosition, t);
		}

		//Then wait 2 sec before "dodging" or "moving" again
		if (this._elapsedMoveTime >= this._timeBetweenMoves + this._moveDuration)
		{
			this._targetPosition = GenerateRandomPosition();
			this._elapsedMoveTime = 0f;
		}
	}
	#endregion

	#region Generate Random Position
	private Vector2 GenerateRandomPosition()
	{
		// Picks best max/min x, y positions enemy is allowed to move to based on the background bounds
		float minX = Mathf.Max(this._minBounds.x, transform.position.x - offset);
		float maxX = Mathf.Min(this._maxBounds.x, transform.position.x + offset);

		float minY = Mathf.Max(this._minBounds.y, transform.position.y - offset);
		float maxY = Mathf.Min(this._maxBounds.y, transform.position.y + offset);

		// Generates random positions within appropriate bounds
		float randXPos = Random.Range(minX, maxX);
		float randYPos = Random.Range(minY, maxY);

		return new Vector2(randXPos, randYPos);
	}
	#endregion

	private void UpdateEnemyState()
	{
		if (this._isShot || this._isWithinRadius) enemyState = EnemyState.Attack;
		else enemyState = EnemyState.Patrol;
	}

	private IEnumerator OnEnemyShot()
	{
		this._isShot = true;
		yield return new WaitForSeconds(this._waitTimeUntilPatrol);
		this._isShot = false;
	}

	private IEnumerator IsWithinRadius()
	{
		this._isWithinRadius = true;
		yield return new WaitForSeconds(this._waitTimeUntilPatrol);
		this._isWithinRadius = false;
	}

	#region Setters && Getters
	public EnemyState GetEnemyState()
	{
		return this.enemyState;
	}

	public EnemyType GetEnemyType()
	{
		return this.enemyType;
	}

	public float GetWaitTimeUntilPatrol()
	{
		return this._waitTimeUntilPatrol;
	}
	#endregion

	private void DestroyOnDie()
	{
		switch (enemyType)
		{
			case EnemyType.Cop:
				this._lilGuardTopInstance = Instantiate
					(this._lilGuardPrefab, this._lilGuardTopSpawnPoint.position, this.transform.rotation);
				this._lilGuardBottomInstance = Instantiate
					(this._lilGuardPrefab, this._lilGuardBottomSpawnPoint.position, this.transform.rotation);
				Destroy(gameObject); break;
			default: Destroy(gameObject); break;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		PBulletManager collisionBM = collision.GetComponent<PBulletManager>();
		IDamageable iDamageable = this.GetComponent<IDamageable>();

		if (collision.CompareTag("PlayerBullet"))
		{
			StartCoroutine(OnEnemyShot()); //Enables isShot boolean and switches Enemy to attack mode
			iDamageable.OnDamaged(collisionBM.GetPlayerBulletDamage());

			Vector3 spawnPos = new Vector3(this.transform.position.x, this.transform.position.y + 0.3f);
			UIManager.instance.SpawnDamageText(spawnPos, (int) collisionBM.GetPlayerBulletDamage());
		}
	}

	#region IDamageable Interface Implementation
	public void OnDamaged(float damageAmount)
	{
		this._currentHealth -= damageAmount;
		if (this._currentHealth <= 0)
		{
			DestroyOnDie(); // Destroy enemy when health reaches zero
		}
	}
	#endregion
}
