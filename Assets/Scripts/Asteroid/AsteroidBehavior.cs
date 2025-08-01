using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
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
	[SerializeField] private float _rotateSpeedThreshold = 50f; // Minimum rotation speed before speeding back up

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

	#region Asteroid Settings Variables
	private float _asteroidForce;
	private float _asteroidDamage;
	private float _currentHealth;
	private float _asteroidRotationSpeed;

	private bool _isShot = false; // Indicates if the asteroid is currently shot
	private bool _hasResetSettings = false;
	#endregion

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
	}

	private void LaunchAsteroid()
	{
		// Both do the same thing
		if (!this._player.GetIsSoraFreezeActive())
		{
			this._rb2d.linearVelocity = Vector2.left * this._asteroidForce;
			this.transform.Rotate(0, 0, this._asteroidRotationSpeed * this._asteroidForce * Time.deltaTime);
		}
		else
		{
			this._rb2d.linearVelocity = Vector2.zero; // Stop the asteroid if Sora's freeze is active
			this.transform.Rotate(0, 0, 0); // Stop the rotation if Sora's freeze is active
		}
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
		this._rotateSpeedThreshold = this._asteroidRotationSpeed - 10f; // Set the rotation speed threshold based on the asteroid type
	}

	// Handles the slow mode effect when the enemy is shot **(Sora's bullet)**
	private void HandleSlowMode(float slowForceAmount, float slowRotateSpeedAmount)
	{
		this._asteroidForce -= slowForceAmount; // Slow down the enemy when shot
		if (this._asteroidForce < this._forceThreshold) this._asteroidForce = this._forceThreshold; // Prevents speed from going below threshold (0.85f)

		this._asteroidRotationSpeed -= slowRotateSpeedAmount; // Slow down the rotation speed of the asteroid
		if (this._asteroidRotationSpeed < this._rotateSpeedThreshold)
			this._asteroidRotationSpeed = this._rotateSpeedThreshold; // Prevents rotation speed from going too low

		if (!this._hasResetSettings) StartCoroutine(ResetAsteroidSettings()); // Start the coroutine to reset asteroid settings
	}
	#endregion

	private IEnumerator OnAsteroidShot()
	{
		//Reset the isShot boolean if the asteroid is shot again before the wait time is over

		this._isShot = true;
		yield return new WaitForSeconds(this._waitTimeUntilMove);
		this._isShot = false;
	}

	#region Other Helper Methods
	private IEnumerator ResetAsteroidSettings()
	{
		this._hasResetSettings = true;
		yield return new WaitUntil(() => !this._isShot); // Wait until the asteroid is not shot anymore

		this._asteroidForce = GetBaseAsteroidForce(); // Reset to base speed when not shot
		this._asteroidRotationSpeed = GetBaseAsteroidRotationSpeed(); // Reset to base rotation speed when not shot

		this._hasResetSettings = false; // Reset the flag after the coroutine is done
	}
	private void TriggerOnAsteroidShot()
	{
		if (this._isShot) StopCoroutine(OnAsteroidShot());
		StartCoroutine(OnAsteroidShot());
	}

	private float GetBaseAsteroidForce()
	{
		switch (this._asteroidType)
		{
			case AsteroidType.Cosmic: return this._cosmicAsteroidForce;
			case AsteroidType.Dwarf: return this._dwarfAsteroidForce;
			default: return 0f; // Default case if no type matches
		}
	}

	private float GetBaseAsteroidRotationSpeed()
	{
		switch (this._asteroidType)
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
			TriggerOnAsteroidShot(); // Start the coroutine to handle slow mode
			iDamageable.OnDamaged(collisionBM.GetPlayerBulletDamage());

			Vector3 spawnPos = new Vector3(this.transform.position.x, this.transform.position.y + 0.3f);
			UIManager.instance.SpawnDamageText(spawnPos, (int) collisionBM.GetPlayerBulletDamage());
		}
	}

	#region IDamageable Interface Implementation
	public void OnDamaged(float damageAmount)
	{
		this._currentHealth -= damageAmount;
		if (this._currentHealth <= 0) Destroy(gameObject); // Destroy asteroid when health reaches zero

		if (this._player.GetPlayerType() == Player.PlayerType.Sora)
			HandleSlowMode(this._slowForceAmount, this._slowRotateSpeedAmount);
	}
	#endregion
}
