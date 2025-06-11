using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.InputSystem;

public class PWeaponManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject _bulletPrefab;
	[SerializeField] private Transform _bulletSpawnPoint;
	[SerializeField] private Transform _leftBulletSpawnPoint;
	[SerializeField] private Transform _rightBulletSpawnPoint;
	[SerializeField] private Transform _thrusterParent;
	[SerializeField] private BulletManager _bulletManager;

	[Header("Thruster Types")]
	[SerializeField] private GameObject _normalPrefab;
	[SerializeField] private GameObject _thinPrefab;
	[SerializeField] private GameObject _widePrefab;
	[SerializeField] private GameObject _doublePrefab;

	#region Thruster Settings
	[Header("Normal Thruster Settings")]
	[SerializeField] private float _normalTimeBetweenShots = 0.45f;
	[SerializeField] private float _normalBulletDamage = 15f;

	[Header("Thin Thruster Settings")]
	[SerializeField] private float _thinTimeBetweenShots = 0.38f;
	[SerializeField] private float _thinBulletDamage = 22.5f;

	[Header("Wide Thruster Settings")]
	[SerializeField] private float _wideTimeBetweenShots = 0.265f;
	[SerializeField] private float _wideBulletDamage = 30f;

	[Header("Double Thruster Settings")]
	[SerializeField] private float _doubleTimeBetweenShots = 0.125f;
	[SerializeField] private float _doubleBulletDamage = 40f;
	#endregion

	private Stack<ThrusterType> _thrusterTypeStack = new Stack<ThrusterType>();

	private GameObject _bulletInstance;
	private GameObject _leftBulletInstance;
	private GameObject _rightBulletInstance;
	private GameObject _thrusterInstance;

	private float _timeBetweenShots;
	private float _nextShootTime = 0f;

	public enum ThrusterType
	{
		Normal = 0, // Standard 
		Thin = 1, // Omen
		Wide = 2, // Sora
		Double = 3, // Ralph
	}

	private ThrusterType _thrusterType;

	private void Start()
	{
		this._thrusterType = ThrusterType.Normal;
	}

	private void Update()
	{
		HandlePlayerShoot();
	}

	#region ThrusterType Settings
	private void Normal()
	{
		this._timeBetweenShots = this._normalTimeBetweenShots;
		this._bulletManager.SetBulletDamage(this._normalBulletDamage);
		this._thrusterInstance = Instantiate(this._normalPrefab, this._normalPrefab.transform.position, Quaternion.identity);
	}

	private void Thin()
	{
		this._timeBetweenShots = this._thinTimeBetweenShots;
		this._bulletManager.SetBulletDamage(this._thinBulletDamage);
		this._thrusterInstance = Instantiate(this._thinPrefab, this._thinPrefab.transform.position, Quaternion.identity);
	}

	private void Wide()
	{
		this._timeBetweenShots = this._wideTimeBetweenShots;
		this._bulletManager.SetBulletDamage(this._wideBulletDamage);
		this._thrusterInstance = Instantiate(this._widePrefab, this._widePrefab.transform.position, Quaternion.identity);
	}

	private void Double()
	{
		this._timeBetweenShots = this._doubleTimeBetweenShots;
		this._bulletManager.SetBulletDamage(this._doubleBulletDamage);
		this._thrusterInstance = Instantiate(this._doublePrefab, this._doublePrefab.transform.position, Quaternion.identity);
	}
	#endregion

	#region Shoot Actions
	private void HandlePlayerShoot()
	{
		//Time.time is the actual time accumulated every single frame since the game started
		//Different from Time.deltaTime which is a static time of 0.0167 seconds for every single frame (60 FPS)
		if (Mouse.current.leftButton.isPressed && Time.time >= this._nextShootTime)
		{
			switch (this._thrusterType)
			{
				case ThrusterType.Double: StartCoroutine(HandleDoubleThrusterShoot()); break;
				default:
					this._bulletInstance = Instantiate
						(this._bulletPrefab, this._bulletSpawnPoint.position, this.transform.rotation);

					//Marks that the character type, player, shot the bullet
					//**Specific to THIS bullet shot at THIS frame**
					BulletManager bullet = this._bulletInstance.GetComponent<BulletManager>();
					bullet.SetBulletCharacterType(GameManager.CharacterType.Player);
					break;
			}

			//Physics2D.IgnoreCollision(this._bulletInstance.GetComponent<Collider2D>(), this._radius.GetComponent<Collider2D>());

			Debug.Log("Time between shots: " + this._timeBetweenShots);
			this._nextShootTime = Time.time + this._timeBetweenShots;
		}
	}

	private IEnumerator HandleDoubleThrusterShoot()
	{
		this._leftBulletInstance = Instantiate
			(this._bulletPrefab, this._leftBulletSpawnPoint.position, this.transform.rotation);

		yield return new WaitForSeconds(this._timeBetweenShots - 0.045f); // Wait time between double thruster shots

		this._rightBulletInstance = Instantiate
			(this._bulletPrefab, this._rightBulletSpawnPoint.position, this.transform.rotation);

		//Every time player shoots a bullet, it marks the character type's (player) name on it
		//Specific to BOTH THESE bullets shot at THIS frame
		this._leftBulletInstance.GetComponent<BulletManager>().SetBulletCharacterType(GameManager.CharacterType.Player);
		this._rightBulletInstance.GetComponent<BulletManager>().SetBulletCharacterType(GameManager.CharacterType.Player);
	}
	#endregion

	public void HandleThrusterAndBulletInstantiation()
	{
		//Destroy previous thruster
		if (this._thrusterInstance != null) Destroy(this._thrusterInstance);

		//Set bullet character type to player so it can get the correct bullet damage
		this._bulletManager.SetBulletCharacterType(GameManager.CharacterType.Player);

		switch (this._thrusterType)
		{
			case ThrusterType.Normal: Normal(); break; //Player Type - Standard
			case ThrusterType.Thin: Thin(); break; //Player Type - Omen
			case ThrusterType.Wide: Wide(); break; //Player Type - Sora
			case ThrusterType.Double: Double(); break; //Player Type - Ralph
		}
		this._thrusterInstance.transform.SetParent(this._thrusterParent, false);

		//Enables/Disables Bullet Sprites and its corresponding Collider based on the current ThrusterType
		this._bulletManager.ToggleBulletSprite(this._thrusterType);
	}

	#region Setters/Adders & Getters
	public void AddThrusterTypeToStack(ThrusterType newThrusterType)
	{
		this._thrusterType = newThrusterType;
		this._thrusterTypeStack.Push(newThrusterType);
		Debug.Log("Stack Count: " + this._thrusterTypeStack.Count);
	}

	public void PopAndSetThrusterType()
	{
		this._thrusterTypeStack.Pop();
		Debug.Log("Stack Count After Pop: " + this._thrusterTypeStack.Count);
		if (this._thrusterTypeStack.Count > 0)
		{
			this._thrusterType = this._thrusterTypeStack.Peek();
		}
	}

	public GameObject GetThrusterInstance()
	{
		return this._thrusterInstance;
	}

	public ThrusterType GetThrusterType()
	{
		return this._thrusterType;
	}
	#endregion

}
