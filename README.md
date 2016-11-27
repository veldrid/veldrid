##Veldrid

An experimental renderer with OpenGL and Direct3D backends.

![Sponza](http://i.imgur.com/4TlmVuh.png)

![Sponza-GIF](http://i.imgur.com/70y6sJq.gif)

*(Severely compressed gif preview)*

## Build instructions (Windows)

* Have Visual Studio 2015 Update 3 installed
* Have latest NuGet extension installed (Tools -> Extensions and Updates -> Updates)
* Uncheck these options under Tools ->NuGet Package Manager -> Package Manager Settings:
![image](https://cloud.githubusercontent.com/assets/8918977/20651200/7cba5744-b495-11e6-812c-d9cb4d51b0af.png)
* Have .NET Core SDK installed: https://go.microsoft.com/fwlink/?LinkID=835014
* After cloning the repository, run `dotnet restore <cloned-path>`.

# Visual Studio
Change the configuration to Windows_Debug, x64. Build everything and set "RenderDemo" as the startup project

# Command line
Open a Visual Studio developer command prompt. Run `msbuild` on RenderDemo.csproj. Open RenderDemo.exe.
