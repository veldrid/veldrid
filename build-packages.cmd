@echo off
@setlocal

dotnet restore src\Veldrid.sln

set UseStableVersions=true

dotnet pack -c Release src\Veldrid.OpenGLBindings\Veldrid.OpenGLBindings.csproj
dotnet pack -c Release src\Veldrid.MetalBindings\Veldrid.MetalBindings.csproj
dotnet pack -c Release src\Veldrid\Veldrid.csproj
dotnet pack -c Release src\Veldrid.Utilities\Veldrid.Utilities.csproj
dotnet pack -c Release src\Veldrid.ImGui\Veldrid.ImGui.csproj
dotnet pack -c Release src\Veldrid.ImageSharp\Veldrid.ImageSharp.csproj
dotnet pack -c Release src\Veldrid.SDL2\Veldrid.SDL2.csproj
dotnet pack -c Release src\Veldrid.StartupUtilities\Veldrid.StartupUtilities.csproj
dotnet pack -c Release src\Veldrid.VirtualReality\Veldrid.VirtualReality.csproj
dotnet pack -c Release src\Veldrid.RenderDoc\Veldrid.RenderDoc.csproj
