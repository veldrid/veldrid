#version 450

layout (set = 1, binding = 0) uniform texture2D SpriteTex; 
layout (set = 1, binding = 1) uniform sampler SpriteSampler; 

layout(location = 0) in vec2 fsin_TexCoords;
layout(location = 1) in vec4 fsin_Tint;

layout(location = 0) out vec4 outputColor;

void main()
{
    outputColor = texture(sampler2D(SpriteTex, SpriteSampler), fsin_TexCoords) * fsin_Tint;
}