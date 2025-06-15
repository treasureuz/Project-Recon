using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
	[Header("References")]
	[SerializeField] private Rigidbody2D _rb2d;
	[SerializeField] private Transform _player;
	[SerializeField] private AsteroidBehavior _asteroid;
	[SerializeField] private RadiusManager _radiusManager;

	[Header("Move Bounds")]
	[SerializeField] private Vector2 _minBounds;
	[SerializeField] private Vector2 _maxBounds;

	[Header("Enemy Settings")]
	[SerializeField] private float _rotationDuration = 0.2f; //How long to rotate towards player position
	[SerializeField] private float _moveDuration = 1f; //How long to move towards random position
	[SerializeField] private float _timeBetweenMoves = 2f;
	[SerializeField] private float _moveSpeed = 2.8f;
	[SerializeField] private float _maxHealth = 120f; // Max health of the player
	[SerializeField] private float _waitTimeUntilIdle = 3f;

	// Forces z axis to be 0
	private Vector2 _directionToPlayer;
	private Vector2 _targetPosition;

	private float _currentHealth;
	private float _elapsedMoveTime;
	private float offset = 0.75f; // How far enemy is allowed to move each time

	private bool _isWithinRadius;
	private bool _isShot;

	public enum EnemyState
	{
		Patrol,
		Attack,
	}

	public EnemyState state = EnemyState.Patrol;

	private void Awake()
	{
		this._player = GameObject.FindWithTag("Player").transform; // Fixes broken prefab reference issue
	}

	private void Start()
	{
		this._currentHealth = this._maxHealth; // Initialize health

		//Dodging/Moving Bounds
		this._minBounds = new Vector2(this.transform.position.x - 5.2f, -2.42f);
		this._maxBounds = new Vector2(this.transform.position.x + 5.2f, 3.42f);

		//Generate position to move to on start
		this._targetPosition = GenerateRandomPosition();
	}
	
	private void Update()
	{
		//Not needed but adds clarity as Enemy HAS the radius
		if (this._radiusManager.IsWithinRadius()) this._isWithinRadius = true;
		else this._isWithinRadius = false;

		UpdateEnemyState();
	}

	private void FixedUpdate()
	{
		if (this._player != null) PerformEnemyAction();
	}

	private void PerformEnemyAction()
	{
		switch (state)
		{
			case EnemyState.Attack:
				HandleEnemyRotation();
				HandleEnemyMovement();
				break;
			case EnemyState.Patrol: break;
		}
	}

	private void UpdateEnemyState()
	{
		if (this._isShot || this._isWithinRadius) state = EnemyState.Attack;
		else state = EnemyState.Patrol;
	}

	#region Enemy Actions
	private void HandleEnemyRotation()
	{
		// Checks if direction is positive (player is to the left) or negative (player is to the right)
		this._directionToPlayer = (this.transform.position - this._player.position);

		float angle = Vector3.SignedAngle(this.transform.right, this._directionToPlayer, Vector3.forward);

		//Rotate smoothly/step by step
		float t = Time.fixedDeltaTime / this._rotationDuration; // a fraction of the total angle you want to rotate THIS frame
		this.transform.Rotate(Vector3.forward, angle * t);
	}

	private void HandleEnemyMovement()
	{
		//This allows for interpolation
		Vector2 playerPos = new Vector2(this._player.position.x + 0.5f, this._player.position.y);
		Vector2 direction = (playerPos - this._rb2d.position).normalized;
		Vector2 newPosition = this._rb2d.position + (direction * this._moveSpeed * Time.fixedDeltaTime);
		//Move towards player
		this._rb2d.MovePosition(newPosition);

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

	private IEnumerator OnEnemyShot()
	{
		this._isShot = true;
		yield return new WaitForSeconds(this._waitTimeUntilIdle);
		this._isShot = false;
	}

	#region Setters && Getters
	public EnemyState GetEnemyState()
	{
		return this.state;
	}

	public float GetWaitTimeUntilIdle()
	{
		return this._waitTimeUntilIdle;
	}

	#endregion

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
			Destroy(gameObject); // Destroy enemy when health reaches zero
		}
	}

	#endregion
}
