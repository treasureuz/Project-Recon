using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private EWeaponManager _enemyWeaponManager;
	[SerializeField] private PWeaponManager _playerWeaponManager;
	[SerializeField] private InputManager _inputManager;
	[SerializeField] private AsteriodSpawnManager _asteroidSpawnManager;

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

		this._enemyWeaponManager = FindAnyObjectByType<EWeaponManager>();
		this._playerWeaponManager = FindAnyObjectByType<PWeaponManager>();
		this._inputManager = FindAnyObjectByType<InputManager>();
		this._asteroidSpawnManager = FindAnyObjectByType<AsteriodSpawnManager>();
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
		Destroy(this._enemyWeaponManager);
		Destroy(this._playerWeaponManager);
		Destroy(this._inputManager);
		Destroy(this._asteroidSpawnManager);
	}

	#region Setters/Getters
	public void SetCharacterType(CharacterType newCharacterType)
	{
		this._characterType = newCharacterType;
	}
	#endregion
}
