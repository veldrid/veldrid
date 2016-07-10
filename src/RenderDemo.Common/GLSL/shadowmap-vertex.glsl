#version 140

uniform mat4 ProjectionMatrix;

uniform mat4 ViewMatrix;

uniform mat4 WorldMatrix;

in vec3 in_position;

void main()
{
    vec4 worldPos = WorldMatrix * vec4(in_position, 1);
    vec4 viewPos = ViewMatrix * worldPos;
    vec4 screenPos = ProjectionMatrix * viewPos;
    gl_Position = screenPos;
}