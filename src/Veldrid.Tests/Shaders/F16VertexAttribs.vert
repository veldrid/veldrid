#version 450

struct Info
{
    uint ColorNormalizationFactor;
    float padding0;
    float padding1;
    float padding2;
};

layout(set = 0, binding = 0) uniform InfoBuffer
{
    Info _Info;
};

layout(set = 0, binding = 1) uniform OrthoBuffer
{
    mat4 _Ortho;
};

layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;
layout(location = 0) out vec4 fsin_Color;

void main()
{
    gl_Position = _Ortho * vec4(Position, 0, 1);
    fsin_Color = Color / _Info.ColorNormalizationFactor;
    fsin_Color.w = 1;
    gl_PointSize = 1;
}
