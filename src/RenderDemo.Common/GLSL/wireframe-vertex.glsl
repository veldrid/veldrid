#version 330 core

uniform ProjectionMatrixBuffer
{
    mat4 projection;
};

uniform ViewMatrixBuffer
{
    mat4 view;
};

uniform WorldMatrixBuffer
{
    mat4 world;
};

in vec3 in_position;
in vec4 in_color;

out vec4 fsin_color;

void main()
{

    vec4 worldPosition = world * vec4(in_position, 1);
    vec4 viewPosition = view * worldPosition;
    gl_Position = projection * viewPosition;

    fsin_color = in_color;
}
