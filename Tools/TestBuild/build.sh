#!/bin/bash
#
# Copyright (c) 2020 - Odd Strabo <oddstr13@openshell.no>
#
#
# The Unlicense
# =============
#
# This is free and unencumbered software released into the public domain.
#
# Anyone is free to copy, modify, publish, use, compile, sell, or
# distribute this software, either in source code form or as a compiled
# binary, for any purpose, commercial or non-commercial, and by any
# means.
#
# In jurisdictions that recognize copyright laws, the author or authors
# of this software dedicate any and all copyright interest in the
# software to the public domain. We make this dedication for the benefit
# of the public at large and to the detriment of our heirs and
# successors. We intend this dedication to be an overt act of
# relinquishment in perpetuity of all present and future rights to this
# software under copyright law.
#
# THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
# EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
# MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
# IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
# OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
# ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
# OTHER DEALINGS IN THE SOFTWARE.
#
# For more information, please refer to <http://unlicense.org/>
#

MY=$(dirname $(realpath -s "${0}"))
JPRM="jprm"

DEFAULT_REPO_DIR="${MY}/test_repo"
DEFAULT_REPO_URL="http://localhost:8080"

PLUGIN=${1:-${PLUGIN:-.}}

ARTIFACT_DIR=${ARTIFACT_DIR:-"${MY}/artifacts"}
mkdir -p "${ARTIFACT_DIR}"

JELLYFIN_REPO=${JELLYFIN_REPO:-${DEFAULT_REPO_DIR}}
JELLYFIN_REPO_URL=${JELLYFIN_REPO_URL:-${DEFAULT_REPO_URL}}

# Each segment of the version is a 16bit number.
# Max number is 65535.
VERSION_SUFFIX=${VERSION_SUFFIX:-$(date -u +%y%m.%d%H.%M%S)}

meta_version=$(grep -Po '^ *version: * "*\K[^"$]+' "${PLUGIN}/build.yaml")
VERSION=${VERSION:-$(echo $meta_version | sed 's/\.[0-9]*\.[0-9]*\.[0-9]*$/.'"$VERSION_SUFFIX"'/')}

# Ensure we use Debug configuration for testing
CONFIGURATION=${CONFIGURATION:-Debug}

find "${PLUGIN}" -name project.assets.json -exec rm -v '{}' ';'

# Initialize repo if it doesn't exist
if [ ! -f "${JELLYFIN_REPO}/manifest.json" ]; then
    mkdir -p "${JELLYFIN_REPO}"
    $JPRM repo init "${JELLYFIN_REPO}"
fi

# Build the plugin using jprm, passing the configuration
# Use --dotnet-configuration to specify Debug
zipfile=$($JPRM plugin build "${PLUGIN}" --output="${ARTIFACT_DIR}" --version="${VERSION}" --dotnet-configuration="${CONFIGURATION}") && {
    $JPRM repo add --url=${JELLYFIN_REPO_URL} "${JELLYFIN_REPO}" "${zipfile}"
}
exit $?
