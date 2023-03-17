#!/bin/bash
# This is a dummy script only used for test. It output a few variables and creates
# a dummy file in the application directory which will be detected by the program.

echo
echo "==========================="
echo "POST_PUBLISH SCRIPT"
echo "==========================="
echo

# Some useful macros / environment variables
echo "BUILD_ARCH: ${BUILD_ARCH}"
echo "BUILD_TARGET: ${BUILD_TARGET}"
echo "BUILD_SHARE: ${BUILD_SHARE}"
echo "PUBLISH_BIN: ${PUBLISH_BIN}"
echo

# Directory and file will be detected by HelloWorld Program
echo "Do work..."
mkdir -p "${PUBLISH_BIN}/subdir"
touch "${PUBLISH_BIN}/subdir/file.test"

echo
echo "==========================="
echo "POST_PUBLISH END"
echo "==========================="
echo