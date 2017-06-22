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

// Per-Vertex
in vec3 in_position;
// Per-Instance
in vec3 in_offset;
in vec4 in_color;

out vec4 out_color;

void main()
{
    vec4 worldPos = world * vec4(in_position + in_offset, 1);
    vec4 viewPos = view * worldPos;
    vec4 projPos = projection * viewPos;
    gl_Position = projPos;
    // Normalize depth range
    gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;

    out_color = in_color;
}
