extends ColorSpaceExtractor
class_name VideoToColorspaceExtractor

func _ready():
	._ready()
	var processor_script = load("res://VideoToColorSpaceProcessor.cs")
	processor = processor_script.new()
	assert(extract_button != null, "no extract button found")
	print(processor)
	
func _process_data():
#	processor.Process(input_path, output_path, {"min_k" : 3, "max_k" : 12, "interval" : 40.0, "threshold" : .1, "method" : "KMEANS"})
	processor.Process(input_path, output_path, {"eps" : 150.0, "min_pts" : 3, "interval" : 40.0, "threshold" : .1, "method" : "DBSCAN"})

func _on_InputFileDialog_file_selected(path):
	input_path = path
	input_folder_text.text = path
