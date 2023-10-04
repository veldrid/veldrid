#version 450

layout(location = 0) in vec3 A_V3;
layout(location = 1) in vec4 B_V4;
layout(location = 2) in vec2 C_V2;
layout(location = 3) in vec4 D_V4;

void main()
{
    vec4 result = vec4(0, 0, 0, 1);
    result += vec4(A_V3, 0);
    result += B_V4;
    result += vec4(C_V2, 0, 0);
    result += D_V4;
    gl_Position = result;
}
