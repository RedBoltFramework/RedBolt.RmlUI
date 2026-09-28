# Copyright (c) 2026 RedBolt and contributors.
# This work is licensed under the terms of the MIT license.
# For a copy, see <https://opensource.org/licenses/MIT> or the accompanying LICENSE file.

redbolt_require_path("${REDBOLT_SHARPCPP_ROOT}" "REDBOLT_SHARPCPP_ROOT")

if(NOT REDBOLT_DEPENDENCIES_GENERATOR_DLL)
    set(REDBOLT_DEPENDENCIES_GENERATOR_DLL
        "${PROJECT_SOURCE_DIR}/build/bin/$<CONFIG>/host/tools/RedBolt.RmlUI.Generator.Dependencies.dll"
        CACHE FILEPATH "RedBolt.RmlUI dependency generator assembly")
endif()

file(GLOB_RECURSE REDBOLT_DEPENDENCIES_GENERATOR_INPUTS CONFIGURE_DEPENDS
    "${CMAKE_CURRENT_SOURCE_DIR}/*.cs"
    "${CMAKE_CURRENT_SOURCE_DIR}/*.csproj")
list(APPEND REDBOLT_DEPENDENCIES_GENERATOR_INPUTS
    "${PROJECT_SOURCE_DIR}/src/RedBolt/Native/RmlUi/RmlHarfBuzzFontEngine.hpp"
    "${PROJECT_SOURCE_DIR}/src/RedBolt/Native/RmlUi/RmlUiValueMarshallers.hpp")

set(REDBOLT_DEPENDENCIES_GENERATOR_INPUTS "${REDBOLT_DEPENDENCIES_GENERATOR_INPUTS}" CACHE INTERNAL "RedBolt.RmlUI generator inputs")

set(REDBOLT_RMLUI_GENERATED_DIR "${REDBOLT_NATIVE_GENERATED_DIR}/rmlui" CACHE INTERNAL "RmlUi binding output")
set(REDBOLT_RMLUI_BINDINGS_STEM "RedBolt.Bindings.RmlUi" CACHE INTERNAL "RmlUi binding stem")
set(REDBOLT_RMLUI_BINDINGS_CS "${REDBOLT_RMLUI_GENERATED_DIR}/${REDBOLT_RMLUI_BINDINGS_STEM}.Bindings.g.cs" CACHE INTERNAL "RmlUi generated C# binding")
set(REDBOLT_RMLUI_BINDINGS_CPP "${REDBOLT_RMLUI_GENERATED_DIR}/${REDBOLT_RMLUI_BINDINGS_STEM}.Bindings.g.cpp" CACHE INTERNAL "RmlUi generated C++ binding")
set(REDBOLT_RMLUI_BINDINGS_JSON "${REDBOLT_RMLUI_GENERATED_DIR}/${REDBOLT_RMLUI_BINDINGS_STEM}.json" CACHE INTERNAL "RmlUi binding metadata")
set(REDBOLT_RMLUI_PARSE_HEADER "${REDBOLT_RMLUI_GENERATED_DIR}/RmlUiAll.hpp" CACHE INTERNAL "RmlUi parse header")
set(REDBOLT_RMLUI_SUPPORT_INCLUDE_DIR "${PROJECT_SOURCE_DIR}/src/RedBolt/Native/RmlUi" CACHE INTERNAL "RedBolt RmlUi support include directory" FORCE)
file(MAKE_DIRECTORY "${REDBOLT_RMLUI_GENERATED_DIR}")

file(CONFIGURE OUTPUT "${REDBOLT_RMLUI_PARSE_HEADER}" CONTENT [=[
#pragma once
#include <RmlUi/Debugger.h>
#include <RmlUi/Core/Core.h>
#include <RmlUi/Core/Context.h>
#include <RmlUi/Core/ContextInstancer.h>
#include <RmlUi/Core/ComputedValues.h>
#include <RmlUi/Core/DataModelHandle.h>
#include <RmlUi/Core/Element.h>
#include <RmlUi/Core/ElementDocument.h>
#include <RmlUi/Core/Event.h>
#include <RmlUi/Core/EventListener.h>
#include <RmlUi/Core/EventListenerInstancer.h>
#include <RmlUi/Core/Factory.h>
#include <RmlUi/Core/FileInterface.h>
#include <RmlUi/Core/FontEngineInterface.h>
#include <RmlUi/Core/Plugin.h>
#include <RmlUi/Core/RenderInterfaceCompatibility.h>
#include <RmlUi/Core/SystemInterface.h>
#include <RmlUi/Core/Variant.h>
#include <RmlHarfBuzzFontEngine.hpp>
]=] @ONLY)
