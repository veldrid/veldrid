#version 330

uniform sampler2D surfaceTexture;

in vec4 color;
in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = color * texture2D(surfaceTexture, texCoord);
}
