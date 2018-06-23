#version 450

layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color0;

void main()
{
    fsout_Color0 = fsin_Color;
}
