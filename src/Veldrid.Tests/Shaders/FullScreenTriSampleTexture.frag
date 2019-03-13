#version 450

layout(set = 0, binding = 0) uniform texture1D Tex;
layout(set = 0, binding = 1) uniform sampler Smp;

layout(location = 0) in vec2 fsin_UV;
layout(location = 0) out vec4 fsout_Color0;

void main()
{
    fsout_Color0 = texture(sampler1D(Tex, Smp), fsin_UV.x);
}
