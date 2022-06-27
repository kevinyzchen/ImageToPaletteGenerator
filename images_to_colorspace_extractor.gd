extends ColorSpaceExtractor
class_name ImageToColorspaceExtractor

func _ready():
	._ready()
	var processor_script = load("res://ImageToColorSpaceProcessor.cs")
	processor = processor_script.new()
	assert(extract_button != null, "no extract button found")
	
func _process_data():
#	processor.Process(input_path, output_path, {"min_k" : 3, "max_k" : 12, "interval" : 40.0, "threshold" : .1, "method" : "KMEANS"})
	processor.Process(input_path, output_path, {"eps" : 15.0, "min_pts" : 3, "interval" : 40.0, "threshold" : .1, "method" : "DBSCAN"})


func _on_InputFolderDialog_dir_selected(dir):
	input_path = dir + "/"
	input_folder_text.text = dir + "/"


