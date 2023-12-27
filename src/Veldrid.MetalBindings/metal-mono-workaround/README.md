# Metal Mono Workaround

On iOS, simply defining a P/Invoke method for `MTLBlitCommandEncoder.copyFromBuffer` and invoking it blows up the application. To work around that, we're defining a method that has a different signature that's friendly with Mono, which redirects back to the original method.

This library should be referenced by any project that uses Veldrid. 

To generate the library, build the Xcode project on both iPhone and iPhone Simulator architectures, and execute the following to generate the XCFramework:
```bash
xcodebuild -create-xcframework -framework ./Release-iphoneos/metal_mono_workaround.framework -framework ./Release-iphonesimulator/metal_mono_workaround.framework -output metal-mono-workaround.xcframework
```

Afterwards, reference the XCFramework in your project and it should work correctly.
