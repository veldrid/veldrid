#version 330 core

in vec2 Position;
in vec4 Color;

out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;
    fsin_Color = Color;
}
