using UnityEngine;
using UnityEngine.InputSystem;

public class Enemy : MonoBehaviour, IDamageable
{
	[Header("References")]
	[SerializeField] private Rigidbody2D _rb2d;
	[SerializeField] private AsteroidBehavior _asteroid;
	//[SerializeField] private BulletManager _bullet;
	[SerializeField] private Transform _player;

	[Header("Bounds")]
	[SerializeField] private Vector2 _minBounds = new Vector2(-12.35f, 3.42f);
	[SerializeField] private Vector2 _maxBounds = new Vector2(-6.65f, 2.42f);

	[Header("Enemy Settings")]
	[SerializeField] private float _moveDuration = 1f;
	[SerializeField] private float _timeBetweenMoves = 2f;
	[SerializeField] private float _maxHealth = 120f; // Max health of the player

	// Forces z axis to be 0
	private Vector2 _directionToPlayer;
	private Vector2 _targetPosition;

	private float _currentHealth;
	private float _elapsedMoveTime;

	private float offset = 0.75f; // How far enemy is allowed to move each time

	private void Start()
	{
		this._player = GameObject.FindWithTag("Player").transform; // Fixes broken prefab reference issue

		this._targetPosition = GenerateRandomPosition();

		this._currentHealth = this._maxHealth; // Initialize health
	}

	private void FixedUpdate()
	{
		if (this._player != null)
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
		float t = Time.fixedDeltaTime / 0.25f; // a fraction of the total angle you want to rotate THIS frame
		this.transform.Rotate(Vector3.forward, angle * t);
	}

	private void HandleEnemyMovement()
	{
		// Wait 0.85 secs before moving (added moveDuration because elapsedTime IS equal/greater than moveDuration)
		this._elapsedMoveTime += Time.fixedDeltaTime;
		if (this._elapsedMoveTime >= this._timeBetweenMoves + this._moveDuration)
		{
			this._targetPosition = GenerateRandomPosition();
			this._elapsedMoveTime = 0f;
		}

		// The enemy is moving so wait 1 secs for interpolation to finish
		if (this._elapsedMoveTime < this._moveDuration)
		{
			float t = Mathf.Clamp01(this._elapsedMoveTime / this._moveDuration);
			this.transform.position = Vector2.Lerp(this.transform.position, this._targetPosition, t);
		}
	}

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

	private void OnTriggerEnter2D(Collider2D collision)
	{
		IDamageable iDamageable = GetComponent<IDamageable>();
		if (collision.CompareTag("Asteroid"))
		{
			iDamageable.OnDamaged(this._asteroid.getAsteroidDamage());
		}
		else if (collision.CompareTag("Bullet"))
		{
			//Bullet damage is specific to THIS bullet/collision's character type
			iDamageable.OnDamaged(collision.GetComponent<BulletManager>().GetBulletDamage());
			Debug.Log("Enemy hit: " + collision.GetComponent<BulletManager>().GetBulletDamage());
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
