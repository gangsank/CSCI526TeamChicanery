using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class FirstPersonController : MonoBehaviour
{
	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float MoveSpeed = 4.0f;
	[Tooltip("Sprint speed of the character in m/s")]
	public float SprintSpeed = 6.0f;
	[Tooltip("Rotation speed of the character")]
	public float RotationSpeed = 1.0f;
	[Tooltip("Acceleration and deceleration")]
	public float SpeedChangeRate = 10.0f;

	[Space(10)]
	[Tooltip("The height the player can jump")]
	public float JumpHeight = 1.2f;
	[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
	public float Gravity = -9.8f;

	[Space(10)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float JumpTimeout = 0.1f;
	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float FallTimeout = 0.15f;

	public bool sliding = false;
	public float SlideScale = 0.5f;
	public float SlidingAnimation = 0.3f;
	public float SlideDuration = 1f;
	public float SlideCameraAngle = 15f;

	public GameObject weapon;
	public bool attacking = false;

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

	// World 
	//public GameObject world;
	//private bool isWorldRotating = false;

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
	}

	private void Update()
	{
		JumpAndGravity();
		GroundedCheck();
		GroundSlide();
		Move();
		GroundSlide();
		Attack();
	}

	private void LateUpdate()
	{
		CameraRotation();
	}

	private void GroundedCheck()
	{
		// set sphere position, with offset
		Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
		Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
	}

	private void CameraRotation()
	{
		if (sliding) return;

		if (!Grounded)
		{
			CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(15f, 0.0f, 0.0f);
		}
		else
		{
			CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(0, 0.0f, 0.0f);
		}
	}

	private void Move()
	{
		//_controller.Move(new Vector3(0, 0, 5) * Time.deltaTime);

		// set target speed based on move speed, sprint speed and if sprint is pressed
		float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
		if (_input.move.x == 0) targetSpeed = 0.0f;


		float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
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
		Vector3 inputDirection = transform.right * _input.move.x;

		// move the player
		_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
	}

	public void CancelJump()
	{
		_verticalVelocity = 0;
    }

	private void JumpAndGravity()
	{
		if (Grounded) // jump 
		{
			// reset the fall timeout timer
			_fallTimeoutDelta = FallTimeout;
			_verticalVelocity = Mathf.Max(-3f, _verticalVelocity);

			if (_input.jump && _jumpTimeoutDelta <= 0.0f)
				_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
			if (_jumpTimeoutDelta >= 0.0f)
				_jumpTimeoutDelta -= Time.deltaTime;
		}
		else // fall
		{
			_jumpTimeoutDelta = JumpTimeout;
			if (_fallTimeoutDelta >= 0.0f)
				_fallTimeoutDelta -= Time.deltaTime;

			_input.jump = false;

		}

		// apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
		if (_verticalVelocity < _terminalVelocity)
		{
			_verticalVelocity += Gravity * Time.deltaTime;
		}
	}

	private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f) lfAngle += 360f;
		if (lfAngle > 360f) lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.name.Contains("Bullet")) return;
		if (attacking)
		{
			Destroy(other.gameObject);
			if (!Grounded)
			{
				_verticalVelocity += Mathf.Max(_verticalVelocity, 6f);
			}
		}
	}

	private void GroundSlide()
	{
		if (_input.slide && !sliding && Grounded)
		{
			sliding = true;
			_input.slide = false;

			StartCoroutine(GroundSlideGenerator());
		}
	}

	private IEnumerator GroundSlideGenerator()
	{
		float curTime = 0;
		Vector3 startScale = transform.localScale;
		Time.timeScale = 0.3f;
		while (curTime < SlidingAnimation)
		{
			curTime += Time.deltaTime;
			var scale = Vector3.Lerp(startScale, new Vector3(1, SlideScale, 1), curTime / SlidingAnimation);
			transform.localScale = scale;

			var rotation = Vector3.Lerp(Vector3.zero, new Vector3(-SlideCameraAngle, 0, 0), curTime / SlidingAnimation);
			CinemachineCameraTarget.transform.rotation = Quaternion.Euler(rotation);
			yield return null;
		}

		yield return new WaitForSecondsRealtime(SlideDuration);
		Time.timeScale = 1f;
		sliding = false;

		curTime = 0;
		startScale = transform.localScale;
		while (curTime < SlidingAnimation)
		{
			curTime += Time.deltaTime;
			var scale = Vector3.Lerp(startScale, Vector3.one, curTime / SlidingAnimation);
			transform.localScale = scale;

			var rotation = Vector3.Lerp(new Vector3(-SlideCameraAngle, 0, 0), Vector3.zero, curTime / SlidingAnimation);
			CinemachineCameraTarget.transform.rotation = Quaternion.Euler(rotation);
			yield return null;
		}

	}

	private void Attack()
	{
		if (!attacking && _input.attack)
		{
			_input.attack = false;
			attacking = true;
			StartCoroutine(AttackGenerator());
		}
	}

	private IEnumerator AttackGenerator()
	{
		float curTime = 0;
		float accAngle = 0;
		while (curTime < 0.3)
		{
			curTime += Time.deltaTime;
			float curAngle = Mathf.Min(180 * curTime / 0.3f, 180f);
			weapon.transform.Rotate(Vector3.right, curAngle - accAngle);
			accAngle = curAngle;
			yield return null;
		}

		while (curTime < 0.2)
		{
			curTime += Time.deltaTime;
			var rotation = Vector3.Lerp(Vector3.zero, new Vector3(180, 0, 0), curTime / 0.2f);
			weapon.transform.localRotation = Quaternion.Euler(rotation);
			yield return null;
		}
		attacking = false;
	}
}
