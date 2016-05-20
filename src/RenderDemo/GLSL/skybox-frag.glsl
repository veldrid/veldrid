#version 140

in vec3 TexCoords;
out vec4 color;

uniform samplerCube skybox;

void main()
{    
    color = texture(skybox, TexCoords);
}