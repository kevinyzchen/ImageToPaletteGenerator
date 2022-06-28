extends VBoxContainer
class_name ExtractedCard

var _colors = []
var _preview_image : ImageTexture

onready var tex = $TextureRect
onready var palette_box = $HBoxContainer
onready var label = $Label


func load_data(string_name : String, colors, image : ImageTexture):
	printerr(image)
	tex.texture = image
	_preview_image = image
	_colors = colors
	label.text = string_name
	make_palette_preview()
	
func make_palette_preview():
	print(_colors.size(), "size")
	for i in _colors.size():
		var rect = ColorRect.new()
		rect.rect_min_size = Vector2(32,32)
		rect.color = _colors[i]
		palette_box.add_child(rect)
