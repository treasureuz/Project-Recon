using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class Player : MonoBehaviour, IDamageable
{
	[Header("References")]
	[SerializeField] private Rigidbody2D _rb2d;
	[SerializeField] private AsteroidBehavior _asteroid;
	[SerializeField] private BulletManager _bulletManager;
	[SerializeField] private PWeaponManager _playerWeaponManager;

	[Header("Global Player Settings")]
	[SerializeField] private float _rotationDuration = 0.072f; //How long to rotate towards mouse position

	#region Serialized Player Settings Fields
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
		this.transform.localScale = new Vector3(0.61f, 0.61f, 0.61f);
	}

	private void Omen()
	{
		this._currentHealth = this._omenMaxHealth;
		this._moveSpeed = this._omenMoveSpeed;
		this.transform.localScale = new Vector3(0.635f, 0.635f, 0.635f);
	}

	private void Sora()
	{
		this._currentHealth = this._soraMaxHealth;
		this._moveSpeed = this._soraMoveSpeed;
		this.transform.localScale = new Vector3(0.665f, 0.665f, 0.665f);
	}

	private void Ralph()
	{
		this._currentHealth = this._ralphMaxHealth;
		this._moveSpeed = this._ralphMoveSpeed;
		this.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
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
		this._playerWeaponManager.HandleThrusterInstantiation();
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

		HandlePlayerSwitch();
		Destroy(collision.gameObject);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		IDamageable iDamageable = GetComponent<IDamageable>();
		BulletManager bullet = collision.GetComponent<BulletManager>();

		if (collision.CompareTag("Asteroid"))
		{
			iDamageable.OnDamaged(this._asteroid.getAsteroidDamage()); 
		} 
		//Adds clarity as it confirms the bullet/collision was shot from the enemy 
		else if (collision.CompareTag("Bullet") && bullet.GetBulletCharacterType() == GameManager.CharacterType.Enemy)
		{
			//Bullet damage is specific to THIS bullet/collision's character type
			iDamageable.OnDamaged(collision.GetComponent<BulletManager>().GetBulletDamage());
			Debug.Log("Player hit: " + collision.GetComponent<BulletManager>().GetBulletDamage());
		}
		else if (collision.gameObject.layer == LayerMask.NameToLayer("Orbs"))
		{
			OnOrbsCollect(collision);
		}
	}

	private void DestroyOnDie()
	{
		if (this._playerType == PlayerType.Standard) //Or if ThrusterType index is "Normal"
		{
			this._playerWeaponManager.PopAndSetThrusterType(); //First, remove from stack
			Destroy(this.gameObject); //Then, destroy in game	
		}
		else
		{
			this._playerWeaponManager.PopAndSetThrusterType(); //First, remove from stack
			Destroy(this._playerWeaponManager.GetThrusterInstance()); //Then, destroy in game

			HandlePlayerSwitch();
		}
	}

	#region IDamageable Interface Implementation
	public void OnDamaged(float damageAmount)
	{
		Debug.Log("Called OnDamaged!");
		this._currentHealth -= damageAmount;
		Debug.Log("Current Health: " + this._currentHealth);
		if (this._currentHealth <= 0)
		{
			DestroyOnDie();
		}
	}
	#endregion
}
