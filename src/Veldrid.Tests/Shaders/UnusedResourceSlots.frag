#version 450

layout(set = 0, binding = 3) uniform texture2D Unused_Tex_0;
layout(set = 0, binding = 4) uniform texture2D Unused_Tex_1;

layout(set = 0, binding = 5) uniform texture2D Tex;
layout(set = 0, binding = 6) uniform sampler Smp;

layout(location = 0) out vec4 _outputColor_;

void main()
{
    _outputColor_ = texture(sampler2D(Tex, Smp), vec2(0.5f, 0.5f));
}
