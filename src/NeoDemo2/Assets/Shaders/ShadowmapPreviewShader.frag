#version 450

layout(set = 0, binding = 2) uniform texture2D Tex;
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

    OutputColor = texture(sampler2D(Tex, TexSampler), texCoord);
}
