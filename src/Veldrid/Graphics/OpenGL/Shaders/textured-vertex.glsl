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
	vec4 worldPos = world_matrix * vec4(in_position, 1);
	vec4 viewPos = view_matrix * worldPos;
	vec4 screenPos = projection_matrix * viewPos;
    gl_Position = screenPos;

    texCoord = in_texCoord; // Pass along unchanged.
}
