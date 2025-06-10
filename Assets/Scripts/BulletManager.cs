using NUnit.Framework.Constraints;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
	[Header("Player Bullets")]
	private GameObject _regularBulletPrefab;
	private GameObject _thinBulletPrefab;
	private GameObject _wideBulletPrefab;

	[Header("Bullet Damage Settings")]
	[SerializeField] private float _playerBulletDamage = 15f;
	[SerializeField] private float _enemyBulletDamage = 20f;

	private GameManager.CharacterType _characterType;
	private PWeaponManager.ThrusterType _thrusterType;

	#region Setters & Getters
	public void SetBulletCharacterType(GameManager.CharacterType newCharacterType)
	{
		this._characterType = newCharacterType;
	}

	public void SetBulletDamage(float newBulletDamage)
	{
		switch (this._characterType)
		{
			case GameManager.CharacterType.Player:
				this._playerBulletDamage = newBulletDamage;
				break;
			case GameManager.CharacterType.Enemy:
				this._enemyBulletDamage = newBulletDamage;
				break;
		}
	}

	public float GetBulletDamage()
	{
		switch (this._characterType)
		{
			case GameManager.CharacterType.Player:
				return this._playerBulletDamage;
			case GameManager.CharacterType.Enemy:
				return this._enemyBulletDamage;
			default:
				return 0f;
		}
	}

	public GameObject GetBulletPrefab()
	{
		switch (this._thrusterType)
		{
			case PWeaponManager.ThrusterType.Thin: return this._thinBulletPrefab;
			case PWeaponManager.ThrusterType.Wide: return this._wideBulletPrefab;
			default: return this._regularBulletPrefab;
		}
	}

	#endregion

}
