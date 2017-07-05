#version 330 core

uniform WorldMatrixBuffer
{
    mat4 world;
};

uniform ProjectionMatrixBuffer
{
    mat4 projection;
};

uniform sampler2D SurfaceTexture;

in vec3 in_position;
in vec2 in_texCoord;

out vec2 out_texCoord;

void main()
{
    vec4 out_position = vec4(in_position, 1);
    out_position = world * out_position;
    out_position = projection * out_position;
    gl_Position = out_position;
    out_texCoord = in_texCoord;
}
