extends Node
class_name ColorSpaceExtractor

var processor 
var preview_objects = []
var card = preload("res://Scenes/ExtractedCard.tscn")

### Passed Parameters ###
var input_path : String 
var output_path : String
var min_k : int = 3 #Min amount of groups check for kmeans
var max_k : int = 9 #Max amount of groups to check for kmeans
var interval : float = 5.0 # Takes an image sample every "interval" seconds if the file is a video
var threshold : float = .05 # Avoids having colors that are closer than the "threshold"
var trials : int = 1 #Amount of times to test a K value with kmeans before settling on the best init group
var max_resolution : int = 256 #the down sampled size of analyzed files
##UI
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
	var processor_script = load("res://Scripts/GodotFileProcessor.cs")
	processor = processor_script.new()
	assert(extract_button != null, "no extract button found")

func _process_data():
	#Calls wrapper GodotFileProcessor.cs
	processor.Process(input_path, output_path, {"min_k" : min_k, "max_k" : max_k, "trials" : trials, "interval" : interval, "threshold" : threshold, "max_res" : max_resolution})
	
func preview():
	clear_preview()
	var result : Dictionary = processor.Preview()
	printerr(result)
	for i in result["palettes"].size():
		var color_list = result["palettes"][i]
		var thumbnail_path = result["thumbnails"][i]
		var string_name = result["names"][i]
		var card = create_new_preview_card(string_name, color_list, thumbnail_path)
		preview_objects.append(card)
		
func create_new_preview_card(string_name : String, colors , thumbnail_path : String) -> ExtractedCard:
	var new_card = card.instance()
	var imageTexture = ImageTexture.new()
	var image = Image.new()
	image.load(thumbnail_path);
	imageTexture.create_from_image(image);
	preview_grid.add_child(new_card)
	new_card.name= string_name
	new_card.load_data(string_name, colors, imageTexture)
	return new_card
	
func clear_preview():
	for i in preview_objects:
		i.call_deferred("queue_free")
	preview_objects.clear()

func process_data():
	_process_data()
	yield(get_tree(), "idle_frame")

##CALLBACKS

func _on_InputFileDialog_file_selected(path):
	input_path = path
	input_folder_text.text = path

func _on_InputFileDialog_dir_selected(path):
	input_path = path
	input_folder_text.text = path

func _on_Preview_button_up():
	preview()

func _on_Extract_button_up():
	yield(process_data(), "completed")
	preview()
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

func _on_Trials_value_changed(value):
	trials = value

func _on_MinK_value_changed(value):
	min_k = value

func _on_MaxK_value_changed(value):
	max_k = value

func _on_Interval_value_changed(value):
	interval = value

func _on_Threshold_value_changed(value):
	threshold = value


func _on_MaxResolution_value_changed(value):
	max_resolution = value



