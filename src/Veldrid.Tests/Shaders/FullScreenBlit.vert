#version 450

layout (location = 0) out vec2 fsin_UV;

const vec4 QuadInfos[4] = 
{
    vec4(-1, 1, 0, 0),
    vec4(1, 1, 1, 0),
    vec4(-1, -1, 0, 1),
    vec4(1, -1, 1, 1),
};

void main()
{
    gl_Position = vec4(QuadInfos[gl_VertexIndex].xy, 0, 1);
    fsin_UV = QuadInfos[gl_VertexIndex].zw;
}
