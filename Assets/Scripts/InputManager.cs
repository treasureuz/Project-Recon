using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput playerInput;
    public static Vector2 _moveDirection;
    public static InputManager instance;

    private InputAction _movement;

    public bool _isMoving;

	void Start()
    {
        instance = this;

		playerInput = this.GetComponent<PlayerInput>();
        this._movement = playerInput.actions["Movement"];
	}
	
	void Update()
    {
        if (this._movement.WasPressedThisFrame() || this._movement.IsPressed() )
        {
            this._isMoving = true;
        }
        else
        {
            this._isMoving = false;
        }
        _moveDirection = this._movement.ReadValue<Vector2>().normalized;

	}
}
