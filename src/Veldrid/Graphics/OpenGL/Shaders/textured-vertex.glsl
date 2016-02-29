#version 330

layout(std140) uniform projectionMatrixUniform
{
    mat4 projection_matrix;
};

layout(std140) uniform viewMatrixUniform
{
    mat4 view_matrix;
};

layout(std140) uniform worldMatrixUniform
{
    mat4 world_matrix;
};

uniform sampler2D surfaceTexture;

in vec3 in_position;
in vec2 in_texCoord;

out vec2 texCoord;

void main()
{
    // works only for orthogonal modelview
    gl_Position = projection_matrix * view_matrix * world_matrix * vec4(in_position, 1);
    texCoord = in_texCoord;
}
