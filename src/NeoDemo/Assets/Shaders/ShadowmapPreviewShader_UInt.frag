#version 450

layout(set = 0, binding = 2) uniform utexture2D Tex;
layout(set = 0, binding = 3) uniform sampler TexSampler;

layout(location = 0) in vec2 fsin_0;
layout(location = 0) out vec4 OutputColor;

layout(constant_id = 101) const bool TextureCoordinatesInvertedY = false;

void main()
{
    vec2 texCoord = fsin_0;
    if (TextureCoordinatesInvertedY)
    {
        texCoord.y *= -1;
    }

    // ivec2 size = textureSize(usampler2D(Tex, TexSampler), 0);
    ivec2 size = ivec2(960, 540);
    OutputColor = vec4(texelFetch(usampler2D(Tex, TexSampler), ivec2(texCoord * size), 0) / 255.0);
    OutputColor.a = 1;
}
