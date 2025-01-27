extends Node

class_name Player_Animation

@onready var _animationPlayer := get_node("AnimationPlayer") as AnimationPlayer

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	_animationPlayer.play("Idle");


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass

func PlayAnimation(animation)-> void:
	_animationPlayer.play(animation);
	
