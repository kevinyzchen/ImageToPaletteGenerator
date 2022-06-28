extends Node
class_name ColorSpaceExtractor

var processor 
var input_path : String 
var output_path : String
var preview_objects = []
var colorspace_name : String

export(NodePath) var preview_grid_path
onready var preview_grid : GridContainer = get_node(preview_grid_path)


export(NodePath) var extract_button_path
onready var extract_button = get_node(extract_button_path)

export(NodePath) var output_folder_text_path
onready var output_folder_text : LineEdit = get_node(output_folder_text_path)

export(NodePath) var output_folder_dialog_path
onready var output_folder_dialog :  FileDialog = get_node(output_folder_dialog_path)

export(NodePath) var input_folder_text_path
onready var input_folder_text : LineEdit = get_node(input_folder_text_path)

export(NodePath) var input_folder_dialog_path
onready var input_folder_dialog : FileDialog = get_node(input_folder_dialog_path)

func _ready():
	var processor_script = load("res://GodotFileProcessor.cs")
	processor = processor_script.new()
	assert(extract_button != null, "no extract button found")
	print(processor)

func _process_data():
	processor.Process(input_path, output_path, {"min_k" : 3, "max_k" : 12, "trials" : 5, "interval" : 40.0, "threshold" : .1})

func _on_InputFileDialog_file_selected(path):
	input_path = path
	input_folder_text.text = path



func clear_preview():
	for i in preview_objects:
		i.call_deferred("queue_free")
	preview_objects.clear()

func _on_Preview_button_up():
	preview()


func preview():
	clear_preview()
	var colors_from_files : Dictionary = processor.Preview()
	for file in colors_from_files:
		var color_list = colors_from_files[file]
		for color in color_list:
			var new_rect = ColorRect.new()
			new_rect.color = color
			new_rect.rect_min_size = Vector2(32,32)
			preview_grid.add_child(new_rect)
			preview_objects.append(new_rect)
			
func _on_Extract_button_up():
	yield(process_data(), "completed")

func process_data():
	_process_data()
	yield(get_tree(), "idle_frame")


func _on_OutputFolderDir_button_up():
	output_folder_dialog.visible = true
	
func _on_OutputFolderDialog_dir_selected(dir):
	output_path = dir + "/"
	output_folder_text.text = dir + "/"

func _on_OutputFolder_text_changed(new_text):
	output_path = new_text

func _on_InputFolder_text_changed(new_text):
	input_path = new_text

func _on_InputFolderDir_button_up():
	input_folder_dialog.visible = true
	
func _on_ColorSpaceName_text_changed(new_text):
	colorspace_name = new_text
