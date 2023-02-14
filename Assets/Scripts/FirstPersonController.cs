using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class FirstPersonController : MonoBehaviour
{
	[Header("Player")]
	public float MaxSpeed = 20f;
	public float SpeedIncreaseRate = 5f;
	public float SpeedIncreaseInterval = 5f;

	public float ForwardSpeed = 5.0f;
	[Tooltip("Move speed of the character in m/s")]
	public float CrossSpeed = 4.0f;
	[Tooltip("Acceleration and deceleration")]
	public float SpeedChangeRate = 10.0f;

	[Space(10)]
	[Tooltip("The height the player can jump")]
	public float JumpHeight = 1.2f;
	[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
	public float Gravity = -9.8f;
	public float gravityDirection = 1;

	[Space(10)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float JumpTimeout = 0.1f;
	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float FallTimeout = 0.15f;

	[Header("Player Grounded")]
	[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
	public bool Grounded = true;
	[Tooltip("Useful for rough ground")]
	public float GroundedOffset = -0.14f;
	[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
	public float GroundedRadius = 0.5f;
	[Tooltip("What layers the character uses as ground")]
	public LayerMask GroundLayers;

	[Header("Cinemachine")]
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
	public GameObject CinemachineCameraTarget;
	[Tooltip("How far in degrees can you move the camera up")]
	public float TopClamp = 90.0f;
	[Tooltip("How far in degrees can you move the camera down")]
	public float BottomClamp = -90.0f;

	public delegate void TriggerAction(Collider other);
	public TriggerAction triggerEnter;

	// cinemachine
	private float _cinemachineTargetPitch;

	// player
	private float _speed;
	private float _rotationVelocity;
	public float _verticalVelocity;
	private float _terminalVelocity = 53.0f;

	// timeout deltatime
	private float _jumpTimeoutDelta;
	private float _fallTimeoutDelta;


	private PlayerInput _playerInput;
	private CharacterController _controller;
	private StarterAssetsInputs _input;
	private GameObject _mainCamera;

	private const float _threshold = 0.01f;

	private IEnumerator reverseAction;

	private bool IsCurrentDeviceMouse
	{
		get
		{
			return _playerInput.currentControlScheme == "KeyboardMouse";
		}
	}

	private void Awake()
	{
		// get a reference to our main camera
		if (_mainCamera == null)
		{
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		}
	}

	private void Start()
	{
		_controller = GetComponent<CharacterController>();
		_input = GetComponent<StarterAssetsInputs>();
		_playerInput = GetComponent<PlayerInput>();

		// reset our timeouts on start
		_jumpTimeoutDelta = JumpTimeout;
		_fallTimeoutDelta = FallTimeout;

		SpeedUp();

    }

	private void Update()
	{
		ReverseGravity();
        JumpAndGravity();
		GroundedCheck();
		Move();
	}

	private void LateUpdate()
	{
		CameraRotation();
	}

	private void GroundedCheck()
	{
		// set sphere position, with offset
		Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + GroundedOffset, transform.position.z);
		Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
	}

	private void CameraRotation()
	{
		CinemachineCameraTarget.transform.eulerAngles = new Vector3(0, 0, 0);
    }

	private void Move()
	{
		// set target speed based on move speed, sprint speed and if sprint is pressed
		float targetSpeed = CrossSpeed;

		float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, 0).magnitude;
		// accelerate or decelerate to target speed
		if (Mathf.Abs(currentHorizontalSpeed - targetSpeed) > 0.1f)
		{
			// float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
			_speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = targetSpeed;
		}

		// normalise input direction
		Vector3 inputDirection = transform.right * _input.move.x * gravityDirection;

		// move the player
		_controller.Move(
            Vector3.forward * ForwardSpeed * Time.deltaTime + 
			inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
	}

    private void OnTriggerEnter(Collider other)
    {
		triggerEnter?.Invoke(other);
    }

    private void ReverseGravity()
	{
		if (_input.primaryAction)
		{
			_input.primaryAction = false;
			if (Physics.Raycast(transform.position, transform.up, 50, GroundLayers))
			{
                gravityDirection = -gravityDirection;
                _verticalVelocity = 0;
                _input.primaryAction = false;

                if (reverseAction != null)
                    StopCoroutine(reverseAction);
                reverseAction = ReverseCharacterAction(gravityDirection == 1 ? 0 : 180);
                StartCoroutine(reverseAction);
            }
		}
	}

	private IEnumerator ReverseCharacterAction(float angle)
	{
		yield return new WaitForSeconds(0.05f);

        while (transform.eulerAngles.z != angle)
		{
			transform.eulerAngles = Vector3.MoveTowards(
				transform.eulerAngles,
				new Vector3(transform.rotation.eulerAngles.x, transform.rotation.y, angle),
				Time.deltaTime * 480
			);
			yield return null;
		}
		reverseAction = null;
    }


    public void CancelJump()
	{
        _input.jump = false;
        _verticalVelocity = -2f;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
  //      if (hit.gameObject.transform.up == Vector3.down * gravityDirection)
		//{
		//	Debug.Log("Cancel Jump");
		//	CancelJump();
  //      }	
    }

    private void JumpAndGravity()
	{
		if (Grounded) // jump 
		{
			// reset the fall timeout timer
			_fallTimeoutDelta = FallTimeout;
			_verticalVelocity = gravityDirection > 0 ? Mathf.Max(-3f, _verticalVelocity) : Mathf.Min(3f, _verticalVelocity);

			if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity) * gravityDirection;
			if (_jumpTimeoutDelta >= 0.0f)
				_jumpTimeoutDelta -= Time.deltaTime;
		}
		else // fall
		{
			_jumpTimeoutDelta = JumpTimeout;
			if (_fallTimeoutDelta >= 0.0f)
				_fallTimeoutDelta -= Time.deltaTime;

			//_input.jump = false;
		}

		// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
		if (_input.jump)
		{
			_verticalVelocity += Time.deltaTime * Gravity * gravityDirection * -0.5f;
        }
        else if (_verticalVelocity < _terminalVelocity)
		{
			_verticalVelocity += Gravity * Time.deltaTime * gravityDirection;
		}
	}

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f) lfAngle += 360f;
		if (lfAngle > 360f) lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

	public void SpeedUp()
	{
		StopCoroutine("SpeedUpAction");
		StartCoroutine("SpeedUpAction");
    }

    public IEnumerator SpeedUpAction()
    {
		while (true)
		{
			if (ForwardSpeed < MaxSpeed)
			{
                ForwardSpeed += SpeedIncreaseRate;
                CrossSpeed += SpeedIncreaseRate;
            }

            yield return new WaitForSeconds(SpeedIncreaseInterval);
		}
    }
}
