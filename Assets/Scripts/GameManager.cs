using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private EWeaponManager[] _enemyWeaponManager;
	[SerializeField] private Player _player;
	[SerializeField] private PWeaponManager _playerWeaponManager;
	[SerializeField] private InputManager _inputManager;
	[SerializeField] private AsteroidSpawnManager _asteroidSpawnManager;

	public static GameManager instance;
	
	public enum CharacterType
	{
		Player, 
		Enemy
	}

	private CharacterType _characterType;

	private void Awake()
	{
		instance = this;	

		this._enemyWeaponManager = FindObjectsByType<EWeaponManager>(FindObjectsSortMode.None);
		this._player = FindAnyObjectByType<Player>();
		this._playerWeaponManager = FindAnyObjectByType<PWeaponManager>();
		this._inputManager = FindAnyObjectByType<InputManager>();
		this._asteroidSpawnManager = FindAnyObjectByType<AsteroidSpawnManager>();
	}

	private void Update()
	{
		HandleGameEnd();
	}

	private void HandleGameEnd()
	{
		if (UIManager.instance.GetTime() >= 480) DestroyScripts();
	}

	private void DestroyScripts()
	{
		Destroy(this._player);
		Destroy(this._playerWeaponManager);
		Destroy(this._inputManager);
		Destroy(this._asteroidSpawnManager);
		foreach (EWeaponManager eWeaponManager in this._enemyWeaponManager)
		{
			if (eWeaponManager != null) Destroy(eWeaponManager);
		}
	}

	#region Setters/Getters
	public void SetCharacterType(CharacterType newCharacterType)
	{
		this._characterType = newCharacterType;
	}
	#endregion
}
