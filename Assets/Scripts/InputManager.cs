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
    private InputAction _ability;

    private bool _isMoving;
    private bool _isAbilityPressed;
    private bool _wasAbilityPressedThisFrame;

	private void Awake()
	{
		instance = this;
		playerInput = this.GetComponent<PlayerInput>();
	}

	private void Start()
    {
        this._movement = playerInput.actions["Movement"];
        this._ability = playerInput.actions["Ability"];
	}

	private void Update()
    {
		if (this._player.GetPlayerType() != Player.PlayerType.Standard && this._ability.WasPressedThisFrame())
        {
            this._wasAbilityPressedThisFrame = true; // Used to check if ability was pressed this frame (for player overlay)
			SetIsAbilityPressed(true); // Doesn't turn false until left mouse button is pressed;
		}
        else this._wasAbilityPressedThisFrame = false; // Reset ability pressed this frame
	}

	private void FixedUpdate()
    {
        if (this._movement.WasPressedThisFrame() || this._movement.IsPressed()) this._isMoving = true;
        else this._isMoving = false;

        moveDirection = this._movement.ReadValue<Vector2>().normalized;
	}

    #region Setters/Getters
    public void SetIsAbilityPressed(bool isPressed)
    {
        this._isAbilityPressed = isPressed;
	}

    public bool GetIsAbilityPressed()
    {
        return this._isAbilityPressed;
    }

    public bool GetWasAbilityPressedThisFrame()
    {
        return this._wasAbilityPressedThisFrame;
	}

	public bool GetIsMoving()
    {
        return this._isMoving;
	}

	#endregion
}
