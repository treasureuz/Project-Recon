using UnityEngine;

public class BulletManager : MonoBehaviour
{
	[Header("Player Bullet Prefabs")]
	[SerializeField] private GameObject _regularBulletPrefab;
	[SerializeField] private GameObject _thinBulletPrefab;
	[SerializeField] private GameObject _wideBulletPrefab;

	[Header("Bullet Damage Settings")]
	[SerializeField] private float _playerBulletDamage = 15f;
	[SerializeField] private float _enemyBulletDamage = 20f;

	private GameManager.CharacterType _characterType;

	#region Setters & Getters
	public void SetBulletPrefab(PWeaponManager.ThrusterType currentType)
	{
		this._regularBulletPrefab.SetActive
			((currentType == PWeaponManager.ThrusterType.Normal) || (currentType == PWeaponManager.ThrusterType.Double));
		this._thinBulletPrefab.SetActive(currentType == PWeaponManager.ThrusterType.Thin);
		this._wideBulletPrefab.SetActive(currentType == PWeaponManager.ThrusterType.Wide);
	}

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
	#endregion

}
