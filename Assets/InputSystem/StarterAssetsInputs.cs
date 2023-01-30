using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
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

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
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

#endif


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
	
}