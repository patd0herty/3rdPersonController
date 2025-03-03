using Godot;
using System;

public partial class Player : CharacterBody3D
{
	// Constants
	// Velocity based constants
	private const float SPEED = 5.0f;
	private const float RUN_SPEED = 25.0f;
	private const float JUMP_VELOCITY = 4.5f;

	// Camera constants
	private const float NORMAL_FOV = 75.0f;
	private const float FAST_FOV = 90.0f;

	private float _currentSpeed = 0f;
	private bool _sprinting = false;

	// Node References
	private AnimationPlayer _animator;
	private Camera3D _camera;
	private Node3D _cameraRotation;

	// Mouse variables
	[Export(PropertyHint.Range, "0.0,1.0")] float _mouseSensitivity = 0.01f;
	[Export] private float _tiltLimit = Mathf.DegToRad(75.0f);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		_currentSpeed = SPEED;
		_animator = GetNode<AnimationPlayer>("RootScene/AnimationPlayer");
		_camera = GetNode<Camera3D>("%Camera3D");
		_cameraRotation = GetNode<Node3D>("%CameraRotation");


		_animator.Play("Idle");
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (Input.IsActionPressed("Sprint"))
		{
			_sprinting = true;
			_currentSpeed = RUN_SPEED;
		}
		else
		{
			_sprinting = false;
			_currentSpeed = SPEED;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("Jump") && IsOnFloor())
		{
			velocity.Y = JUMP_VELOCITY;
		}

		// Get the input direction and handle the movement/deceleration.
		Vector2 inputDir = Input.GetVector("Left", "Right", "Forward", "Backward");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			if (!IsOnFloor())
			{
				_currentSpeed = SPEED;
			}
			velocity.X = direction.X * _currentSpeed;
			velocity.Z = direction.Z * _currentSpeed;

			// Change walking animation
			if (_sprinting)
			{
				_animator.Play("Run");
			}
			else
			{
				_animator.Play("Walk");
			}
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, SPEED);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, SPEED);
			_animator.Play("Idle");
		}

		_updateFOV();

		Velocity = velocity;
		MoveAndSlide();
	}

	private void _updateFOV()
	{
		float tempFOV;
		float delta;
		if (_sprinting)
		{
			tempFOV = FAST_FOV;
			delta = 2.0f;
		}
		else
		{
			tempFOV = NORMAL_FOV;
			delta = 1.0f;
		}
		
}

    public override void _UnhandledInput(InputEvent @event)
    {
		// Check if it was a mouse movement
        if(@event is InputEventMouseMotion)
		{
			InputEventMouseMotion temp = (@event as InputEventMouseMotion);
			// Rotate the body
			RotateY(Mathf.DegToRad(temp.Relative.X));

			// Then we want to rotate the camera
			// but C# is weird we can't modify components of rotation individually,
			// We need to modify the entire vec3 for rotation, so we make a copy and edit that
			Vector3 tempRotation = _cameraRotation.Rotation;	// Copy of the rotation
			// change the rotation by the difference with the mouse movement
			tempRotation.X = _cameraRotation.Rotation.X - (temp.Relative.Y * _mouseSensitivity);
			// Clamp the rotation so nothing crazy happens
			tempRotation.X = Mathf.Clamp(tempRotation.X, -_tiltLimit, _tiltLimit);
			// Finally we assign the modified rotation
			_cameraRotation.Rotation = tempRotation;
        }

		// Check if the input was us quitting
		if (Input.IsActionJustPressed("Quit"))
		{
			// Close the game
			GetTree().Quit();
		}
    }
}
