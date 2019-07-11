#version 450

layout (set = 0, binding = 0) uniform OrthographicProjection
{
    mat4 Projection;
};

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 Size;
layout(location = 2) in vec4 Tint;
layout(location = 3) in float Rotation;

layout(location = 0) out vec2 fsin_TexCoords;
layout(location = 1) out vec4 fsin_Tint;

const vec4 Quads[4]= vec4[4](
    vec4(-.5, .5, 0, 0),
    vec4(.5, .5, 1, 0),
    vec4(-.5, -.5, 0, 1),
    vec4(.5, -.5, 1, 1)
);

vec2 rotate(vec2 v, float a)
{
    float s = sin(a);
    float c = cos(a);
    mat2 m = mat2(c, -s, s, c);
    return m * v;
}

void main()
{
    vec4 src = Quads[gl_VertexIndex];

    vec2 srcPos = src.xy;
    srcPos = rotate(srcPos, Rotation);

    srcPos.x = (srcPos.x * Size.x) + Position.x + (Size.x / 2);
    srcPos.y = (srcPos.y * Size.y) + Position.y + (Size.y / 2);

    gl_Position = Projection * vec4(srcPos, 0, 1);

    fsin_TexCoords = src.zw;
    fsin_Tint = Tint;
}
