extends CharacterBody3D

# Movement Variables--------------
const SPEED = 5.0
const RUNSPEED = 25.0
const JUMP_VELOCITY = 4.5
var _currentSpeed = 0.0
@onready var _sprinting = false

# Camera Variables ----------------
# Camera references 
@onready var _camera := %Camera3D as Camera3D
@onready var _cameraRotation := %CameraRotation as Node3D

# Camera values
const NORMAL_FOV = 75.0
const FAST_FOV = 90.0


#Animation Variables
@onready var _animator := get_node("Root Scene") as Player_Animation

@export_range(0.0, 1.0) var mouse_sensitivity = 0.01
@export var tilt_limit = deg_to_rad(75)

func _ready() -> void:
	_currentSpeed = SPEED;

func _physics_process(delta: float) -> void:
	# Add the gravity.
	if not is_on_floor():
		velocity += get_gravity() * delta
		# Clamp vertical velocity so we don't go flying with weird physics collisions
		#NEED TO FIX #velocity.y = clampf(velocity.y, -20* get_gravity().y, 2* JUMP_VELOCITY)
		
	if Input.is_action_pressed("Sprint"):
		_sprinting = true
		_currentSpeed = RUNSPEED
	else:
		_sprinting = false
		_currentSpeed = SPEED
	
	# Handle jump.
	if Input.is_action_just_pressed("Jump") and is_on_floor():
		velocity.y = JUMP_VELOCITY

	# Get the input direction and handle the movement/deceleration.
	var input_dir := Input.get_vector("Left", "Right", "Forward", "Backward")
	var direction := (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()
	if direction:
		if not is_on_floor():
			_currentSpeed = SPEED
		velocity.x = direction.x * _currentSpeed
		velocity.z = direction.z * _currentSpeed
		# change walk animation
		if _sprinting:
			_animator.PlayAnimation("Run")
		else:
			_animator.PlayAnimation("Walk")
	else:
		velocity.x = move_toward(velocity.x, 0, SPEED)
		velocity.z = move_toward(velocity.z, 0, SPEED)
		_animator.PlayAnimation("Idle")
	
	_updateFOV()

	move_and_slide()

# handled specific inputs, camera movement and quitting
func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		rotate_y(deg_to_rad(event.relative.x))
		_cameraRotation.rotation.x -= event.relative.y * mouse_sensitivity
		_cameraRotation.rotation.x = clampf(_cameraRotation.rotation.x, -tilt_limit, tilt_limit)
		
	if Input.is_action_just_pressed("Quit"):
		# Quit the game when we press Escape key
		get_tree().quit()

func _updateFOV()-> void:
	var tempFOV # target FOV
	var delta # delta/step in Tween
	if _sprinting:
		tempFOV = FAST_FOV
		delta = 2.0
	else:
		tempFOV = NORMAL_FOV
		delta = 1.0
	_camera.fov = move_toward(_camera.fov, tempFOV, delta)
