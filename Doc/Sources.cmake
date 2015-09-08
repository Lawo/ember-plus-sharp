# Apparently, the directory containing c# files needs to appear before any
# directories containing C++ files.

set(input_dir
  ${SUB_PROJECT_DIR}/Lawo
  ${SUB_PROJECT_DIR}/Lawo.EmberPlus/S101
  ${SUB_PROJECT_DIR}/Lawo.EmberPlus/Ember
  ${SUB_PROJECT_DIR}/Lawo.EmberPlus/Glow
  ${SUB_PROJECT_DIR}/Lawo.EmberPlus/Model
  ${SUB_PROJECT_DIR}/Doc
)

set(example_dir
  ${SUB_PROJECT_DIR}
  ${SUB_PROJECT_DIR}/Doc
)

set(image_dir
  ${SUB_PROJECT_DIR}/Doc/Images
)