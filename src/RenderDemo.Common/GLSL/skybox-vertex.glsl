#version 330 core

in vec3 position;
out vec3 TexCoords;

uniform ProjectionMatrixBuffer
{
    mat4 projection;
};

uniform ViewMatrixBuffer
{
    mat4 view;
};

void main()
{
    gl_Position = (projection * view * vec4(position, 1.0)).xyww;  
    TexCoords = position;
}  