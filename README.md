# coretemplate

This repo provides a barebones template for creating multi-platform applications running on CoreCLR. It links in a small set of build targets which set up a very basic build environment from https://github.com/mellinoe/corebuild.

#Requirements
* Visual Studio 2015 (MSBuild 14.0)
* Nuget.exe 3.x or equivalent

# Setup/Build instructions
First, clone the repo recursively (````git clone --recursive https://github.com/mellinoe/coretemplate <path>````)

Building in VS is easier and more user-friendly, but not completely necessary.

## Building in VS
1. Open src/TestProj.sln
2. Select your configuration (Windows, Ubuntu, OSX)
3. Build
4. Optionally, set a break point and debug the program

## Building outside VS
1. run nuget restore or equivalent to restore the packages for the solution
2. run msbuild /p:Configuration=Windows_Debug /p:Platform=x64 (or other configuration)

# Build Artifacts
* The intermediate objects from the build will be placed in ````<root>/bin/obj/<arch>/<platform>_<config>/````
* The binaries will be placed in ````<root>/bin/<arch>/<platform>_config/````
* Executable projects' bin/ directories will include a full runtime and all library dependencies, ready to be copied to the target platform and run. If you are building on Windows, you can immediately run the <project>.exe file and launch your program on CoreCLR.

# Notes
* If you elect to use corerun instead of coreconsole, you will need to modify the vsdebug.targets file. It assumes that you have renamed coreconsole.exe -> <project>.exe, and will therefore not work correctly if you are using corerun.exe instead.
* There are various random warnings that are emitted by NuGet when building and restoring packages. Check the "Build Succeeded/Failed" message in the bottom-left of VS to determine if the build was actually successful or not.
* When building a library rather than an executable, you should be able to use the regular VS 2015 portable class library project template. Adding project references to such projects from your executable application project should work. (NOTE: I have not tested this with this exact setup).
