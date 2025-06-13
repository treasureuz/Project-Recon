using System;
using TMPro;
using UnityEngine;

public class EBulletManager : MonoBehaviour
{
	[Header("Enemy Bullet Settings")]
	[SerializeField] private float _enemyBulletDamage = 20f;

	#region Setters & Getters
	public void SetEnemyBulletDamage(float newBulletDamage)
	{
		this._enemyBulletDamage = newBulletDamage; 
	}

	public float GetEnemyBulletDamage()
	{
		return this._enemyBulletDamage;
	}
	#endregion
}

