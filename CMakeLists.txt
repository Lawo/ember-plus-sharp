cmake_minimum_required(VERSION 2.8)

# make sure "true" is treated as a boolean constant
cmake_policy(SET CMP0012 NEW)

set(SUB_PROJECT_DIR ${CMAKE_CURRENT_LIST_DIR})

add_subdirectory(Doc)
