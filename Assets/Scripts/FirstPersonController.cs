using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Cinemachine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Gravity))]
public class FirstPersonController : MonoBehaviour
{
	[Header("Player")]
	public float MaxSpeed = 5;
	//public float SpeedIncreaseRate = 1.0f;
	//public float SpeedIncreaseInterval = 5.0f;

	public float ForwardSpeed = 8.0f;
	[Tooltip("Move speed of the character in m/s")]
	public float CrossSpeed = 8.0f;
	[Tooltip("Acceleration and deceleration")]
	public float SpeedChangeRate = 1.0f;

	[Space(10)]
	[Tooltip("The height the player can jump")]
	public float JumpHeight = 1.2f;

	[Space(10)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float JumpTimeout = 0.1f;
	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float FallTimeout = 0.15f;

	[Header("Cinemachine")]
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
	public GameObject CinemachineCameraTarget;
	[Tooltip("How far in degrees can you move the camera up")]
	public float TopClamp = 90.0f;
	[Tooltip("How far in degrees can you move the camera down")]
	public float BottomClamp = -90.0f;

	public delegate void TriggerAction(Collider other);
	public TriggerAction triggerEnter;

	public LayerMask boundaryMask;

	// cinemachine
	private float _cinemachineTargetPitch;

	// player
	private float _speed;
	private float _rotationVelocity;
	//public float _verticalVelocity;
	//private float _terminalVelocity = 53.0f;

	// timeout deltatime
	private float _jumpTimeoutDelta;
	private float _fallTimeoutDelta;


	private PlayerInput _playerInput;
	private Gravity _gravity;
    private CharacterController _controller;
	private StarterAssetsInputs _input;

	private GameObject _mainCamera;
	public CinemachineVirtualCamera vCamera;

    private const float _threshold = 0.01f;

	public AudioSource sound_change_gravity;
	public AudioSource sound_jump;


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
		_gravity = GetComponent<Gravity>();

		// reset our timeouts on start
		_jumpTimeoutDelta = JumpTimeout;
		_fallTimeoutDelta = FallTimeout;

		//SpeedUp();

    }

	private void Update()
	{
        ReverseGravity();
        JumpAndGravity();
        Move();

		if (_controller.velocity.z - MaxSpeed >= 1)
		{
			if (!GetComponent<TrailRenderer>().emitting)
				GetComponent<TrailRenderer>().emitting = true;
		}
		else
		{
			if (GetComponent<TrailRenderer>().emitting)
				GetComponent<TrailRenderer>().emitting = false;
		}
	}

	private void LateUpdate()
	{
		CameraRotation();
	}

	private void CameraRotation()
	{
		CinemachineCameraTarget.transform.position = transform.position + transform.up;

        CinemachineCameraTarget.transform.eulerAngles = Vector3.zero;
		var follow = vCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

		if (_gravity.direction > 0)
		{
			follow.ShoulderOffset.y = Mathf.Min(2f, follow.ShoulderOffset.y + 4 * Time.deltaTime);
		}
		else
		{
			follow.ShoulderOffset.y = Mathf.Max(-2f, follow.ShoulderOffset.y - 4 * Time.deltaTime);
		}

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
		bool hitBoundary = Physics.Raycast(transform.position, transform.right * _input.move.x * _gravity.direction, 0.5f, boundaryMask);
        Vector3 inputDirection = hitBoundary ? Vector3.zero : transform.right * _input.move.x * _gravity.direction;

        _controller.Move(
            Vector3.forward * ForwardSpeed * Time.deltaTime + 
			inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _gravity.velocity, 0.0f) * Time.deltaTime);
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
			if (_gravity.HasGroundUp())
			{
                _gravity.Reverse();
				sound_change_gravity.Play();

            }
        }
	}

    public void CancelJump()
	{
        _input.jump = false;
        _gravity.velocity = -2f;
    }

    private void JumpAndGravity()
	{
		if (_gravity.Grounded) // jump 
		{
			// reset the fall timeout timer
			_fallTimeoutDelta = FallTimeout;

			if (_input.jump && _jumpTimeoutDelta <= 0.0f)
			{
                sound_jump.Play();
                _gravity.velocity = Mathf.Sqrt(JumpHeight * -2f * _gravity.force) * _gravity.direction;
            }
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
			_gravity.AddForce(Time.deltaTime * _gravity.force * _gravity.direction * -0.5f);
		}
		//else
		//{
		//	_gravity.ApplyGravity();
		//}
	}

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f) lfAngle += 360f;
		if (lfAngle > 360f) lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

	//public void SpeedUp()
	//{
	//	StopCoroutine("SpeedUpAction");
	//	StartCoroutine("SpeedUpAction");
 //   }

 //   public IEnumerator SpeedUpAction()
 //   {
	//	while (true)
	//	{
	//		if (ForwardSpeed < MaxSpeed)
	//		{
 //               ForwardSpeed += SpeedIncreaseRate;
 //               CrossSpeed += SpeedIncreaseRate;
 //           }

 //           yield return new WaitForSeconds(SpeedIncreaseInterval);
	//	}
 //   }
}
