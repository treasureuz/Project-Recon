using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EWeaponManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private GameObject _bulletPrefab;
	[SerializeField] private Transform _bulletSpawnPoint;
	[SerializeField] private Enemy _enemy;
	[SerializeField] private Player _player;

	[Header("Weapon Settings")]
	[SerializeField] private float _timeBetweenEnemyShots = 0.85f;

	private GameObject _bulletInstance;

	private float _nextShootTime = 0f;

	private void Update()
	{
		if (this._player != null && (this._enemy.GetEnemyState() == Enemy.EnemyState.Attack))
		{
			HandleEnemyShoot();
		}
	}

	private void HandleEnemyShoot()
	{
		//Time.time is the actual time accumulated every single frame since the game started
		//Different from Time.deltaTime which is a static time of 0.0167 seconds for every single frame (60 FPS)
		if (Time.time >= this._nextShootTime)
		{
			this._bulletInstance = Instantiate(this._bulletPrefab, this._bulletSpawnPoint.position, this.transform.rotation);

			//Every time player shoots, it marks the character type's (player) name on it
			//Specific to THIS bullet shot at THIS frame
			BulletManager bullet = this._bulletInstance.GetComponent<BulletManager>();
			bullet.SetBulletCharacterType(GameManager.CharacterType.Enemy);

			this._nextShootTime = Time.time + this._timeBetweenEnemyShots;
		}
	}

}
