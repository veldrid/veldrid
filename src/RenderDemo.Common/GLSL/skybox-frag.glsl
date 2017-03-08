#version 330 core

in vec3 TexCoords;
out vec4 color;

uniform samplerCube Skybox;

void main()
{    
    color = texture(Skybox, TexCoords);
}