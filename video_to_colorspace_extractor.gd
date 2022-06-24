extends ColorSpaceExtractor
class_name VideoToColorspaceExtractor

func _ready():
	._ready()
	var processor_script = load("res://VideoProcessor.cs")
	processor = processor_script.new()
	assert(extract_button != null, "no extract button found")
	print(processor)
	
func _process_data():
	processor.Process(input_path, output_path, {"min_k" : 3, "max_k" : 12, "interval" : 20.0})

func _on_InputFileDialog_file_selected(path):
	input_path = path
	input_folder_text.text = path
