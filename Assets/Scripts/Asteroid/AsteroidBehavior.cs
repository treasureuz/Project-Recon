using System.Collections;
using UnityEngine;

public class AsteroidBehavior : MonoBehaviour, IDamageable
{
	[Header("References")]
	[SerializeField] private Rigidbody2D _rb2d;

	[Header("Global Asteroid Settings")]
	[SerializeField] private float _waitTimeUntilMove = 2.55f; // Time to wait before moving again after being shot

	[Header("Slow Mode Settings")]
	[SerializeField] private float _slowForceAmount = 0.02f; // Amount to slow down the asteroid
	[SerializeField] private float _slowRotateSpeedAmount = 1.32f; // Amount to slow down the asteroid's rotation speed	
	[SerializeField] private float _forceThreshold = 0.85f; // Minimum speed before slowing down
	[SerializeField] private float _rotateSpeedThreshold = 10f; // Minimum rotation speed before slowing down

	#region Asteroid Settings
	[Header("Cosmic Asteroid Settings")]
	[SerializeField] private float _cosmicAsteroidForce = 2f;
	[SerializeField] private float _cosmicMaxHealth = 40f;
	[SerializeField] private float _cosmicAsteroidDamage = 52f; // Damage dealt to player on collision
	[SerializeField] private float _cosmicAsteroidRotationSpeed = 60f; // Speed of asteroid rotation

	[Header("Dwarf Asteroid Settings")]
	[SerializeField] private float _dwarfAsteroidForce = 2.5f;
	[SerializeField] private float _dwarfMaxHealth = 20f;
	[SerializeField] private float _dwarfAsteroidDamage = 30f; // Damage dealt to player on collision
	[SerializeField] private float _dwarfAsteroidRotationSpeed = 63.35f; // Speed of asteroid rotation
	#endregion

	private Player _player; // Reference to the player

	private float _asteroidForce;
	private float _asteroidDamage;
	private float _currentHealth;
	private float _asteroidRotationSpeed;

	private bool _isShot = false; // Indicates if the asteroid is currently shot

	public enum AsteroidType
	{
		Cosmic,
		Dwarf
	}

	[SerializeField] private AsteroidType _asteroidType; // Type of the asteroid

	private void Start()
	{
		this._player = FindAnyObjectByType<Player>(); // Finds the active Player in the scene
		HandleAsteroidType(); // Set asteroid type and properties
	}

	private void Update()
	{
		LaunchAsteroid();
		this.transform.Rotate(0, 0, this._asteroidRotationSpeed * this._asteroidForce * Time.deltaTime);
	}

	private void LaunchAsteroid()
	{
		// Both do the same thing
		this._rb2d.linearVelocity = Vector2.left * this._asteroidForce;
		//this._rb2d.AddForce(Vector2.left * this._asteroidForce, ForceMode2D.Impulse);
	}

	#region Asteroid Type Settings
	private void Cosmic()
	{
		this._currentHealth = this._cosmicMaxHealth; // Initialize health
		this._asteroidForce = this._cosmicAsteroidForce;
		this._asteroidDamage = this._cosmicAsteroidDamage;
		this._asteroidRotationSpeed = this._cosmicAsteroidRotationSpeed;
	}

	private void Dwarf()
	{
		this._currentHealth = this._dwarfMaxHealth; // Initialize health
		this._asteroidForce = this._dwarfAsteroidForce;
		this._asteroidDamage = this._dwarfAsteroidDamage;
		this._asteroidRotationSpeed = this._dwarfAsteroidRotationSpeed;
	}
	#endregion

	#region Asteroid Type Handlers
	private void HandleAsteroidType()
	{
		switch (this._asteroidType)
		{
			case AsteroidType.Cosmic: Cosmic(); break;
			case AsteroidType.Dwarf: Dwarf(); break;
		}
		this._rotateSpeedThreshold = this._asteroidRotationSpeed - 9.78f; // Set the rotation speed threshold based on the asteroid type
	}

	// Handles the slow mode effect when the enemy is shot **(Sora's bullet)**
	private IEnumerator HandleSlowMode(float slowForceAmount, float forceThreshold, float slowRotateSpeedAmount, float rotateSpeedThreshold)
	{
		this._asteroidForce -= slowForceAmount; // Slow down the enemy when shot
		if (this._asteroidForce < forceThreshold) this._asteroidForce = forceThreshold; // Prevents speed from going below threshold (0.85f)

		this._asteroidRotationSpeed -= slowRotateSpeedAmount; // Slow down the rotation speed of the asteroid
		if (this._asteroidRotationSpeed < rotateSpeedThreshold)
			this._asteroidRotationSpeed = rotateSpeedThreshold; // Prevents rotation speed from going too low

		yield return new WaitUntil(() => !this._isShot); // Wait until the asteroid is not shot anymore
		Debug.Log($"{this.gameObject.name} is not shot anymore");
		if (this._asteroidForce != GetBaseAsteroidForce()) // Only reset if the speed was changed
			this._asteroidForce = GetBaseAsteroidForce(); // Reset to base speed when not shot

		if (this._asteroidRotationSpeed != GetBaseAsteroidRotationSpeed()) // Only reset if the rotation speed was changed
			this._asteroidRotationSpeed = GetBaseAsteroidRotationSpeed(); // Reset to base rotation speed when not shot
	}
	#endregion

	private IEnumerator OnAsteroidShot()
	{
		//Reset the isShot boolean if the asteroid is shot again before the wait time is over

		this._isShot = true;
		yield return new WaitForSeconds(this._waitTimeUntilMove);
		this._isShot = false;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		PBulletManager collisionBM = collision.GetComponent<PBulletManager>();
		IDamageable iDamageable = this.GetComponent<IDamageable>();
		
		if (collision.gameObject.layer == LayerMask.NameToLayer("Player") || collision.CompareTag("Backdrop"))
		{
			Destroy(gameObject);
		}
		else if (collision.gameObject.CompareTag("PlayerBullet"))
		{
			StartCoroutine(OnAsteroidShot()); // Start the coroutine to handle slow mode
			iDamageable.OnDamaged(collisionBM.GetPlayerBulletDamage());

			Vector3 spawnPos = new Vector3(this.transform.position.x, this.transform.position.y + 0.3f);
			UIManager.instance.SpawnDamageText(spawnPos, (int) collisionBM.GetPlayerBulletDamage());
		}
	}

	#region Helper Methods
	private float GetBaseAsteroidForce()
	{
		switch(this._asteroidType)
		{
			case AsteroidType.Cosmic: return this._cosmicAsteroidForce;
			case AsteroidType.Dwarf: return this._dwarfAsteroidForce;
			default: return 0f; // Default case if no type matches
		}
	}

	private float GetBaseAsteroidRotationSpeed()
	{
		switch(this._asteroidType)
		{
			case AsteroidType.Cosmic: return this._cosmicAsteroidRotationSpeed;
			case AsteroidType.Dwarf: return this._dwarfAsteroidRotationSpeed;
			default: return 0f; // Default case if no type matches
		}
	}
	#endregion

	#region Getters and Setters
	public float GetAsteroidDamage()
	{
		return this._asteroidDamage;
	}
	#endregion

	#region IDamageable Interface Implementation
	public void OnDamaged(float damageAmount)
	{
		this._currentHealth -= damageAmount;
		if (this._currentHealth <= 0) Destroy(gameObject); // Destroy asteroid when health reaches zero

		if (this._player.GetPlayerType() == Player.PlayerType.Sora)
			StartCoroutine(HandleSlowMode(this._slowForceAmount, this._forceThreshold, this._slowRotateSpeedAmount, this._rotateSpeedThreshold));

		Debug.Log("Asteroid Force (" + this.gameObject.name + "): " + this._asteroidForce);
		Debug.Log("Asteroid Rotation Speed (" + this.gameObject.name + "): " + this._asteroidRotationSpeed);
	}
	#endregion

	
}
