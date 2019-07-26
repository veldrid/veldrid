#version 450

layout(set = 0, binding = 0) uniform UnusedBuffer_0
{
    vec4 not_used_0;
};

layout(set = 0, binding = 1) uniform UnusedBuffer_1
{
    mat4 not_used_1;
    vec4 not_used_2;
    vec4 not_used_3;
    vec4 not_used_4;
    vec4 not_used_5;
};

layout(set = 0, binding = 2) uniform OrthoBuffer
{
    mat4 Ortho;
};

layout(location = 0) in vec2 vsin_position;

void main()
{
    gl_PointSize = 1;
    gl_Position = Ortho * vec4(vsin_position, 0, 1);
}
