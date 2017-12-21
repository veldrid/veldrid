#include <metal_stdlib>
using namespace metal;

struct VertexOut
{
    float4 Position [[ position ]];
    float2 TexCoord;
};

fragment float4 FS(
    VertexOut input [[ stage_in ]],
    texture2d<float> SurfaceTexture [[ texture(0) ]],
    sampler SurfaceSampler [[ sampler(0) ]])
{
    constexpr sampler s(coord::normalized,
                    address::repeat,
                    filter::linear);
    // return SurfaceTexture.sample(s, input.TexCoord);
    return SurfaceTexture.sample(SurfaceSampler, input.TexCoord);
}
