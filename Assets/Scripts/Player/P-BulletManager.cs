using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static PWeaponManager;

public class PBulletManager : MonoBehaviour
{
	[Header("Player Bullet Settings")]
	[SerializeField] private float _playerBulletDamage = 15f;
	[SerializeField] private int _bulletMagazineCount = 140;

	[Header("Player Bullet Sprite Components")]
	[SerializeField] private SpriteRenderer _thinBulletSprite;
	[SerializeField] private SpriteRenderer _wideBulletSprite;
	[SerializeField] private CircleCollider2D _wideBulletCircleCollider;
	[SerializeField] private SpriteRenderer _regularBulletSprite;
	[SerializeField] private CapsuleCollider2D _regularBulletCapCollider;

	public void ToggleBulletSprite(PWeaponManager.ThrusterType currentType)
	{
		//Enables Regular Bullet Sprite if Thruster Type is "Normal" or "Double"
		this._regularBulletSprite.enabled =
		(currentType == PWeaponManager.ThrusterType.Normal || currentType == PWeaponManager.ThrusterType.Double);

		//Enables Regular Bullet Capsule Collider if Thruster Type is not Wide ("Normal", "Double", or "Thin")
		this._regularBulletCapCollider.enabled = (currentType != PWeaponManager.ThrusterType.Wide);

		//Enables Thin Bullet Sprite
		this._thinBulletSprite.enabled = (currentType == PWeaponManager.ThrusterType.Thin);

		//Enables Wide Bullet Sprite and its Circle Collider
		this._wideBulletSprite.enabled = (currentType == PWeaponManager.ThrusterType.Wide);
		this._wideBulletCircleCollider.enabled = (currentType == PWeaponManager.ThrusterType.Wide);
	}

	#region Setters & Getters
	public void SetMagazineCount(int newBulletMagCount)
	{
		this._bulletMagazineCount = newBulletMagCount;
	}

	public int DecrementMagazineCount()
	{
		return --this._bulletMagazineCount;
	}

	public int GetCurrentMagazineCount()
	{
		return this._bulletMagazineCount;
	}

	public void SetPlayerBulletDamage(float newBulletDamage)
	{
		this._playerBulletDamage = newBulletDamage; 
	}

	public float GetPlayerBulletDamage()
	{
		return this._playerBulletDamage;
	}
	#endregion
}

