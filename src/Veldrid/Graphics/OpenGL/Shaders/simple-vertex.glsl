#version 330

layout(std140) uniform projectionMatrixUniform
{
    mat4 projection_matrix;
};

layout(std140) uniform modelviewMatrixUniform
{
    mat4 modelview_matrix;
};

in vec3 in_position;
in vec4 in_color;

out vec4 color;

void main()
{
    // works only for orthogonal modelview
    gl_Position = projection_matrix * modelview_matrix * vec4(in_position, 1);
    color = in_color;
}
