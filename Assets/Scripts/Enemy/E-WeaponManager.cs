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
		//HandleWeaponRotation();
		if (this._player != null && (this._enemy.GetIsEnemyShot() || this._enemy.GetIsWithinRadius()))
		{
			HandleEnemyShoot();
		}
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
