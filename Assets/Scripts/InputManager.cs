using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player _player;

	private static PlayerInput playerInput;
    public static Vector2 moveDirection; //Caches move
	public static InputManager instance;
                                        
    private InputAction _movement;
    private InputAction _abilityE;
    private InputAction _abilityC;

    private bool _isMoving;
    private bool _isAbilityEPressed;
    private bool _isAbilityCPressed;
	private bool _wasAbilityEPressedThisFrame;

	private void Awake()
	{
		instance = this;
		playerInput = this.GetComponent<PlayerInput>();
	}

	private void Start()
    {
        this._movement = playerInput.actions["Movement"];
        this._abilityE = playerInput.actions["E - Ability"];
        this._abilityC = playerInput.actions["C - Ability"];
	}

	private void Update()
    {
		if (this._player.GetPlayerType() != Player.PlayerType.Standard && this._abilityE.WasPressedThisFrame())
        {
            this._wasAbilityEPressedThisFrame = true; // Used to check if ability was pressed this frame (for player overlay)
			this._isAbilityEPressed = true; // Set ability pressed to true
		}
        else this._wasAbilityEPressedThisFrame = false; // Reset ability pressed this frame

        if (this._player.GetPlayerType() == Player.PlayerType.Ralph && this._abilityC.IsPressed())
        {
            this._isAbilityCPressed = true; // Set ability C pressed to true
        }
        else this._isAbilityCPressed = false; // Reset ability C pressed
	}

	#region Movement Input
	private void FixedUpdate()
    {
        if (this._movement.WasPressedThisFrame() || this._movement.IsPressed()) this._isMoving = true;
        else this._isMoving = false;

		moveDirection = this._movement.ReadValue<Vector2>().normalized;
	}
	#endregion

	#region Setters/Getters
	public void SetIsAbilityEPressedToFalse()
    {
        this._isAbilityEPressed = false;
	}

    public bool GetIsAbilityEPressed()
    {
        return this._isAbilityEPressed;
    }

    public bool GetIsAbilityCPressed()
    {
        return this._isAbilityCPressed;
	}

	public bool GetWasAbilityEPressedThisFrame()
    {
        return this._wasAbilityEPressedThisFrame;
	}

	public bool GetIsMoving()
    {
        return this._isMoving;
	}

	#endregion
}
