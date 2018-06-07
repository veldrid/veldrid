#version 450

layout(set = 0, binding = 0) uniform texture2D SourceTexture;
layout(set = 0, binding = 1) uniform sampler SourceSampler;

layout(location = 0) in vec2 fsin_TexCoords;
layout(location = 0) out vec4 OutputColor;

void main()
{
    //OutputColor = vec4(gl_FragCoord.x / 1280, gl_FragCoord.y / 720, 0, 1);
    //OutputColor = vec4(fsin_TexCoords, 0, 1);
    OutputColor = texture(sampler2D(SourceTexture, SourceSampler), fsin_TexCoords);
}
