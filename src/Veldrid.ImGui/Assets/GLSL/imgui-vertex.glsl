#version 330 core

uniform ProjectionMatrixBuffer
{
    mat4 projection_matrix;
};

in vec2 in_position;
in vec2 in_texCoord;
in vec4 in_color;

out vec4 color;
out vec2 texCoord;

vec3 SrgbToLinear(vec3 srgb)
{
    return srgb * (srgb * (srgb * 0.305306011 + 0.682171111) + 0.012522878);
}

void main()
{
    gl_Position = projection_matrix * vec4(in_position, 0, 1);
    color = vec4(SrgbToLinear(in_color.rgb), in_color.a);
	texCoord = in_texCoord;
}
