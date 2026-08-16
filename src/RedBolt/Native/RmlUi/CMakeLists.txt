# Copyright (c) 2026 RedBolt and contributors.
# This work is licensed under the terms of the MIT license.
# For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

redbolt_add_native_binding_generation(
    TARGET redbolt-generate-rmlui-bindings
    SURFACE rmlui
    HEADER "${REDBOLT_RMLUI_PARSE_HEADER}"
    HEADER_INCLUDE_PATH "RmlUiAll.hpp"
    FILE_STEM "${REDBOLT_RMLUI_BINDINGS_STEM}"
    OUTPUT_DIR "${REDBOLT_RMLUI_GENERATED_DIR}"
    OUTPUT_METADATA "${REDBOLT_RMLUI_BINDINGS_JSON}"
    OUTPUT_CS "${REDBOLT_RMLUI_BINDINGS_CS}"
    OUTPUT_CPP "${REDBOLT_RMLUI_BINDINGS_CPP}"
    NATIVE_LIBRARY_NAME "${REDBOLT_RMLUI_NATIVE_LIBRARY_NAME}"
    INCLUDE_DIRS "${REDBOLT_RMLUI_SOURCE_ROOT}/Include" "${REDBOLT_RMLUI_SUPPORT_INCLUDE_DIR}"
    DEFINES RMLUI_NO_THIRDPARTY_CONTAINERS
    OWNED_HEADERS "${REDBOLT_RMLUI_PARSE_HEADER}" "${REDBOLT_RMLUI_SOURCE_ROOT}/Include/RmlUi"
        "${REDBOLT_RMLUI_SUPPORT_INCLUDE_DIR}/RmlHarfBuzzFontEngine.hpp")

add_dependencies(redbolt-generate-rmlui-bindings rmlui_core)
