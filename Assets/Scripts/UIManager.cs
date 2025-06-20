using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Camera _mainCam;
	[SerializeField] private TMP_Text _healthText;
	[SerializeField] private TMP_Text _bulletText;
	[SerializeField] private TMP_Text _damageTextPrefab;
	[SerializeField] private RectTransform _canvasParent;
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

	public void SpawnDamageText(Vector3 spawnPos, int damageAmount)
	{
		TMP_Text damageTextInstance = Instantiate(this._damageTextPrefab, this._canvasParent);

		damageTextInstance.rectTransform.localScale = Vector3.one; // Reset scale to 1,1,1

		damageTextInstance.rectTransform.position = spawnPos;
		damageTextInstance.text = damageAmount.ToString(); //Set text value at the above position

		Destroy(damageTextInstance.gameObject, 0.35f);
	}

}
