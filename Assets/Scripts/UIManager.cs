using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
	[Header("Text References")]
	[SerializeField] private TMP_Text _timeText;
	[SerializeField] private TMP_Text _healthText;
	[SerializeField] private TMP_Text _bulletText;
	[SerializeField] private TMP_Text _cooldownText;
	[SerializeField] private TMP_Text _damageTextPrefab;

	[Header("Other References")]
	[SerializeField] private Camera _mainCam;
	[SerializeField] private RectTransform _worldCanvasParent;
	[SerializeField] private GameObject _cooldownBox;
	[SerializeField] private Player _player;
	[SerializeField] private PWeaponManager _playerWeaponManager;
	[SerializeField] private PBulletManager _playerBulletManager;

	public static UIManager instance;

	private int _time = 0;
	private int _minutes = 0;
	private int _seconds;

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

	private void Update()
	{
		if (!this._player.GetIsSoraFreezeActive())
		{
			UpdateTimeText(); // Update the time text every frame	
		}
	}

	private void UpdateTimeText()
	{
		this._time = (int) Time.time; // Get the time since the level started in seconds

		this._minutes = this._time / 60;
		this._seconds = this._time % 60;

		if (this._seconds < 10) this._timeText.text = "TIME: " + this._minutes.ToString() + ":0" + this._seconds.ToString();
		else this._timeText.text = "TIME: " + this._minutes.ToString() + ":" + this._seconds.ToString();

		// If the time reaches 8 minutes, stop the game
		if (this._time >= 480) // 8 minutes = 480 seconds
		{
			Time.timeScale = 0; // Stop the game
			this._timeText.text = "TIME: " + this._minutes.ToString() + ":00"; // Show final time
		}
		else Time.timeScale = 1; // Resume the game if not stopped (Default behavior)
	}

	public void UpdateHealthText(int currentHealth)
	{
		if (currentHealth < 0) currentHealth = 0;
		this._healthText.text = "HEALTH: " + currentHealth.ToString();
	}

	public void UpdateBulletText(int currentBulletMagCount, int maxBulletMagCount)
	{
		this._bulletText.text = "BULLETS: " + currentBulletMagCount.ToString() + "/" + maxBulletMagCount.ToString();
	}

	public void UpdateCooldownText(int cooldownTime)
	{
		if (this._player.GetPlayerType() != Player.PlayerType.Standard) 
		{
			this._cooldownBox.SetActive(true); // Show the cooldown box if player type is not Standard
			cooldownTime -= (int) Time.time; // Calculate remaining cooldown time
			if (cooldownTime <= 0) cooldownTime = this._player.GetBaseCooldownTime(); // Ensure cooldown time does not go negative
			this._cooldownText.text = cooldownTime.ToString() + "s";
		}
		else this._cooldownBox.SetActive(false); // Hide the cooldown box
	}

	public void SpawnDamageText(Vector3 spawnPos, int damageAmount)
	{
		TMP_Text damageTextInstance = Instantiate(this._damageTextPrefab, this._worldCanvasParent);

		damageTextInstance.rectTransform.localScale = Vector3.one; // Reset scale to 1,1,1

		damageTextInstance.rectTransform.position = spawnPos;
		damageTextInstance.text = damageAmount.ToString(); //Set text value at the above position

		Destroy(damageTextInstance.gameObject, 0.35f);
	}

	#region Getters and Setters
	public int GetTime()
	{
		return this._time;
	}
	#endregion
}
