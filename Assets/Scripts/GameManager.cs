using UnityEngine;

public class GameManager : MonoBehaviour
{
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
	}

	public void SetCharacterType(CharacterType newCharacterType)
	{
		this._characterType = newCharacterType;
	}
}
