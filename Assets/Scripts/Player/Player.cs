using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour, IDamageable
{
	[Header("References")]
	[SerializeField] private Rigidbody2D _rb2d;
	[SerializeField] private AsteroidBehavior _asteroid;
	[SerializeField] private BulletManager _bulletManager;
	[SerializeField] private PWeaponManager _playerWeaponManager;

	[Header("Standard Player Settings")]
	[SerializeField] private float _standardMaxHealth = 100f; // Max health of the player
	[SerializeField] private float _standardMoveSpeed = 3f;
	[SerializeField] private float _standardBulletDamage = 15f;
	[SerializeField] private float _standardTimeBetweenShots = 0.45f;

	[Header("Omen's Player Settings")]
	[SerializeField] private float _omenMaxHealth = 125f; // Max health of the player
	[SerializeField] private float _omenMoveSpeed = 3.3f;
	[SerializeField] private float _omenBulletDamage = 22.5f;
	[SerializeField] private float _omenTimeBetweenShots = 0.38f;

	[Header("Sora's Player Settings")]
	[SerializeField] private float _soraMaxHealth = 155f; // Max health of the player
	[SerializeField] private float _soraMoveSpeed = 3.65f;
	[SerializeField] private float _soraBulletDamage = 30f;
	[SerializeField] private float _soraTimeBetweenShots = 0.265f;

	[Header("Ralph's Player Settings")]
	[SerializeField] private float _ralphMaxHealth = 200f; // Max health of the player
	[SerializeField] private float _ralphMoveSpeed = 4f;
	[SerializeField] private float _ralphBulletDamage = 40f;
	[SerializeField] private float _ralphTimeBetweenShots = 0.125f;


	// Forces z axis to be 0
	private Vector2 _mousePosition;
	private Vector2 _direction;

	private float _currentHealth;
	private float _moveSpeed;

	private void Awake()
	{
		this._playerWeaponManager.AddThrusterTypeToStack(PWeaponManager.ThrusterType.Normal);
		HandlePlayerSwitch();
	}

	private void FixedUpdate()
	{
		this._rb2d.linearVelocity = InputManager._moveDirection * this._moveSpeed;
		HandlePlayerRotation();
	}

	private void HandlePlayerRotation()
	{
		this._mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

		// Checks if direction is positive (mouse position is to the right) or
		// negative (mouse position is to the left)
		this._direction = (this._mousePosition - (Vector2)this.transform.position);

		float angle = Vector3.SignedAngle(this.transform.right, this._direction, Vector3.forward);

		//Rotate smoothly/step by step
		float t = Time.fixedDeltaTime / 0.082f; // a fraction of the total angle you want to rotate THIS frame
		this.transform.Rotate(Vector3.forward, angle * t);

		// Rotate the weapon to face the mouse position (Does similarly thing as above)
		// this.transform.right = Vector3.Slerp(this.transform.right, this._direction, Mathf.Clamp01(t));
	}

	private void HandlePlayerSwitch()
	{
		//Set bullet character type to player so I can get the correct bullet damage
		this._bulletManager.SetBulletCharacterType(GameManager.CharacterType.Player);

		switch (this._playerWeaponManager.GetThrusterType())
		{
			case PWeaponManager.ThrusterType.Normal: //Standard
				this._currentHealth = this._standardMaxHealth;
				this._moveSpeed = this._standardMoveSpeed;
				this.transform.localScale = new Vector3(0.68f, 0.68f, 0.68f);
				this._bulletManager.SetBulletDamage(this._standardBulletDamage);
				this._playerWeaponManager.SetTimeBetweenPlayerShots(this._standardTimeBetweenShots);
				break;

			case PWeaponManager.ThrusterType.Thin: //Omen
				this._currentHealth = this._omenMaxHealth;
				this._moveSpeed = this._omenMoveSpeed;
				this.transform.localScale = new Vector3(0.73f, 0.73f, 0.73f);
				this._bulletManager.SetBulletDamage(this._omenBulletDamage);
				this._playerWeaponManager.SetTimeBetweenPlayerShots(this._omenTimeBetweenShots);
				break;
			case PWeaponManager.ThrusterType.Wide: //Sora
				this._currentHealth = this._soraMaxHealth;
				this._moveSpeed = this._soraMoveSpeed;
				this.transform.localScale = new Vector3(0.775f, 0.775f, 0.775f);
				this._bulletManager.SetBulletDamage(this._soraBulletDamage);
				this._playerWeaponManager.SetTimeBetweenPlayerShots(this._soraTimeBetweenShots);
				break;
			case PWeaponManager.ThrusterType.Double: //Ralph
				this._currentHealth = this._ralphMaxHealth;
				this._moveSpeed = this._ralphMoveSpeed;
				this.transform.localScale = new Vector3(0.825f, 0.825f, 0.825f);
				this._bulletManager.SetBulletDamage(this._ralphBulletDamage);
				this._playerWeaponManager.SetTimeBetweenPlayerShots(this._ralphTimeBetweenShots);
				break;
		}

		this._playerWeaponManager.HandleThrusterInstantiation();
	}

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
		if (collision.CompareTag("Asteroid"))
		{
			iDamageable.OnDamaged(this._asteroid.getAsteroidDamage()); 
		} 
		else if (collision.CompareTag("Bullet"))
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
		if (this._playerWeaponManager.GetThrusterTypeIndex() == 0) //If index is "NormalThrusterType" index
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
		this._currentHealth -= damageAmount;
		Debug.Log("Current Health: " + this._currentHealth);
		if (this._currentHealth <= 0)
		{
			DestroyOnDie();
		}
	}

	#endregion
}
