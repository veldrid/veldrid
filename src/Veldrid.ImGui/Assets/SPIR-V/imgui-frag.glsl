#version 450

#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

layout(set = 1, binding = 0) uniform texture2D FontTexture;
layout(set = 0, binding = 1) uniform sampler FontSampler;

layout (location = 0) in vec4 color;
layout (location = 1) in vec2 texCoord;
layout (location = 0) out vec4 outputColor;

layout (constant_id = 1) const bool OutputLinear = false;

// http://chilliant.blogspot.com/2012/08/srgb-approximations-for-hlsl.html
vec3 LinearToSrgb(vec3 linear)
{
  vec3 S1 = sqrt(linear);
  vec3 S2 = sqrt(S1);
  vec3 S3 = sqrt(S2);
  return 0.585122381 * S1 + 0.783140355 * S2 - 0.368262736 * S3;
}

void main()
{
    outputColor = color * texture(sampler2D(FontTexture, FontSampler), texCoord);
    if (!OutputLinear)
    {
        outputColor = vec4(LinearToSrgb(outputColor.rgb), outputColor.a);
    }
}
