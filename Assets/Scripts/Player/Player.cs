using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IDamageable
{
	[Header("References")]
	[SerializeField] private Rigidbody2D _rb2d;
	[SerializeField] private GameObject _playerClonePrefab;
	[SerializeField] private GameObject _playerOverlayPrefab;
	[SerializeField] private GameObject _playerTeleporterPrefab;
	[SerializeField] private PWeaponManager _playerWeaponManager;

	[Header("Global Player Settings")]
	[SerializeField] private static float _rotationDuration = 0.072f; //How long to rotate towards mouse position

	#region Player Settings
	[Header("Standard Player Settings")]
	[SerializeField] private Vector3 _standardScale = new Vector3(0.855f, 0.855f, 0.855f);
	[SerializeField] private int _standardMaxHealth = 100;
	[SerializeField] private float _standardMoveSpeed = 3f;

	[Header("Omen's Player Settings")]
	[SerializeField] private Vector3 _omenScale = new Vector3(0.875f, 0.875f, 0.875f);
	[SerializeField] private int _omenMaxHealth = 125;
	[SerializeField] private float _omenMoveSpeed = 3.3f;
	[SerializeField] private float _omensCloneCooldown = 5f; // Cooldown for Omen's clone ability

	[Header("Sora's Player Settings")]
	[SerializeField] private Vector3 _soraScale = new Vector3(0.9f, 0.9f, 0.9f);
	[SerializeField] private int _soraMaxHealth = 155;
	[SerializeField] private float _soraMoveSpeed = 3.65f;
	[SerializeField] private float _sorasFreezeCooldown = 12f; // Cooldown for Sora's freeze ability
	[SerializeField] private float _sorasFreezeDuration = 6.89f; // Duration of Sora's freeze ability

	[Header("Ralph's Player Settings")]
	[SerializeField] private Vector3 _ralphScale = new Vector3(0.93f, 0.93f, 0.93f);
	[SerializeField] private int _ralphMaxHealth = 200;
	[SerializeField] private float _ralphMoveSpeed = 4f;
	[SerializeField] private float _ralphsTeleportCooldown = 15f; // Cooldown for Ralph's teleport ability
	[SerializeField] private float _ralphsInvincibilityCount = 400f; // Amount of invincibility Ralph has on start
	#endregion

	private EnemyHelper _enemyHelper;
	private AsteroidSpawnManager _asteroidSpawnManager; // Reference to the AsteroidSpawnManager

	private GameObject _playerCloneInstance; // Player clone instance for Omen's ability
	private GameObject _playerOverlayInstance;
	private GameObject _playerTeleportInstance; // Player teleport object for Ralph's ability

	private List<GameObject> _playerList = new List<GameObject>(); // List of players in the game

	#region Player Settings Variables
	private float _currentHealth;
	private float _moveSpeed;
	private float _cooldown;

	private float _ralphsMaxInvincibilityCount = 400f; // Maximum invincibility count for Ralph

	private bool _isSoraFreezeActive = false; // Indicates if Sora's freeze ability is active
	private bool _canTeleport = false; // Indicates if Ralph can teleport
	private bool _isInTeleport = false; // Indicates if the player is currently teleporting
	private bool _isRalphInvincibilityActive = false; // Indicates if Ralph's invincibility is active
	#endregion

	public enum PlayerType
	{
		Standard = 0, // Normal
		Omen = 1, // Thin
		Sora = 2, // Wide
		Ralph = 3 // Double
	}

	private PlayerType _playerType;

	private void Awake()
	{
		this._enemyHelper = FindAnyObjectByType<EnemyHelper>(); // Finds the active EnemyHelper in the scene
		this._asteroidSpawnManager = FindAnyObjectByType<AsteroidSpawnManager>(); // Finds the active AsteroidSpawnManager in the scene
	}

	private void Start()
	{
		this.transform.eulerAngles = new Vector3(0, 0, 0); // Reset rotation to default
		AddPlayerToList(this.gameObject); // Add this player to the list
		this._playerType = PlayerType.Standard;
		this._playerWeaponManager.AddThrusterTypeToStack(PWeaponManager.ThrusterType.Normal);
		HandlePlayerSwitch();
	}

	private void Update()
	{
		HandlePlayerRotation();
		if (InputManager.instance.GetIsAbilityEPressed()) HandlePlayerAbilities();
		if (this._playerType == PlayerType.Ralph) HandleRalphsInvincibility(); // Handles Ralph's invincibility ability

		#region UI Updates
		UIManager.instance.UpdateCooldownText(GetCurrentCooldownTime());
		UIManager.instance.UpdateRalphsInvincibilityText((int) this._ralphsInvincibilityCount);
		#endregion
	}

	private void FixedUpdate()
	{
		this._rb2d.linearVelocity = InputManager.moveDirection * this._moveSpeed; //Handles Player Movement
	}

	#region PlayerType Settings
	private void Standard()
	{
		this._currentHealth = this._standardMaxHealth;
		this._moveSpeed = this._standardMoveSpeed;
		this.transform.localScale = this._standardScale;
	}

	private void Omen()
	{
		this._currentHealth = this._omenMaxHealth;
		this._moveSpeed = this._omenMoveSpeed;
		this._cooldown = this._omensCloneCooldown; 
		this.transform.localScale = this._omenScale;
	}

	private void Sora()
	{
		this._currentHealth = this._soraMaxHealth;
		this._moveSpeed = this._soraMoveSpeed;
		this._cooldown = this._sorasFreezeCooldown; // Set cooldown for Sora's freeze ability
		this.transform.localScale = this._soraScale;
	}

	private void Ralph()
	{
		this._currentHealth = this._ralphMaxHealth;
		this._moveSpeed = this._ralphMoveSpeed;
		this._cooldown = this._ralphsTeleportCooldown; // Set cooldown for Ralph's teleport ability
		this._ralphsInvincibilityCount = this._ralphsMaxInvincibilityCount; // Reset Ralph's invincibility count
		this.transform.localScale = this._ralphScale;
	}
	#endregion

	#region Player Abilities
	private void OmensClone()
	{
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		
		if (Time.time >= this._cooldown)
		{
			if (InputManager.instance.GetWasAbilityEPressedThisFrame() && this._playerOverlayInstance == null)
			{
				this._playerOverlayInstance = Instantiate(this._playerOverlayPrefab, mousePosition, Quaternion.identity);
			}
			else if (InputManager.instance.GetWasAbilityEPressedThisFrame() && this._playerOverlayInstance != null)
			{
				InputManager.instance.SetIsAbilityEPressedToFalse();
				Destroy(this._playerOverlayInstance); // Destroy overlay instance if it exists
				this._playerOverlayInstance = null; // Reset overlay instance
			}

			#region Handle Player Overlay Local Scale
			if (this._playerOverlayInstance != null)
			{
				this._playerOverlayInstance.transform.localScale = this.transform.localScale; // Match player scale

				if (this.transform.localScale.y == -Mathf.Abs(this.transform.localScale.y)) 
					this._playerOverlayInstance.transform.eulerAngles = new Vector3(0, 0, 180); // Flip overlay if player is facing left
				else this._playerOverlayInstance.transform.eulerAngles = new Vector3(0, 0, 0); // Keep overlay upright if player is facing right

				this._playerOverlayInstance.transform.position = mousePosition;
			}
			#endregion

			if (Mouse.current.rightButton.wasPressedThisFrame && this._playerOverlayInstance != null)
			{
				this._playerCloneInstance = Instantiate
				(this._playerClonePrefab, this._playerOverlayInstance.transform.position, Quaternion.identity);

				InputManager.instance.SetIsAbilityEPressedToFalse(); // Reset the ability pressed state

				Destroy(this._playerOverlayInstance); // Destroy the overlay after spawning the player clone
				this._playerOverlayInstance = null; // Reset the overlay instance

				this._cooldown = Time.time + this._omensCloneCooldown; // Reset cooldown
			}
		}
		else InputManager.instance.SetIsAbilityEPressedToFalse(); // Reset the ability pressed state if cooldown is active
	}

	private IEnumerator SorasFreeze()
	{
		if (this._isSoraFreezeActive) yield break; // Prevents multiple calls to this coroutine

		if (Time.time >= this._cooldown)
		{
			this._isSoraFreezeActive = true;

			// Disable/Freeze Enemies and their Weapon Managers
			foreach (Enemy enemy in this._enemyHelper.GetEnemyList())
			{
				enemy.enabled = false; // Disable enemy behavior
				enemy.GetComponentInChildren<EWeaponManager>().enabled = false; // Disable enemy weapon manager
			}
			// Disable/Freeze asteroid spawning
			this._asteroidSpawnManager.enabled = false; // Disable asteroid spawning

			yield return new WaitForSeconds(this._sorasFreezeDuration); // Wait for the freeze duration (6.89s)

			this._isSoraFreezeActive = false;

			// Re-enable Enemies and their Weapon Managers
			foreach (Enemy enemy in this._enemyHelper.GetEnemyList())
			{
				enemy.enabled = true; 
				enemy.GetComponentInChildren<EWeaponManager>(true).enabled = true;
			}
			// Re-enable asteroid spawning;
			this._asteroidSpawnManager.enabled = true; 

			this._cooldown = Time.time + this._sorasFreezeCooldown;
		}
		else InputManager.instance.SetIsAbilityEPressedToFalse(); // Reset the ability pressed state if cooldown is active
	}

	#region Ralph's Abilities
	private void RalphsTeleport()
	{
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

		if (Time.time >= this._cooldown)
		{
			if (InputManager.instance.GetWasAbilityEPressedThisFrame() && this._playerTeleportInstance == null)
			{
				this._playerTeleportInstance = Instantiate(this._playerTeleporterPrefab, mousePosition, Quaternion.identity);
			}
			else if (InputManager.instance.GetWasAbilityEPressedThisFrame() && this._playerTeleportInstance != null)
			{
				InputManager.instance.SetIsAbilityEPressedToFalse(); // Reset the ability pressed state
				Destroy(this._playerTeleportInstance); // Destroy teleport instance if it exists
				this._playerTeleportInstance = null; // Reset teleport instance
			}

			if (this._playerTeleportInstance != null && !this._canTeleport)
			{
				this._playerTeleportInstance.transform.position = mousePosition; // Update teleport instance position
				if (Mouse.current.rightButton.wasPressedThisFrame)
				{
					Vector2 newMousePos = new Vector2(mousePosition.x, mousePosition.y);
					this._playerTeleportInstance.transform.position = newMousePos; // Set teleport position to mouse position
					this._canTeleport = true; // Allow teleportation
				}
			}
			else if (Mouse.current.rightButton.wasPressedThisFrame && this._canTeleport)
			{
				StartCoroutine(Teleport(this._playerTeleportInstance.transform.position, 0.55f)); // Teleport player to the teleport instance position
				
				InputManager.instance.SetIsAbilityEPressedToFalse(); // Reset the ability pressed state
				this._canTeleport = false; this._isInTeleport = false; // Reset teleport states

				Destroy(this._playerTeleportInstance); // Destroy teleport instance after teleporting
				this._playerTeleportInstance = null; // Reset teleport instance

				this._cooldown = Time.time + this._ralphsTeleportCooldown;
			}
		}
		else InputManager.instance.SetIsAbilityEPressedToFalse(); // Reset the ability pressed state if cooldown is active
	}
	private void HandleRalphsInvincibility()
	{
		if (HasInvincibilityEnergy() && InputManager.instance.GetIsAbilityCPressed())
		{
			this._isRalphInvincibilityActive = true; // Set invincibility active
			// Decrease invincibility energy by 0.5 per frame
			this._ralphsInvincibilityCount = Mathf.Max(0, this._ralphsInvincibilityCount - 0.5f); // Ensures the count does not go negative
		}
		else
		{
			this._isRalphInvincibilityActive = false; // Reset invincibility active 
			// Regenerate invincibility energy by 0.0833 per frame -- 5 per second. 
			this._ralphsInvincibilityCount = Mathf.Min
			(this._ralphsMaxInvincibilityCount, this._ralphsInvincibilityCount + 0.0833f); //Ensures the count does not exceed the maximum
		}
	}
	#endregion
	#endregion

	#region Player Handlers
	private void HandlePlayerRotation()
	{
		if (this._isInTeleport) return; // Prevents rotation while teleporting
		Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		//Checks if direction is positive (mouse position is to the right) or negative (mouse position is to the left)
		Vector2 direction = (mousePosition - (Vector2)this.transform.position).normalized;

		float angle = Vector3.SignedAngle(this.transform.right, direction, Vector3.forward);

		//Rotate smoothly/step by step
		float t = Time.deltaTime / _rotationDuration; // a fraction of the total angle/rotation (THIS frame)
		this.transform.Rotate(Vector3.forward, angle * t);

		//Normalizes angle for scale flipping (eulerAngles.z can't be negative -- 0 to 360 degrees)
		float zAngle = this.transform.eulerAngles.z;
		if (zAngle > 180f) zAngle -= 360f;

		Vector3 localScale = this.transform.localScale;

	    // Flip the player sprite vertically when facing left
		if (zAngle > 120f || zAngle < -100f) localScale.y = -Mathf.Abs(localScale.y);
		else localScale.y = Mathf.Abs(localScale.y); // Keep the player sprite upright when facing right

		this.transform.localScale = localScale; // Apply the scale to the player
	}

	private void HandlePlayerSwitch()
	{
		switch (this._playerType)
		{
			case PlayerType.Standard: Standard(); break; //Thruster Type - Normal
			case PlayerType.Omen: Omen(); break; //Thruster Type - Thin
			case PlayerType.Sora: Sora(); break; //Thruster Type - Wide
			case PlayerType.Ralph: Ralph(); break; //Thruster Type - Double
		}
		this._playerWeaponManager.InstantiateThrusterAndBullet();
		if (this._playerCloneInstance != null)
		{
			RemovePlayerFromList(this._playerCloneInstance); //Remove clone from player list
			Destroy(this._playerCloneInstance); //Destroy clone if exists
		}

		UIManager.instance.UpdateHealthText(GetCurrentHealth());
	}

	private void HandlePlayerAbilities()
	{
		if (this._playerType == PlayerType.Omen) OmensClone();
		else if (this._playerType == PlayerType.Sora) StartCoroutine(SorasFreeze());
		else if (this._playerType == PlayerType.Ralph)
		{
			RalphsTeleport();
		}
	}
	#endregion

	private void OnOrbsCollect(Collider2D collision)
	{
		if (collision.CompareTag("OmensTriangle"))
		{
			this._playerWeaponManager.AddThrusterTypeToStack(PWeaponManager.ThrusterType.Thin); //Omen's thruster type
		}
		else if (collision.CompareTag("SorasOrb"))
		{ 
			this._playerWeaponManager.AddThrusterTypeToStack(PWeaponManager.ThrusterType.Wide); //Sora's thruster type
		}
		else if (collision.CompareTag("RalphsCube"))
		{
			this._playerWeaponManager.AddThrusterTypeToStack(PWeaponManager.ThrusterType.Double); //Ralph's thruster type
		}

		SetPlayerTypeWithThruster();
		HandlePlayerSwitch();
		Destroy(collision.gameObject);
	}

	#region Other Helper Methods
	private IEnumerator Teleport(Vector2 targetPosition, float duration)
	{
		float elapsedTime = 0f;
		Vector2 startPosition = this.transform.position;
		this._isInTeleport = true; // Set teleport state to true

		while (elapsedTime < duration)
		{
			float t = elapsedTime / duration;
			this.transform.position = Vector2.Lerp(startPosition, targetPosition, t); // Animation is done when elapsed time >= duration
			yield return null; // Waits for next frame then continues the loop
			elapsedTime += Time.deltaTime;
		}
	}

	private void SetPlayerTypeWithThruster()
	{
		switch (this._playerWeaponManager.GetThrusterType())
		{
			case PWeaponManager.ThrusterType.Normal: this._playerType = PlayerType.Standard; break;
			case PWeaponManager.ThrusterType.Thin: this._playerType = PlayerType.Omen; break;
			case PWeaponManager.ThrusterType.Wide: this._playerType = PlayerType.Sora; break;
			case PWeaponManager.ThrusterType.Double: this._playerType = PlayerType.Ralph; break;
		}
	}

	private bool HasInvincibilityEnergy()
	{
		return this._ralphsInvincibilityCount > 0;
	}
	#endregion

	#region Setters/Adders and Getters
	public PlayerType GetPlayerType()
	{
		return this._playerType;
	}
	public void AddPlayerToList(GameObject newPlayer)
	{
		if (!this._playerList.Contains(newPlayer))
		{
			this._playerList.Add(newPlayer);
		}
	}
	public void RemovePlayerFromList(GameObject playerToRemove)
	{
		if (this._playerList.Contains(playerToRemove))
		{
			this._playerList.Remove(playerToRemove);
		}
	}
	public List<GameObject> GetPlayerList()
	{
		return this._playerList;
	}
	public int GetCurrentHealth()
	{
		return (int) this._currentHealth;
	}
	public int GetMaxHealth()
	{
		switch (this._playerType)
		{
			case PlayerType.Standard: return this._standardMaxHealth;
			case PlayerType.Omen: return this._omenMaxHealth;	
			case PlayerType.Sora: return this._soraMaxHealth;
			case PlayerType.Ralph: return this._soraMaxHealth;
			default: return 0;
		}
	}
	public float GetMoveSpeed()
	{
		return this._moveSpeed;
	}
	public int GetCurrentCooldownTime()
	{
		return (int) this._cooldown - (int) Time.time;
	}
	public int GetBaseCooldownTime()
	{
		switch (this._playerType)
		{
			case PlayerType.Omen: return (int) this._omensCloneCooldown;
			case PlayerType.Sora: return (int) this._sorasFreezeCooldown;
			case PlayerType.Ralph: return (int) this._ralphsTeleportCooldown;
			default: return 0;
		}
	}
	public bool GetIsSoraFreezeActive()
	{
		return this._isSoraFreezeActive;
	}
	#endregion

	private void DestroyOnDie()
	{
		switch (this._playerType)
		{
			case PlayerType.Standard: //Thruster Type - Normal
				this._playerWeaponManager.PopAndSetThrusterType(); //First, remove from stack,

				SetPlayerTypeWithThruster(); //Set player type based on current thruster type,

				RemovePlayerFromList(this.gameObject);
				Destroy(this.gameObject); break; //Then, destroy in game	

			default: //Every other Thruster Type - Thin, Wide, Double
				if (this._playerCloneInstance != null)
				{
					RemovePlayerFromList(this._playerCloneInstance); //Remove clone from player list
					Destroy(this._playerCloneInstance); //Destroy clone if exists
				}
				
				this._playerWeaponManager.PopAndSetThrusterType(); //First, remove from stack,

				SetPlayerTypeWithThruster(); //Set player type based on current thruster type,

				Destroy(this._playerWeaponManager.GetThrusterInstance()); //Then, destroy in game
				HandlePlayerSwitch(); break;
		}
	}
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
		else if (collision.gameObject.layer == LayerMask.NameToLayer("Orbs"))
		{
			OnOrbsCollect(collision);
		}
	}

	#region IDamageable Interface Implementation
	public void OnDamaged(float damageAmount)
	{
		if (!this._isRalphInvincibilityActive)
		{
			this._currentHealth -= damageAmount;
		}
		
		UIManager.instance.UpdateHealthText(GetCurrentHealth());

		if (this._currentHealth <= 0) DestroyOnDie();
	}	
	#endregion
}
