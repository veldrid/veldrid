#include <metal_stdlib>
using namespace metal;

// This must be kept in sync with MTLUnalignedBufferCopyInfo.cs
struct CopyInfo
{
    uint32_t SrcOffset;
    uint32_t DstOffset;
    uint32_t CopySize;
};

kernel void copy_bytes(
    device uint8_t* src [[ buffer(0) ]],
    device uint8_t* dst [[ buffer(1) ]],
    constant CopyInfo& info [[ buffer(2) ]])
{
    for (uint32_t i = 0; i < info.CopySize; i++)
    {
        dst[i + info.DstOffset] = src[i + info.SrcOffset];
    }
}