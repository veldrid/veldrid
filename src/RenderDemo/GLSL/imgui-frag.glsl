#version 140

uniform sampler2D surfaceTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = color * texture(surfaceTexture, texCoord);
}
