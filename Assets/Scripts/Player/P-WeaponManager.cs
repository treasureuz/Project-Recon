using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PWeaponManager : MonoBehaviour
{
	[Header("Bullet References")]
	[SerializeField] private GameObject _bulletPrefab;
	[SerializeField] private Transform _bulletSpawnPoint;
	[SerializeField] private Transform _leftBulletSpawnPoint;
	[SerializeField] private Transform _rightBulletSpawnPoint;
	[SerializeField] private Transform _thrusterParent;

	[Header("Weapon Settings")]
	[SerializeField] private float _timeBetweenPlayerShots = 0.5f;

	[Header("Thruster Types")]
	[SerializeField] private GameObject _normalPrefab;
	[SerializeField] private GameObject _thinPrefab;
	[SerializeField] private GameObject _widePrefab;

	[Header("Double Thrusters")]
	[SerializeField] private GameObject _doublePrefab;
	[SerializeField] private GameObject _leftPrefab;
	[SerializeField] private GameObject _rightPrefab;

	private Stack<ThrusterType> _thrusterTypeStack = new Stack<ThrusterType>();

	private GameObject _bulletInstance;
	private GameObject _leftBulletInstance;
	private GameObject _rightBulletInstance;
	private GameObject _thrusterInstance;

	private float _nextShootTime = 0f;

	public enum ThrusterType
	{
		Normal = 0, // Standard 
		Thin = 1, // Omen
		Wide = 2, // Sora
		Double = 3, // Ralph
	}

	private ThrusterType thrusterType;

	private void Start()
	{
		this.thrusterType = ThrusterType.Normal;
	}

	private void Update()
	{
		//HandleWeaponRotation();
		HandlePlayerShoot();
		
		
	}

	#region HandleWeaponRotation
	//private void HandleWeaponRotation()
	//{
	//	this._mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

	//	// Checks if direction is positive (mouse position is to the right) or
	//	// negative (mouse position is to the left)
	//	this._direction = (this._mousePosition - (Vector2)this.transform.position).normalized;

	//	float angle = Vector3.SignedAngle(this.transform.right, this._direction, Vector3.forward);

	//	//Rotate smoothly/step by step
	//	float t = Time.fixedDeltaTime / _rotationDuration;
	//	this.transform.Rotate(Vector3.forward, angle * t);

	//	// Rotate the weapon to face the mouse position (Does similarly thing as above)
	//	//this.transform.right = Vector3.Slerp(this.transform.right, this._direction, Mathf.Clamp01(t));
	//}
	#endregion

	private void HandlePlayerShoot()
	{
		//Time.time is the actual time accumulated every single frame since the game started
		//Different from Time.deltaTime which is a static time of 0.0167 seconds for every single frame (60 FPS)
		if (Mouse.current.leftButton.isPressed && Time.time >= this._nextShootTime)
		{
			if (GetThrusterTypeIndex() < 3)
			{
				this._bulletInstance = Instantiate(this._bulletPrefab, this._bulletSpawnPoint.position, this.transform.rotation);

				//Every time player shoots a bullet, it marks the character type's (player) name on it
				//Specific to THIS bullet shot at THIS frame
				BulletManager bullet = this._bulletInstance.GetComponent<BulletManager>();
				bullet.SetBulletCharacterType(GameManager.CharacterType.Player);
			}
			else
			{
				this._leftBulletInstance = Instantiate(this._bulletPrefab, this._leftBulletSpawnPoint.position, this.transform.rotation);
				this._rightBulletInstance = Instantiate(this._bulletPrefab, this._leftBulletSpawnPoint.position, this.transform.rotation);

				//Every time player shoots a bullet, it marks the character type's (player) name on it
				//Specific to BOTH THESE bullets shot at THIS frame
				this._leftBulletInstance.GetComponent<BulletManager>().SetBulletCharacterType(GameManager.CharacterType.Player);
				this._rightBulletInstance.GetComponent<BulletManager>().SetBulletCharacterType(GameManager.CharacterType.Player);
			}

			//Physics2D.IgnoreCollision(this._bulletInstance.GetComponent<Collider2D>(), this._player.GetComponent<Collider2D>());

			Debug.Log("Time between shots: " + this._timeBetweenPlayerShots);
			this._nextShootTime = Time.time + this._timeBetweenPlayerShots;
		}
	}


	private void HandleDoubleTypeShoot()
	{
		//Time.time is the actual time accumulated every single frame since the game started
		//Different from Time.deltaTime which is a static time of 0.0167 seconds for every single frame (60 FPS)
		if (Mouse.current.leftButton.isPressed && Time.time >= this._nextShootTime)
		{
			this._bulletInstance = Instantiate(this._bulletPrefab, this._bulletSpawnPoint.position, this.transform.rotation);

			//Every time player shoots, it marks the character type's (player) name on it
			//Specific to THIS bullet shot at THIS frame
			BulletManager bullet = this._bulletInstance.GetComponent<BulletManager>();
			bullet.SetBulletCharacterType(GameManager.CharacterType.Player);

			//Physics2D.IgnoreCollision(this._bulletInstance.GetComponent<Collider2D>(), this._player.GetComponent<Collider2D>());

			Debug.Log("Time between shots: " + this._timeBetweenPlayerShots);
			this._nextShootTime = Time.time + this._timeBetweenPlayerShots;
		}
	}

	public void HandleThrusterInstantiation()
	{
		if (this._thrusterInstance != null) Destroy(this._thrusterInstance);

		switch (GetThrusterTypeIndex())
		{
			case 0: //Thruster Type - Normal
				this._thrusterInstance = Instantiate(this._normalPrefab, this._normalPrefab.transform.position, Quaternion.identity);
				break;
			case 1: //Thruster Type - Thin 
				this._thrusterInstance = Instantiate(this._thinPrefab, this._thinPrefab.transform.position, Quaternion.identity);
				break;
			case 2:
				this._thrusterInstance = Instantiate(this._widePrefab, this._widePrefab.transform.position, Quaternion.identity); 
				break;
			case 3:
				this._thrusterInstance = Instantiate(this._doublePrefab, this._doublePrefab.transform.position, Quaternion.identity);
				break;
		}	
		this._thrusterInstance.transform.SetParent(this._thrusterParent, false);
	}


	#region Setters/Adders & Getters
	public void SetTimeBetweenPlayerShots(float newTimeBetweenPlayerShots)
	{
		this._timeBetweenPlayerShots = newTimeBetweenPlayerShots;
	}

	public void AddThrusterTypeToStack(ThrusterType newThrusterType)
	{
		this.thrusterType = newThrusterType;
		this._thrusterTypeStack.Push(newThrusterType);
		Debug.Log("Stack Count: " + this._thrusterTypeStack.Count);
	}

	public void PopAndSetThrusterType()
	{
		this._thrusterTypeStack.Pop();

		if (this._thrusterTypeStack.Count > 0)
		{
			this.thrusterType = this._thrusterTypeStack.Peek();
		}
	}

	public GameObject GetThrusterInstance()
	{
		return this._thrusterInstance;
	}

	public ThrusterType GetThrusterType()
	{
		return this.thrusterType;
	}

	public int GetThrusterTypeIndex()
	{
		return (int) this.thrusterType;
	}

	#endregion

}
