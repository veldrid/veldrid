#version 450

layout(set = 0, binding = 2) uniform textureCube CubeTexture;
layout(set = 0, binding = 3) uniform sampler CubeSampler;

layout(location = 0) in vec3 fsin_0;
layout(location = 0) out vec4 OutputColor;

void main()
{
    OutputColor = texture(samplerCube(CubeTexture, CubeSampler), fsin_0);
}
