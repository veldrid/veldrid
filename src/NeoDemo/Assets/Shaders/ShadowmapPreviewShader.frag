#version 450

layout(set = 0, binding = 2) uniform texture2D Tex;
layout(set = 0, binding = 3) uniform sampler TexSampler;

layout(location = 0) in vec2 fsin_0;
layout(location = 0) out vec4 OutputColor;

void main()
{
    OutputColor = texture(sampler2D(Tex, TexSampler), fsin_0);
}
