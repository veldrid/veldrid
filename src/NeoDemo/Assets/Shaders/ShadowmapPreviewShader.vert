#version 450

struct SizeInfo
{
    vec2 Position;
    vec2 Size;
};

layout(set = 0, binding = 0) uniform Projection
{
    mat4 _Projection;
};

layout(set = 0, binding = 1) uniform SizePos
{
    SizeInfo _SizePos;
};

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 TexCoord;
layout(location = 0) out vec2 fsin_0;

layout(constant_id = 100) const bool ClipSpaceInvertedY = true;

void main()
{
    fsin_0 = TexCoord;
    vec2 scaledInput = (Position * _SizePos.Size) + _SizePos.Position;
    gl_Position = _Projection * vec4(scaledInput, 0, 1);
    if (ClipSpaceInvertedY)
    {
        gl_Position.y *= -1;
    }
}
