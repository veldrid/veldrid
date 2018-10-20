#version 450

layout(set = 0, binding = 0) uniform texture2D SourceTexture;
layout(set = 0, binding = 1) uniform sampler SourceSampler;

layout(location = 0) in vec2 fsin_TexCoords;
layout(location = 0) out vec4 OutputColor;

layout(constant_id = 103) const bool OutputFormatSrgb = true;

vec3 LinearToSrgb(vec3 linear)
{
    // http://chilliant.blogspot.com/2012/08/srgb-approximations-for-hlsl.html
    vec3 S1 = sqrt(linear);
    vec3 S2 = sqrt(S1);
    vec3 S3 = sqrt(S2);
    vec3 sRGB = 0.662002687 * S1 + 0.684122060 * S2 - 0.323583601 * S3 - 0.0225411470 * linear;
    return sRGB;
}

void main()
{
    vec4 color = texture(sampler2D(SourceTexture, SourceSampler), fsin_TexCoords);

    if (!OutputFormatSrgb)
    {
        color = vec4(LinearToSrgb(color.rgb), 1);
    }

    OutputColor = color;
}
