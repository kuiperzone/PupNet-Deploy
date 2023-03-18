@echo off
:: This is a dummy bat file used for demonstration and test. It outputs a few variables
:: and creates a dummy file in the application directory which will be detected by the program.

echo
echo ===========================
echo POST_PUBLISH BAT SCRIPT
echo ===========================
echo

:: Some useful macros / environment variables
echo BUILD_ARCH: %BUILD_ARCH%
echo BUILD_TARGET: %BUILD_TARGET%
echo PUBLISH_BIN: %PUBLISH_BIN%
echo

:: Directory and file will be detected by HelloWorld Program
echo Do work...
@echo on
mkdir "%PUBLISH_BIN%/subdir"
copy NUL "%PUBLISH_BIN%/subdir/file.test"
@echo off

echo
echo ===========================
echo POST_PUBLISH END
echo ===========================
echo
