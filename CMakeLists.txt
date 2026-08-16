# Copyright (c) 2026 RedBolt and contributors.
# This work is licensed under the terms of the MIT license.
# For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

cmake_minimum_required(VERSION 3.28)

project(RedBoltRmlUI VERSION 0.1.0 LANGUAGES C CXX)

set(BUILD_SHARED_LIBS OFF CACHE BOOL "Build dependencies statically" FORCE)
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)
set(CMAKE_CXX_SCAN_FOR_MODULES OFF)

if(EMSCRIPTEN)
    add_compile_options(-mbulk-memory)
endif()
if(ANDROID)
    add_compile_options(-fsigned-char)
    add_link_options("-Wl,-z,max-page-size=16384")
endif()
if(MINGW)
    set(CMAKE_CXX_LINK_LIBRARY_USING_WHOLE_ARCHIVE
        "LINKER:--whole-archive"
        "<LINK_ITEM>"
        "LINKER:--no-whole-archive")
    set(CMAKE_CXX_LINK_LIBRARY_USING_WHOLE_ARCHIVE_SUPPORTED TRUE)
endif()

set(REDBOLT_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../RedBolt" CACHE PATH "RedBolt source root")
set(REDBOLT_SHARPCPP_ROOT "${REDBOLT_ROOT}/extern/SharpCpp" CACHE PATH "SharpCpp source root")
set(REDBOLT_DEPENDENCY_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/extern" CACHE PATH "RedBolt.RmlUI dependency cache")
set(REDBOLT_GENERATED_ROOT "${CMAKE_BINARY_DIR}/generated")
set(REDBOLT_NATIVE_GENERATED_DIR "${REDBOLT_GENERATED_ROOT}/redbolt-rmlui-native")
set(REDBOLT_RUNTIME_OUTPUT_DIR "${CMAKE_CURRENT_SOURCE_DIR}/build/bin/$<CONFIG>/linux-x64/extern/runtimes/linux-x64/native" CACHE PATH "Native runtime output")
set(REDBOLT_PDB_OUTPUT_DIR "${CMAKE_CURRENT_SOURCE_DIR}/build/bin/$<CONFIG>/linux-x64/extern/pdb" CACHE PATH "Native PDB output")
set(REDBOLT_RMLUI_NATIVE_LIBRARY_NAME "RedBolt.Native.RmlUi" CACHE INTERNAL "RmlUi native library name")

find_program(DOTNET_EXECUTABLE dotnet REQUIRED)
function(redbolt_require_path path description)
    if(NOT EXISTS "${path}")
        message(FATAL_ERROR "${description} does not exist: ${path}")
    endif()
endfunction()

list(PREPEND CMAKE_MODULE_PATH "${REDBOLT_ROOT}/cmake/Modules")
set(CPM_DONT_UPDATE_MODULE_PATH ON CACHE BOOL "Use RedBolt CMake modules" FORCE)
set(CPM_SOURCE_CACHE "${REDBOLT_DEPENDENCY_ROOT}" CACHE PATH "CPM source cache" FORCE)
unset(CPM_DIRECTORY CACHE)
unset(CPM_VERSION CACHE)
include("${REDBOLT_ROOT}/cmake/CPM.cmake")
set(FETCHCONTENT_BASE_DIR "${CMAKE_BINARY_DIR}/deps")

set(SHARPCPP_BUILD_E2E OFF CACHE BOOL "Build SharpCpp E2E" FORCE)
set(SHARPCPP_BUILD_RBFX OFF CACHE BOOL "Build SharpCpp rbfx bindings" FORCE)
add_subdirectory("${REDBOLT_SHARPCPP_ROOT}" "${CMAKE_BINARY_DIR}/deps/SharpCpp" EXCLUDE_FROM_ALL)
include(extern/RmlUi.cmake)

if(ANDROID)
    if(ANDROID_ABI STREQUAL "x86_64")
        set(REDBOLT_BINDING_TARGET_TRIPLE "x86_64-linux-android24")
        set(_redbolt_core_platform "android-x64")
    elseif(ANDROID_ABI STREQUAL "arm64-v8a")
        set(REDBOLT_BINDING_TARGET_TRIPLE "aarch64-linux-android24")
        set(_redbolt_core_platform "android-arm64")
    else()
        message(FATAL_ERROR "Unsupported Android ABI: ${ANDROID_ABI}")
    endif()
    get_filename_component(_redbolt_android_compiler_bin "${CMAKE_CXX_COMPILER}" DIRECTORY)
    get_filename_component(_redbolt_android_toolchain_root "${_redbolt_android_compiler_bin}" DIRECTORY)
    set(REDBOLT_BINDING_SYSROOT "${_redbolt_android_toolchain_root}/sysroot")
elseif(WIN32)
    set(_redbolt_core_platform "win-x64")
else()
    set(_redbolt_core_platform "linux-x64")
endif()
set(REDBOLT_BINDING_CXX_COMPILER "${CMAKE_CXX_COMPILER}")
set(REDBOLT_BINDING_DEFINES)
if(CMAKE_CONFIGURATION_TYPES)
    list(APPEND REDBOLT_BINDING_DEFINES "$<$<NOT:$<CONFIG:Debug>>:NDEBUG>")
elseif(CMAKE_BUILD_TYPE MATCHES "^(Release|Distribution|ReleaseASAN|ReleaseUBSAN|ReleaseTSAN|ReleaseCoverage)$")
    list(APPEND REDBOLT_BINDING_DEFINES NDEBUG)
endif()

include("${REDBOLT_ROOT}/cmake/RedBoltSharpCpp.cmake")
add_subdirectory(src/RedBolt/Generator/Dependencies)
add_subdirectory(src/RedBolt/Native/RmlUi)

if(EMSCRIPTEN)
    set(_redbolt_rmlui_library_type STATIC)
else()
    set(_redbolt_rmlui_library_type SHARED)
endif()
add_library(redbolt-rmlui-native-rmlui ${_redbolt_rmlui_library_type}
    "${REDBOLT_RMLUI_BINDINGS_CPP}"
    src/RedBolt/Native/RmlUi/RmlHarfBuzzFontEngine.cpp)
add_dependencies(redbolt-rmlui-native-rmlui
    redbolt-generate-rmlui-bindings
    rmlui_core
    rmlui_debugger)
target_link_libraries(redbolt-rmlui-native-rmlui PRIVATE SharpCpp::NativeSupport)
if(NOT EMSCRIPTEN)
    add_library(redbolt-core-harfbuzz SHARED IMPORTED)
    if(CMAKE_CONFIGURATION_TYPES)
        foreach(_redbolt_core_configuration IN LISTS CMAKE_CONFIGURATION_TYPES)
            string(TOUPPER "${_redbolt_core_configuration}" _redbolt_core_configuration_upper)
            set(_redbolt_core_native_dir
                "${REDBOLT_ROOT}/build/bin/${_redbolt_core_configuration}/${_redbolt_core_platform}/extern/runtimes/${_redbolt_core_platform}/native")
            if(WIN32)
                set_target_properties(redbolt-core-harfbuzz PROPERTIES
                    IMPORTED_LOCATION_${_redbolt_core_configuration_upper} "${_redbolt_core_native_dir}/RedBolt.Native.HarfBuzz.dll"
                    IMPORTED_IMPLIB_${_redbolt_core_configuration_upper} "${REDBOLT_ROOT}/build/native/win-x64/lib/${_redbolt_core_configuration}/libRedBolt.Native.HarfBuzz.dll.a")
            else()
                set_target_properties(redbolt-core-harfbuzz PROPERTIES
                    IMPORTED_LOCATION_${_redbolt_core_configuration_upper} "${_redbolt_core_native_dir}/libRedBolt.Native.HarfBuzz.so")
            endif()
        endforeach()
    else()
        set(_redbolt_core_native_dir
            "${REDBOLT_ROOT}/build/bin/${CMAKE_BUILD_TYPE}/${_redbolt_core_platform}/extern/runtimes/${_redbolt_core_platform}/native")
        set_target_properties(redbolt-core-harfbuzz PROPERTIES
            IMPORTED_LOCATION "${_redbolt_core_native_dir}/libRedBolt.Native.HarfBuzz.so")
    endif()
    target_link_libraries(redbolt-rmlui-native-rmlui PRIVATE
        "$<LINK_LIBRARY:WHOLE_ARCHIVE,$<TARGET_FILE:rmlui_core>,$<TARGET_FILE:rmlui_debugger>>"
        redbolt-core-harfbuzz)
endif()
target_include_directories(redbolt-rmlui-native-rmlui PRIVATE
    "${REDBOLT_RMLUI_SOURCE_ROOT}/Include"
    "${CMAKE_CURRENT_SOURCE_DIR}/src/RedBolt/Native/RmlUi"
    "${REDBOLT_ROOT}/extern/harfbuzz/src"
    "${REDBOLT_ROOT}/src/RedBolt/Native/HarfBuzz")
target_compile_definitions(redbolt-rmlui-native-rmlui PRIVATE REDBOLT_NATIVE_BUILD RMLUI_NO_THIRDPARTY_CONTAINERS RMLUI_STATIC_LIB)
set_target_properties(redbolt-rmlui-native-rmlui PROPERTIES
    OUTPUT_NAME "${REDBOLT_RMLUI_NATIVE_LIBRARY_NAME}"
    RUNTIME_OUTPUT_DIRECTORY "${REDBOLT_RUNTIME_OUTPUT_DIR}"
    LIBRARY_OUTPUT_DIRECTORY "${REDBOLT_RUNTIME_OUTPUT_DIR}"
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib/$<CONFIG>"
    WINDOWS_EXPORT_ALL_SYMBOLS ON
    C_VISIBILITY_PRESET default)
if(CMAKE_CONFIGURATION_TYPES)
    foreach(_redbolt_configuration IN LISTS CMAKE_CONFIGURATION_TYPES)
        string(TOUPPER "${_redbolt_configuration}" _redbolt_configuration_upper)
        set_target_properties(redbolt-rmlui-native-rmlui PROPERTIES
            RUNTIME_OUTPUT_DIRECTORY_${_redbolt_configuration_upper} "${REDBOLT_RUNTIME_OUTPUT_DIR}"
            LIBRARY_OUTPUT_DIRECTORY_${_redbolt_configuration_upper} "${REDBOLT_RUNTIME_OUTPUT_DIR}")
    endforeach()
endif()
if(WIN32)
    set_target_properties(redbolt-rmlui-native-rmlui PROPERTIES PREFIX "")
endif()

if(NOT EMSCRIPTEN)
    file(GENERATE
        OUTPUT "${REDBOLT_GENERATED_ROOT}/native-modules/$<CONFIG>/RMLUI.json"
        CONTENT "{\n  \"module\": \"RMLUI\",\n  \"managedSelector\": \"RedBolt.Bindings.RmlUi\",\n  \"nativeLibrary\": \"RedBolt.Native.RmlUi\",\n  \"requires\": [],\n  \"requirementClosure\": [],\n  \"artifacts\": [\"$<TARGET_FILE_NAME:redbolt-rmlui-native-rmlui>\"]\n}\n")
endif()

add_custom_target(redbolt-rmlui-native-all DEPENDS redbolt-rmlui-native-rmlui)
add_custom_target(redbolt-rmlui-android-native DEPENDS redbolt-rmlui-native-rmlui)

if(EMSCRIPTEN)
    set(REDBOLT_RMLUI_WASM_ARCHIVE "${CMAKE_BINARY_DIR}/lib/Release/wasm/RedBolt.RmlUI.Native.Web.a")
    set(REDBOLT_RMLUI_WASM_MRI "${CMAKE_BINARY_DIR}/redbolt-rmlui-wasm-archives.mri")
    file(GENERATE OUTPUT "${REDBOLT_RMLUI_WASM_MRI}" CONTENT
        "create ${REDBOLT_RMLUI_WASM_ARCHIVE}\naddlib $<TARGET_FILE:redbolt-rmlui-native-rmlui>\naddlib $<TARGET_FILE:rmlui_core>\naddlib $<TARGET_FILE:rmlui_debugger>\nsave\nend\n")
    add_custom_command(
        OUTPUT "${REDBOLT_RMLUI_WASM_ARCHIVE}"
        COMMAND "${CMAKE_COMMAND}"
            "-DREDBOLT_EMSCRIPTEN_ENV=${_redbolt_emscripten_environment}"
            "-DREDBOLT_ARCHIVER=${CMAKE_AR}"
            "-DREDBOLT_MRI_FILE=${REDBOLT_RMLUI_WASM_MRI}"
            "-DREDBOLT_OUTPUT_ARCHIVE=${REDBOLT_RMLUI_WASM_ARCHIVE}"
            -P "${REDBOLT_ROOT}/cmake/MergeStaticArchives.cmake"
        DEPENDS redbolt-rmlui-native-rmlui rmlui_core rmlui_debugger
        VERBATIM)
    add_custom_target(redbolt-rmlui-wasm-native DEPENDS "${REDBOLT_RMLUI_WASM_ARCHIVE}")
endif()
