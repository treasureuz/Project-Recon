using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy : MonoBehaviour, IDamageable
{
	[Header("References")]
	[SerializeField] private Rigidbody2D _rb2d;
	[SerializeField] private Transform _player;
	[SerializeField] private RadiusManager _radiusManager;
	[SerializeField] private EnemyTrigger _enemyTrigger;

	[Header("Bounds")]
	[SerializeField] private Vector2 _minBounds;
	[SerializeField] private Vector2 _maxBounds;

	[Header("Enemy Settings")]
	//[SerializeField] private float _moveDuration = 1f;
	//[SerializeField] private float _timeBetweenMoves = 2f;
	[SerializeField] private float _moveSpeed = 2.8f;
	[SerializeField] private float _maxHealth = 120f; // Max health of the player
	[SerializeField] private float _waitTimeUntilIdle = 3f;

	// Forces z axis to be 0
	private Vector2 _directionToPlayer;
	//private Vector2 _targetPosition;

	private float _currentHealth;
	//private float _elapsedMoveTime;
	
	private float offset = 0.75f; // How far enemy is allowed to move each time

	private bool _isWithinRadius;

	private void Awake()
	{
		this._player = GameObject.FindWithTag("Player").transform; // Fixes broken prefab reference issue
	}

	private void Start()
	{
		this._currentHealth = this._maxHealth; // Initialize health
		this._minBounds = new Vector2(-12.35f, 3.42f);
		this._maxBounds = new Vector2(-6.65f, 2.42f);
		//this._targetPosition = GenerateRandomPosition();
	}

	//Not needed but adds clarity as Enemy HAS the radius
	private void Update()
	{
		if (this._radiusManager.IsWithinRadius()) this._isWithinRadius = true;
		else this._isWithinRadius = false;
	}

	private void FixedUpdate()
	{
		if (this._player != null && (this._enemyTrigger.GetIsEnemyShot() || this._isWithinRadius))
		{
			HandleEnemyRotation();
			HandleEnemyMovement();
		}
	}

	private void HandleEnemyRotation()
	{
		// Checks if direction is positive (player is to the left) or negative (player is to the right)
		this._directionToPlayer = (this.transform.position - this._player.position);

		float angle = Vector3.SignedAngle(this.transform.right, this._directionToPlayer, Vector3.forward);

		//Rotate smoothly/step by step
		float t = Time.fixedDeltaTime / 0.2f; // a fraction of the total angle you want to rotate THIS frame
		this.transform.Rotate(Vector3.forward, angle * t);
	}

	private void HandleEnemyMovement()
	{
		// Wait 0.85 + 1 secs before moving
		//this._elapsedMoveTime += Time.fixedDeltaTime;
		//if (this._elapsedMoveTime >= this._timeBetweenMoves + this._moveDuration)
		//{
		//	this._targetPosition = GenerateRandomPosition();
		//	this._elapsedMoveTime = 0f;
		//}

		//// The enemy is moving so wait 1 secs for interpolation to finish
		//if (this._elapsedMoveTime < this._moveDuration)
		//{
		//	float t = Mathf.Clamp01(this._elapsedMoveTime / this._moveDuration);
		//	this.transform.position = Vector2.Lerp(this.transform.position, this._targetPosition, t);
		//}

		//This allows for interpolation
		Vector2 playerPos = new Vector2(this._player.position.x + 0.5f, this._player.position.y);
		Vector2 direction = (playerPos - this._rb2d.position).normalized;
		Vector2 newPosition = this._rb2d.position + (direction * this._moveSpeed * Time.fixedDeltaTime);
		
		this._rb2d.MovePosition(newPosition);
	}

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

	#region Setters && Getters
	public bool GetIsWithinRadius()
	{
		return this._isWithinRadius;
	}

	public float GetWaitTimeUntilIdle()
	{
		return this._waitTimeUntilIdle;
	}

	#endregion

	#region IDamageable Interface Implementation
	public void OnDamaged(float damageAmount)
	{
		this._currentHealth -= damageAmount;
		if (this._currentHealth <= 0)
		{
			Debug.Log("Enemy Dead");
			Destroy(gameObject); // Destroy enemy when health reaches zero
		}
	}

	#endregion
}
