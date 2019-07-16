#version 450 core
layout(location = 0) in vec2 fsin_uv;
layout(location = 0) out vec4 fsout_color;
layout(set = 0, binding = 1) uniform texture2D Tex;
layout(set = 0, binding = 2) uniform sampler Smp;

void main()
{
    fsout_color = texture(sampler2D(Tex, Smp), fsin_uv);
}