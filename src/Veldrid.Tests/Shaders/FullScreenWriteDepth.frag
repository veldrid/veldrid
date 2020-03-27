#version 450

layout(location = 0) in vec2 fsin_UV;

layout (set = 0, binding = 0) uniform FramebufferInfo
{
    vec4 OutputSize;
};

void main()
{
    vec2 uv = gl_FragCoord.xy - vec2(0.5, 0.5);
    float xComp = uv.x;
    float yComp = uv.y * OutputSize.x;
    float val = (yComp + xComp) / (OutputSize.x * OutputSize.y);
    gl_FragDepth = val;
}
