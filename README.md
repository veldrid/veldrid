# Veldrid

_Note: This repository contains a new in-development version of Veldrid. Older versions of Veldrid can be found in the [Veldrid-legacy](https://github.com/mellinoe/Veldrid-Legacy) repository._

Veldrid is a cross-platform, graphics API-agnostic rendering library for .NET. It allows you to use a single set of rendering commands and run your application on a number of different graphics API's. With some small exceptions, applications written against Veldrid will run on any of its backends without modification.

Supported backends:

* Direct3D 11
* Vulkan
* Metal
* OpenGL 3

[Veldrid documentation site](https://mellinoe.github.io/veldrid-docs/)

Veldrid is available on NuGet:

[![NuGet](https://img.shields.io/nuget/v/Veldrid.svg)](https://www.nuget.org/packages/Veldrid)

Pre-release versions of Veldrid are also available from MyGet: https://www.myget.org/feed/mellinoe/package/nuget/Veldrid

![Sponza](https://i.imgur.com/QDaXwWL.jpg)

### Build instructions

Veldrid  uses the standard .NET Core tooling. [Install the tools](https://www.microsoft.com/net/download/core) and build normally (`dotnet restore && dotnet build`).

Run the RenderDemo program to see a quick demonstration of the rendering capabilities of the library.

### Using the library

The recommended way to reference Veldrid is via source. Veldrid includes some debug-only validation code which is disabled in release builds.
