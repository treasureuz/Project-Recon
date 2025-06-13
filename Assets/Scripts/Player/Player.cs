using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IDamageable
{
	[Header("References")]
	[SerializeField] private Rigidbody2D _rb2d;
	[SerializeField] private AsteroidBehavior _asteroid;
	[SerializeField] private PWeaponManager _playerWeaponManager;

	[Header("Global Player Settings")]
	[SerializeField] private float _rotationDuration = 0.072f; //How long to rotate towards mouse position

	#region Player Settings
	[Header("Standard Player Settings")]
	[SerializeField] private float _standardMaxHealth = 100f; // Max health of the player
	[SerializeField] private float _standardMoveSpeed = 3f;

	[Header("Omen's Player Settings")]
	[SerializeField] private float _omenMaxHealth = 125f; // Max health of the player
	[SerializeField] private float _omenMoveSpeed = 3.3f;

	[Header("Sora's Player Settings")]
	[SerializeField] private float _soraMaxHealth = 155f; // Max health of the player
	[SerializeField] private float _soraMoveSpeed = 3.65f;

	[Header("Ralph's Player Settings")]
	[SerializeField] private float _ralphMaxHealth = 200f; // Max health of the player
	[SerializeField] private float _ralphMoveSpeed = 4f;
	#endregion

	// Forces z axis to be 0
	private Vector2 _mousePosition;
	private Vector2 _direction;

	private float _currentHealth;
	private float _moveSpeed;

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
		this._playerType = PlayerType.Standard;
		this._playerWeaponManager.AddThrusterTypeToStack(PWeaponManager.ThrusterType.Normal);
		HandlePlayerSwitch();
	}

	private void Update()
	{
		HandlePlayerRotation();
	}

	private void FixedUpdate()
	{
		this._rb2d.linearVelocity = InputManager._moveDirection * this._moveSpeed;
	}

	#region Player Settings
	private void Standard()
	{
		this._currentHealth = this._standardMaxHealth;
		this._moveSpeed = this._standardMoveSpeed;
		this.transform.localScale = new Vector3(0.925f, 0.925f, 0.925f);
	}

	private void Omen()
	{
		this._currentHealth = this._omenMaxHealth;
		this._moveSpeed = this._omenMoveSpeed;
		this.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
	}

	private void Sora()
	{
		this._currentHealth = this._soraMaxHealth;
		this._moveSpeed = this._soraMoveSpeed;
		this.transform.localScale = new Vector3(0.985f, 0.985f, 0.985f);
	}

	private void Ralph()
	{
		this._currentHealth = this._ralphMaxHealth;
		this._moveSpeed = this._ralphMoveSpeed;
		this.transform.localScale = new Vector3(1.035f, 1.035f, 1.035f);
	}
	#endregion

	#region Player Actions
	private void HandlePlayerRotation()
	{
		this._mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

		// Checks if direction is positive (mouse position is to the right) or
		// negative (mouse position is to the left)
		this._direction = (this._mousePosition - (Vector2)this.transform.position).normalized;

		float angle = Vector3.SignedAngle(this.transform.right, this._direction, Vector3.forward);

		//Rotate smoothly/step by step
		float t = Time.deltaTime / this._rotationDuration; // a fraction of the total angle you want to rotate THIS frame
		this.transform.Rotate(Vector3.forward, angle * t);

		// Rotate the weapon to face the mouse position (Does similarly thing as above)
		// this.transform.right = Vector3.Slerp(this.transform.right, this._direction, Mathf.Clamp01(t));
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
		this._playerWeaponManager.HandleThrusterAndBulletInstantiation();

		UIManager.instance.UpdateHealthText();
	}
	#endregion

	private void OnOrbsCollect(Collider2D collision)
	{
		if (collision.CompareTag("OmensTriangle"))
		{
			this._playerType = PlayerType.Omen;
			this._playerWeaponManager.AddThrusterTypeToStack(PWeaponManager.ThrusterType.Thin); //Omen's thruster type
		}
		else if (collision.CompareTag("SorasOrb"))
		{
			this._playerType = PlayerType.Sora;
			this._playerWeaponManager.AddThrusterTypeToStack(PWeaponManager.ThrusterType.Wide); //Sora's thruster type
		}
		else if (collision.CompareTag("RalphsCube"))
		{
			this._playerType = PlayerType.Ralph;
			this._playerWeaponManager.AddThrusterTypeToStack(PWeaponManager.ThrusterType.Double); //Ralph's thruster type
		}

		HandlePlayerSwitch();
		Destroy(collision.gameObject);
	}

	#region Setters and Getters
	private void SetPlayerType()
	{
		switch (this._playerWeaponManager.GetThrusterType())
		{
			case PWeaponManager.ThrusterType.Normal: this._playerType = PlayerType.Standard; break;
			case PWeaponManager.ThrusterType.Thin: this._playerType = PlayerType.Omen; break;
			case PWeaponManager.ThrusterType.Wide: this._playerType = PlayerType.Sora; break;
			case PWeaponManager.ThrusterType.Double: this._playerType = PlayerType.Ralph; break;
		}
	}

	public float GetCurrentHealth()
	{
		return this._currentHealth;
	}

	public float GetMaxHealth()
	{
		switch (this._playerType)
		{
			case PlayerType.Standard: return this._standardMaxHealth;
			case PlayerType.Omen: return this._omenMaxHealth;	
			case PlayerType.Sora: return this._soraMaxHealth;
			case PlayerType.Ralph: return this._soraMaxHealth;
			default: return 0f;
		}
	}
	#endregion

	private void DestroyOnDie()
	{
		switch (this._playerType)
		{
			case PlayerType.Standard: //Thruster Type - Normal
				this._playerWeaponManager.PopAndSetThrusterType(); //First, remove from stack,
				SetPlayerType(); //Set player type based on current thruster type,
				Destroy(this.gameObject); break;//Then, destroy in game	

			default: //Every other Thruster Type - Thin, Wide, Double
				this._playerWeaponManager.PopAndSetThrusterType(); //First, remove from stack,
				SetPlayerType(); //Set player type based on current thruster type,
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
			Debug.Log("Player hit by Asteroid: " + collision.GetComponent<AsteroidBehavior>().GetAsteroidDamage());
		} 
		else if (collision.CompareTag("EnemyBullet"))
		{
			//Bullet damage is specific to THIS bullet/collision's character type
			iDamageable.OnDamaged(collision.GetComponent<EBulletManager>().GetEnemyBulletDamage());
			Debug.Log("Player hit: " + collision.GetComponent<EBulletManager>().GetEnemyBulletDamage());
		}
		else if (collision.gameObject.layer == LayerMask.NameToLayer("Orbs"))
		{
			OnOrbsCollect(collision);
		}
	}

	#region IDamageable Interface Implementation
	public void OnDamaged(float damageAmount)
	{
		Debug.Log("Called OnDamaged!");
		this._currentHealth -= damageAmount;

		UIManager.instance.UpdateHealthText();

		if (this._currentHealth <= 0)
		{
			DestroyOnDie();
		}
	}
	#endregion
}
