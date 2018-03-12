#!/usr/bin/env bash

script_dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

xcrun -sdk macosx metal $script_dir/UnalignedBufferCopy.metal -o $script_dir/UnalignedBufferCopy.macos.air
xcrun -sdk macosx metallib $script_dir/UnalignedBufferCopy.macos.air -o $script_dir/MTL_UnalignedBufferCopy_macOS.metallib

xcrun -sdk iphoneos metal $script_dir/UnalignedBufferCopy.metal -o $script_dir/UnalignedBufferCopy.ios.air
xcrun -sdk iphoneos metallib $script_dir/UnalignedBufferCopy.ios.air -o $script_dir/MTL_UnalignedBufferCopy_iOS.metallib

rm $script_dir/UnalignedBufferCopy.macos.air
rm $script_dir/UnalignedBufferCopy.ios.air
