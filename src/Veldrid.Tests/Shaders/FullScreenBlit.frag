#version 450

layout(set = 0, binding = 0) uniform texture2D Input;
layout(set = 0, binding = 1) uniform sampler InputSampler;

layout(location = 0) in vec2 fsin_UV;
layout(location = 0) out vec4 fsout_Color0;

layout (constant_id = 0) const bool InvertTexY = false;

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
    vec2 uv = fsin_UV;
    if (InvertTexY) { uv.y = 1 - uv.y; }
    fsout_Color0 = texture(sampler2D(Input, InputSampler), uv);
}
