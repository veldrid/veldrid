#version 330 core

uniform ProjectionMatrixBuffer
{
    mat4 projection_matrix;
};

uniform ModelViewMatrixBuffer
{
    mat4 modelview_matrix;
};

in vec3 in_position;
in vec4 in_color;

out vec4 color;

void main()
{
    gl_Position = projection_matrix * modelview_matrix * vec4(in_position, 1);
    // Normalize depth range
    gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;
    color = in_color;
}
