#version 330 core
#ifdef GL_ES
#extension GL_ARB_gpu_shader5 : enable
#endif

uniform sampler2D surfaceTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = color * texture(surfaceTexture, texCoord);
}
