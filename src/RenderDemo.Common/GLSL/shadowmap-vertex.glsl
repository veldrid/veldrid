#version 330 core

uniform ProjectionMatrixBuffer
{
    mat4 projection;
};

uniform ViewMatrixBuffer
{
    mat4 view;
};

uniform WorldMatrixBuffer
{
    mat4 world;
};

in vec3 in_position;

void main()
{
    vec4 worldPos = world * vec4(in_position, 1);
    vec4 viewPos = view * worldPos;
    vec4 screenPos = projection * viewPos;
    gl_Position = screenPos;
    // Normalize depth range
    gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;
}