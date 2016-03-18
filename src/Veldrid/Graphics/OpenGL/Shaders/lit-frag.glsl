#version 330

layout(std140) uniform LightBuffer
{
    vec4 diffuseColor;
    vec3 lightDirection;
};

uniform sampler2D surfaceTexture;

in vec2 texCoord;
in vec3 normal;

out vec4 outputColor;

void main()
{
    outputColor = texture2D(surfaceTexture, texCoord);

    vec4 ambientColor = vec4(.4, .4, .4, 1);

    vec4 color = texture2D(surfaceTexture, texCoord);
    vec3 lightDir = -normalize(lightDirection);
    float effectiveness = dot(normal, lightDir);
    float lightEffectiveness = clamp(effectiveness, 0, 1);
    vec4 lightColor = clamp(diffuseColor * lightEffectiveness, 0, 1);
    outputColor = clamp((lightColor * color) + (ambientColor * color), 0, 1);

}
