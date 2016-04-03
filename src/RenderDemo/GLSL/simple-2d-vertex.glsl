#version 140

uniform mat4 WorldMatrixBuffer;
uniform mat4 ProjectionMatrixBuffer;
uniform sampler2D SurfaceTexture;

in vec3 in_position;
in vec2 in_texCoord;

out vec2 out_texCoord;

void main()
{
    vec4 out_position = vec4(in_position, 1);
    out_position = WorldMatrixBuffer * out_position;
    out_position = ProjectionMatrixBuffer * out_position;
    gl_Position = out_position;
    out_texCoord = in_texCoord;
}
