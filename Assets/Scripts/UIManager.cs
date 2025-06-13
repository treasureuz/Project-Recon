using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
	[Header("References")]
    [SerializeField] private TMP_Text _healthText;
	[SerializeField] private TMP_Text _bulletText;
	[SerializeField] private Player _player;
	[SerializeField] private PWeaponManager _playerWeaponManager;
	[SerializeField] private PBulletManager _playerBulletManager;

	public static UIManager instance;

	private float _currentHealth;
	private float _maxHealth;
	private float _currentBulletMagCount;
	private float _maxBulletMagCount;

	private void Awake()
	{
		instance = this;
	}

	private void Start()
	{
		this._healthText.text = "HEALTH: " + this._player.GetCurrentHealth().ToString();
		this._bulletText.text = "BULLETS: " + this._playerBulletManager.GetCurrentMagazineCount().ToString() + "/" +
								this._playerWeaponManager.GetMaxBulletMagazineCount().ToString();
	}

	public void UpdateHealthText()
	{
		this._currentHealth = this._player.GetCurrentHealth();
		this._maxHealth = this._player.GetMaxHealth();

		if (this._currentHealth < 0) this._currentHealth = 0;
		this._healthText.text = "HEALTH: " + this._currentHealth.ToString();
	}

	public void UpdateBulletText()
	{
		this._currentBulletMagCount = this._playerBulletManager.GetCurrentMagazineCount();
		this._maxBulletMagCount = this._playerWeaponManager.GetMaxBulletMagazineCount();

		if (this._currentBulletMagCount < 0) this._currentBulletMagCount = 0;
		this._bulletText.text = "BULLETS: " + this._currentBulletMagCount.ToString() + "/" + 
								this._maxBulletMagCount.ToString();
	}
}
