#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable
struct Veldrid_Tests_Shaders_UIntVertexAttribs_Vertex
{
    vec2 Position;
    uvec4 Color_Int;
};

struct Veldrid_Tests_Shaders_UIntVertexAttribs_FragmentInput
{
    vec4 Position;
    vec4 Color;
};

struct Veldrid_Tests_Shaders_UIntVertexAttribs_Info
{
    uint ColorNormalizationFactor;
    float padding0;
    float padding1;
    float padding2;
};


vec4 FS( Veldrid_Tests_Shaders_UIntVertexAttribs_FragmentInput input_)
{
    return input_.Color;
}


layout(location = 0) in vec4 fsin_0;
layout(location = 0) out vec4 _outputColor_;

void main()
{
    Veldrid_Tests_Shaders_UIntVertexAttribs_FragmentInput input_;
    input_.Position = gl_FragCoord;
    input_.Color = fsin_0;
    vec4 output_ = FS(input_);
    _outputColor_ = output_;
}
