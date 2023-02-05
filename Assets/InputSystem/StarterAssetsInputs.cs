using UnityEngine;
using UnityEngine.InputSystem;

public class StarterAssetsInputs : MonoBehaviour
{
	[Header("Character Input Values")]
	public Vector2 move;
	//public Vector2 look;
	public bool jump;
	public bool sprint;
	public bool slide;
	public bool attack;

	[Header("Movement Settings")]
	public bool analogMovement;

	[Header("Mouse Cursor Settings")]
	public bool cursorLocked = true;
	public bool cursorInputForLook = true;

	public void OnMove(InputValue value)
	{
		MoveInput(value.Get<Vector2>());
	}


	//public void OnLook(InputValue value)
	//{
	//	if(cursorInputForLook)
	//	{
	//		LookInput(;
	// look = value.Get<Vector2>());
	//	}
	//}

	public void OnJump(InputValue value)
	{
		JumpInput(value.isPressed);
	}


	public void OnSlide(InputValue value)
	{
		slide = value.isPressed;
	}

	public void OnAttack(InputValue value)
	{
		attack = value.isPressed;
	}


	public void MoveInput(Vector2 newMoveDirection)
	{
		move = newMoveDirection;
	}

	public void JumpInput(bool newJumpState)
	{
		jump = newJumpState;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		SetCursorState(cursorLocked);
	}

	private void SetCursorState(bool newState)
	{
		Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
	}
}