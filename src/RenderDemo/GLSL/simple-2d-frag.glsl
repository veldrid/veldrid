#version 140

uniform sampler2D SurfaceTexture;

in vec2 out_texCoord;

out vec4 outputColor;

void main()
{
    float r = texture2D(SurfaceTexture, out_texCoord).r;
    outputColor = vec4(r, r, r, 1);
}
