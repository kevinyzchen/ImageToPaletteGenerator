extends ColorSpaceExtractor
class_name ImageToColorspaceExtractor

func _ready():
	._ready()
	var processor_script = load("res://ImageProcessor.cs")
	processor = processor_script.new()
	assert(extract_button != null, "no extract button found")
	
func _process_data():
	processor.Process(input_path, output_path, {"min_k" : 4, "max_k" : 12})


func _on_InputFolderDialog_dir_selected(dir):
	input_path = dir + "/"
	input_folder_text.text = dir + "/"


