#include <metal_stdlib>
using namespace metal;

struct VertexOut
{
    float4 Position [[ position ]];
    float4 Color;
};

fragment float4 FS(
    VertexOut input [[ stage_in ]])
{
    return input.Color;
}
