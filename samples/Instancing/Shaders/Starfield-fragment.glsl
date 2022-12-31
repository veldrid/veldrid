#version 450

#define HASHSCALE3 vec3(443.897, 441.423, 437.195)
#define STARFREQUENCY 0.01

layout(set = 0, binding = 0) uniform InvCameraInfo
{
    mat4 InvProj;
    mat4 InvView;
};

layout(location = 0) in vec4 fsin_ClipPos;
layout(location = 1) in vec3 fsin_TexCoord;

layout(location = 0) out vec4 outputColor;

// Hash function by Dave Hoskins (https://www.shadertoy.com/view/4djSRW)
float hash33(vec3 p3)
{
    p3 = fract(p3 * HASHSCALE3);
    p3 += dot(p3, p3.yxz + vec3(19.19, 19.19, 19.19));
    return fract((p3.x + p3.y) * p3.z + (p3.x + p3.z) * p3.y + (p3.y + p3.z) * p3.x);
}

vec3 starField(vec3 pos)
{
    vec3 color = vec3(0, 0, 0);
    float threshhold = (1.0 - STARFREQUENCY);
    float rnd = hash33(pos);
    if (rnd >= threshhold)
    {
        float starCol = pow(abs((rnd - threshhold) / (1.0 - threshhold)), 16.0);
        color += vec3(starCol, starCol, starCol);
    }
    return color;
}

void main()
{
    // View Coordinates
    vec4 viewCoords = InvProj * fsin_ClipPos;
    viewCoords.z = -1.0f;
    viewCoords.w = 0.0f;

    vec3 worldDirection = (InvView * viewCoords).xyz;
    worldDirection = normalize(worldDirection);

    worldDirection = floor(worldDirection * 700) / 700;

    outputColor =  vec4(starField(worldDirection), 1.0);
}
