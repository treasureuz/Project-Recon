using System;
using System.Collections;
using System.Collections.Generic;
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

	[Header("Weapon Settings")]
	[SerializeField] private float _timeBetweenPlayerShots = 0.45f;

	[Header("Thruster Types")]
	[SerializeField] private GameObject _normalPrefab;
	[SerializeField] private GameObject _thinPrefab;
	[SerializeField] private GameObject _widePrefab;
	[SerializeField] private GameObject _doublePrefab;

	private Stack<ThrusterType> _thrusterTypeStack = new Stack<ThrusterType>();

	//private GameObject _bulletInstance;
	//private GameObject _leftBulletInstance;
	//private GameObject _rightBulletInstance;
	private GameObject _thrusterInstance;

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

	private void HandlePlayerShoot()
	{
		//Time.time is the actual time accumulated every single frame since the game started
		//Different from Time.deltaTime which is a static time of 0.0167 seconds for every single frame (60 FPS)
		if (Mouse.current.leftButton.isPressed && Time.time >= this._nextShootTime)
		{
			if (GetThrusterTypeIndex() == 3) //If ThrusterType is "Double" (bc double has diff shooting mechanic) 
			{
				StartCoroutine(HandleDoubleThrusterShoot());
			}
			else
			{
				GameObject bulletInstance = Instantiate(this._bulletPrefab, this._bulletSpawnPoint.position, this.transform.rotation);

				//Every time player shoots a bullet, it marks the character type's (player) name on it
				//Specific to THIS bullet shot at THIS frame
				bulletInstance.GetComponent<BulletManager>().SetBulletCharacterType(GameManager.CharacterType.Player);
			}

			//Physics2D.IgnoreCollision(this._bulletInstance.GetComponent<Collider2D>(), this._radius.GetComponent<Collider2D>());

			Debug.Log("Time between shots: " + this._timeBetweenPlayerShots);
			this._nextShootTime = Time.time + this._timeBetweenPlayerShots;
		}
	}

	private IEnumerator HandleDoubleThrusterShoot()
	{
		GameObject leftBulletInstance = Instantiate(this._bulletPrefab, this._leftBulletSpawnPoint.position, this.transform.rotation);
		
		yield return new WaitForSeconds(this._timeBetweenPlayerShots - 0.045f);
		
		GameObject rightBulletInstance = Instantiate(this._bulletPrefab, this._rightBulletSpawnPoint.position, this.transform.rotation);

		//Every time player shoots a bullet, it marks the character type's (player) name on it
		//Specific to BOTH THESE bullets shot at THIS frame
		leftBulletInstance.GetComponent<BulletManager>().SetBulletCharacterType(GameManager.CharacterType.Player);
		rightBulletInstance.GetComponent<BulletManager>().SetBulletCharacterType(GameManager.CharacterType.Player);
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
			case 2: //Thruster Type - Wide
				this._thrusterInstance = Instantiate(this._widePrefab, this._widePrefab.transform.position, Quaternion.identity); 
				break;
			case 3: //Thruster Type - Double
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

	public int GetThrusterTypeIndex()
	{
		return (int) this._thrusterType;
	}

	#endregion

}
