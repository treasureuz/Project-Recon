using NUnit.Framework.Constraints;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
	[Header("Player Bullet Sprites")]
	[SerializeField] SpriteRenderer _thinBulletSprite;
	[SerializeField] SpriteRenderer _wideBulletSprite;

	[Header("Bullet Damage Settings")]
	[SerializeField] private float _playerBulletDamage = 15f;
	[SerializeField] private float _enemyBulletDamage = 20f;

	private SpriteRenderer _regularBulletSprite;
	private CapsuleCollider2D _regularBulletCapCollider;
	private CircleCollider2D _wideBulletCircleCollider;

	private GameManager.CharacterType _characterType;

	public void ToggleBulletSprite(PWeaponManager.ThrusterType currentType) 
	{
		//Gets this GameObject's Sprite Renderer, Capsule Collider, and Circle Collider (used for wideBulletSprite)
		this._regularBulletSprite = GetComponent<SpriteRenderer>();
		this._regularBulletCapCollider = GetComponent<CapsuleCollider2D>();
		this._wideBulletCircleCollider = GetComponent<CircleCollider2D>();

		//Enables Regular Bullet Sprite if Thruster Type is "Normal" or "Double"
		this._regularBulletSprite.enabled = 
			(currentType == PWeaponManager.ThrusterType.Normal || currentType == PWeaponManager.ThrusterType.Double);
		//Enables Regular Bullet Capsule Collider if Thruster Type is not Wide ("Normal", "Double", or "Thin")
		this._regularBulletCapCollider.enabled = 
			(currentType != PWeaponManager.ThrusterType.Wide);

		//Enables Thin Bullet Sprite
		this._thinBulletSprite.enabled = (currentType == PWeaponManager.ThrusterType.Thin);

		//Enables Wide Bullet Sprite and its Circle Collider
		this._wideBulletSprite.enabled = (currentType == PWeaponManager.ThrusterType.Wide);
		this._wideBulletCircleCollider.enabled = (currentType == PWeaponManager.ThrusterType.Wide);
	}

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
				this._playerBulletDamage = newBulletDamage; break;
			case GameManager.CharacterType.Enemy:
				this._enemyBulletDamage = newBulletDamage; break;
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
