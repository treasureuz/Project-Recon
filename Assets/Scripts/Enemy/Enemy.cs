using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
	[Header("References")]
	[SerializeField] private Rigidbody2D _rb2d;
	[SerializeField] private RadiusManager _radiusManager;
	[SerializeField] private CircleCollider2D _radius;
	[SerializeField] private EWeaponManager _enemyWeaponManager;

	[Header("Move Bounds")]
	[SerializeField] private Vector2 _minBounds;
	[SerializeField] private Vector2 _maxBounds;

	#region Other Enemy Settings
	[Header("Global Enemy Settings")]
	[SerializeField] private float _rotationDuration = 0.2f; //How long to rotate towards player position
	[SerializeField] private float _moveDuration = 1f; //How long to move towards random position
	[SerializeField] private float _moveOffset; // How far enemy is allowed to move each time

	[Header("Slow Mode Settings")]
	[SerializeField] private float _slowAmount = 0.2f; // Amount to slow down the enemy when shot
	[SerializeField] private float _moveSpeedThreshold = 0.85f; // Minimum speed for the enemy to move
	[SerializeField] private float _timeBetweenMovesThreshold = 3.5f; // Minimum time between moves for the enemy
	#endregion

	#region Enemy Settings
	[Header("Guard Enemy Settings")]
	[SerializeField] private float _guardTimeBetweenMoves = 2f;
	[SerializeField] private float _guardMoveSpeed = 1.55f;
	[SerializeField] private float _guardMaxHealth = 120f;
	[SerializeField] private float _guardWaitTimeUntilPatrol = 3.85f;

	[Header("Cop Enemy Settings")]
	[SerializeField] private float _copTimeBetweenMoves = 1.65f;
	[SerializeField] private float _copMoveSpeed = 2f;
	[SerializeField] private float _copMaxHealth = 180f; 
	[SerializeField] private float _copWaitTimeUntilPatrol = 5.58f;

	[Header("Lil Guard Enemy Settings")]
	[SerializeField] private Enemy _lilGuardPrefab;
	[SerializeField] private float _lilGuardMoveSpeed = 1.75f;
	[SerializeField] private float _lilGuardMaxHealth = 180f;
	#endregion

	private Player _player;
	private EnemyHelper _enemyHelper;
	private LilGuardSpawnManager _lilGuardSpawnPointsParent;

	private GameObject _closestPlayer;
	private List<GameObject> _players;

	// Forces z axis to be 0
	private Vector2 _directionToClosestPlayer;
	private Vector2 _targetPosition;

	#region Enemy Settings Variables
	private float _timeBetweenMoves;
	private float _currentHealth;
	private float _moveSpeed;
	private float _waitTimeUntilPatrol;

	private float _closestDistance = Mathf.Infinity; //Set to positive infinity

	private float _elapsedMoveTime; // Used for move interpolation
	private float _elapsedTimeInRadius; // Used for radius check

	private bool _isWithinRadius;
	private bool _isShot;
	private bool _hasResetSettings;
	#endregion

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

	private void Start()
	{
		#region Find References
		// Finds the active LilGuardSpawnPointsParent in the scene
		this._lilGuardSpawnPointsParent = FindAnyObjectByType<LilGuardSpawnManager>();

		this._radiusManager = this.gameObject.GetComponentInChildren<RadiusManager>();
		this._radius = this.transform.Find("Radius").GetComponent<CircleCollider2D>();

		this._player = FindAnyObjectByType<Player>();	
		this._players = this._player.GetPlayerList(); // Get all players in the scene

		this._enemyHelper = FindAnyObjectByType<EnemyHelper>(); // Finds the active EnemyHelper in the scene
		this._enemyHelper.AddEnemyToList(this); // Adds this enemy to the enemy list in EnemyHelper
		#endregion

		UpdateMoveBounds(); // Updates the move bounds based on the radius
		//Generate position to move to on start
		this._targetPosition = GenerateDodgePosition();

		HandleEnemyType();
		this._enemyWeaponManager.HandleEnemyWeaponType(); // Sets enemy weapon type
	}

	private void Update()
	{
		UpdateMoveBounds(); // Updates the move bounds based on the radius
		IsWithinRadius(); //Not needed but adds clarity as Enemy HAS the radius
		UpdateEnemyState();
		FindClosestPlayer();

		if (this._closestPlayer != null && this.enemyState == EnemyState.Attack)
		{
			HandleEnemyMovement();
			HandleEnemyRotation();
		}
	}

	private void FixedUpdate()
	{
		
	}

	private void UpdateEnemyState()
	{
		if (this._isShot || this._isWithinRadius) this.enemyState = EnemyState.Attack;
		else this.enemyState = EnemyState.Patrol;
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

	private void FindClosestPlayer()
	{
		this._closestDistance = Mathf.Infinity; // Reset every frame (Guarantees closestPlayer exists)
		this._players = this._player.GetPlayerList(); // Get all players in the scene

		foreach (GameObject player in this._players)
		{
			float distance = Vector2.Distance(this.transform.position, player.transform.position);
			if (distance < this._closestDistance)
			{
				this._closestDistance = distance;
				this._closestPlayer = player;
			}
		}
	}

	#region Enemy Handlers
	private void HandleEnemyType()
	{
		switch (enemyType)
		{
			case EnemyType.Guard: Guard(); break;
			case EnemyType.Cop: Cop(); break;
			case EnemyType.LilGuard: LilGuard(); break;
		}
	}

	private void HandleEnemyMovement()
	{
		MoveTowardsClosestPlayer();
		if (!(enemyType == EnemyType.LilGuard)) HandleDodging();
	}

	#region Enemy Movement
	private void MoveTowardsClosestPlayer()
	{
		Vector2 closestPlayerPos = new Vector2(this._closestPlayer.transform.position.x + 2.35f, this._closestPlayer.transform.position.y);
		Vector2 direction = (closestPlayerPos - this._rb2d.position).normalized;

		//This allows for interpolation
		Vector2 newPosition = this._rb2d.position + (direction * this._moveSpeed * Time.fixedDeltaTime);

		//Move towards player
		this._rb2d.MovePosition(newPosition);
	}

	private void HandleDodging()
	{
		Vector2 startPosition = this.transform.position;
		//First, "Dodge"/"Move" to the generated position. **Takes 1 sec for move interpolation to finish**
		if (this._elapsedMoveTime < this._moveDuration)
		{
			float t = this._elapsedMoveTime / this._moveDuration; //a fraction of the total move duration (0 to 1)
			this.transform.position = Vector2.Lerp(startPosition, this._targetPosition, t);
		}
		this._elapsedMoveTime += Time.fixedDeltaTime;

		float nextMoveTime = this._timeBetweenMoves + this._moveDuration; // Total time until next move
		//Then wait 2 sec before "dodging" or "moving" again
		if (this._elapsedMoveTime >= nextMoveTime)
		{
			this._targetPosition = GenerateDodgePosition();
			this._elapsedMoveTime = 0f; // Reset elapsed move time
		}
	}
	#endregion

	private void HandleEnemyRotation()
	{
		// Checks if direction is positive (player is to the left) or negative (player is to the right)
		this._directionToClosestPlayer = (this.transform.position - this._closestPlayer.transform.position); 

		float angle = Vector3.SignedAngle(this.transform.right, this._directionToClosestPlayer, Vector3.forward);

		//Rotate smoothly/step by step
		float t = Time.deltaTime / this._rotationDuration; // a fraction of the total rotation duration
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

	// Handles the slow mode effect when the enemy is shot **(Sora's bullet)**
	private void HandleSlowMode(float slowAmount)
	{
		this._moveSpeed -= slowAmount; // Slow down the enemy when shot
		if (this._moveSpeed < this._moveSpeedThreshold) this._moveSpeed = this._moveSpeedThreshold; // Prevents speed from going below threshold (0.85f)

		this._timeBetweenMoves += slowAmount; // Slow down the enemy's movement speed
		if (this._timeBetweenMoves > this._timeBetweenMovesThreshold) 
			this._timeBetweenMoves = this._timeBetweenMovesThreshold; // Prevents time between moves from going above threshold (3.5f)

		this._enemyWeaponManager.SetSlowModeTimeBetweenShots(slowAmount); // Adjust the enemy's shooting speed
		// Prevents shoot speed from going below the threshold
		if (this._enemyWeaponManager.GetCurrentTimeBetweenShots() > this._enemyWeaponManager.GetTimeBetweenShotsThreshold())
			this._enemyWeaponManager.SetTimeBetweenShots(this._enemyWeaponManager.GetTimeBetweenShotsThreshold()); 

		if(!this._hasResetSettings) StartCoroutine(ResetEnemySettings()); // Reset enemy settings 
	}
	#endregion

	private void UpdateMoveBounds()
	{
		if (enemyType != EnemyType.LilGuard)
		{
			//Dodging/Moving Bounds based off radius 
			this._minBounds = new Vector2(this.transform.position.x - this._radius.bounds.extents.x, -1.35f);
			this._maxBounds = new Vector2(this.transform.position.x + this._radius.bounds.extents.x, 2.35f);
		}
	}

	private IEnumerator OnEnemyShot()
	{
		this._isShot = true;
		yield return new WaitForSeconds(this._waitTimeUntilPatrol);
		this._isShot = false;
	}

	private void IsWithinRadius()
	{
		if (this._radiusManager.IsWithinRadius())
		{
			this._elapsedTimeInRadius = 0f; //Reset elapsed time in radius
			this._isWithinRadius = true;
		}
		else
		{
			this._elapsedTimeInRadius += Time.deltaTime;
			if (this._elapsedTimeInRadius >= this._waitTimeUntilPatrol)
			{
				this._isWithinRadius = false;
			}
		}
	}

	#region Generate Dodge Position
	private Vector2 GenerateDodgePosition()
	{
		this._moveOffset = Random.Range(0.75f, 1.55f); // Randomize the move offset to dodge

		// Picks best max/min x, y positions enemy is allowed to move to based on the background bounds
		float minX = Mathf.Max(this._minBounds.x, transform.position.x - this._moveOffset);
		float maxX = Mathf.Min(this._maxBounds.x, transform.position.x + this._moveOffset);

		float minY = Mathf.Max(this._minBounds.y, transform.position.y - this._moveOffset);
		float maxY = Mathf.Min(this._maxBounds.y, transform.position.y + this._moveOffset);

		// Generates random positions within appropriate bounds
		float randXPos = Random.Range(minX, maxX);
		float randYPos = Random.Range(minY, maxY);

		return new Vector2(randXPos, randYPos);
	}
	#endregion

	#region Other Helper Methods/Coroutines
	private IEnumerator ResetEnemySettings()
	{
		this._hasResetSettings = true; // Prevents multiple resets from happening at the same time
		yield return new WaitUntil(() => !this._isShot); // Wait until the enemy is not shot anymore

		this._moveSpeed = GetBaseMoveSpeed(); // Reset to base speed when not shot
		this._timeBetweenMoves = GetBaseTimeBetweenMoves(); // Reset to base time between moves when not shot
		this._enemyWeaponManager.SetTimeBetweenShots(this._enemyWeaponManager.GetBaseTimeBetweenShots()); // Reset to base shooting speed

		this._hasResetSettings = false; // Reset the hasResetSettings boolean
	}

	private void TriggerOnEnemyShot()
	{
		if (this._isShot) StopCoroutine(OnEnemyShot());
		StartCoroutine(OnEnemyShot());
	}
	
	private float GetBaseMoveSpeed()
	{
		switch (this.enemyType)
		{
			case EnemyType.Cop: return this._copMoveSpeed;
			case EnemyType.Guard: return this._guardMoveSpeed;
			case EnemyType.LilGuard: return this._lilGuardMoveSpeed;
			default: return 0f;
		}
	}
	private float GetBaseTimeBetweenMoves()
	{
		switch (this.enemyType)
		{
			case EnemyType.Cop: return this._copTimeBetweenMoves;
			case EnemyType.Guard: return this._guardTimeBetweenMoves;
			case EnemyType.LilGuard: return this._lilGuardMoveSpeed; // Lil Guard doesn't have a time between moves
			default: return 0f;
		}
	}
	#endregion

	#region Setters/Adders && Getters
	public GameObject GetClosestPlayer()
	{
		return this._closestPlayer;
	}

	public EnemyState GetEnemyState()
	{
		return this.enemyState;
	}

	public EnemyType GetEnemyType()
	{
		return this.enemyType;
	}

	public bool GetIsWithinRadius()
	{
		return this._isWithinRadius;
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
				Instantiate(this._lilGuardPrefab, this._lilGuardSpawnPointsParent.GetTopSpawnPoint(), this.transform.rotation);
				Instantiate(this._lilGuardPrefab, this._lilGuardSpawnPointsParent.GetBottomSpawnPoint(), this.transform.rotation);
				this._enemyHelper.RemoveEnemyFromList(this);
				Destroy(gameObject); break;
			default:
				this._enemyHelper.RemoveEnemyFromList(this);
				Destroy(gameObject); break;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		PBulletManager collisionBM = collision.GetComponent<PBulletManager>();
		IDamageable iDamageable = this.GetComponent<IDamageable>();

		if (collision.CompareTag("PlayerBullet"))
		{
			TriggerOnEnemyShot(); //Enables isShot boolean and switches Enemy to attack mode
			iDamageable.OnDamaged(collisionBM.GetPlayerBulletDamage());

			Vector3 spawnPos = new Vector3(this.transform.position.x, this.transform.position.y + 0.3f);
			UIManager.instance.SpawnDamageText(spawnPos, (int) collisionBM.GetPlayerBulletDamage());
		}
	}

	#region IDamageable Interface Implementation
	public void OnDamaged(float damageAmount)
	{
		this._currentHealth -= damageAmount;
		if (this._currentHealth <= 0) DestroyOnDie(); // Destroy enemy when health reaches zero

		if (this._player.GetPlayerType() == Player.PlayerType.Sora) HandleSlowMode(this._slowAmount);
	}
	#endregion
}
