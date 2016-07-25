#version 140

in vec3 position;
out vec3 TexCoords;

uniform mat4 ProjectionMatrixBuffer;
uniform mat4 ViewMatrixBuffer;

void main()
{
    gl_Position = (ProjectionMatrixBuffer * ViewMatrixBuffer * vec4(position, 1.0)).xyww;  
    TexCoords = position;
}  