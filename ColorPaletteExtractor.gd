extends Node
class_name ColorPaletteExtractor

var image_processor 
export(NodePath) var extract_button_path
onready var extract_button = get_node(extract_button_path)
var input_path : String 
var output_path : String

func _ready():
	var image_processor_script = load("res://ImageProcessor.cs")
	image_processor = image_processor_script.new()
	assert(extract_button != null, "no extract button found")

func _on_Extract_button_up():
	image_processor.StartProcessing(input_path, output_path)

func _on_InputFolder_text_changed(new_text):
	pass # Replace with function body.

func _on_OutputFolder_text_changed(new_text):
	pass # Replace with function body.

func _on_ColorSpaceName_text_changed(new_text):
	pass # Replace with function body.
