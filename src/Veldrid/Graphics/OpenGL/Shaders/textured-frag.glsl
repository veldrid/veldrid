#version 330

uniform sampler2D surfaceTexture;

in vec2 texCoord;

out vec4 outputColor;

void main()
{
    outputColor = texture2D(surfaceTexture, texCoord);
    //outputColor = vec4(.5, .5, .5, 1);
}
