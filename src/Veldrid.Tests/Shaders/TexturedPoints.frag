#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable
struct Veldrid_Tests_Shaders_TexturedPoints_Vertex
{
    vec2 Position;
};

struct Veldrid_Tests_Shaders_TexturedPoints_FragmentInput
{
    vec4 Position;
};

layout(set = 0, binding = 1) uniform texture2D Tex;
layout(set = 0, binding = 2) uniform sampler Smp;

vec4 FS( Veldrid_Tests_Shaders_TexturedPoints_FragmentInput input_)
{
    return texture(sampler2D(Tex, Smp), vec2(0.5f, 0.5f));
}


layout(location = 0) out vec4 _outputColor_;

void main()
{
    Veldrid_Tests_Shaders_TexturedPoints_FragmentInput input_;
    input_.Position = gl_FragCoord;
    vec4 output_ = FS(input_);
    _outputColor_ = output_;
}
