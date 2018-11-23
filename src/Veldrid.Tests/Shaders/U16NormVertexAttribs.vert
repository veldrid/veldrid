#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable
struct Veldrid_Tests_Shaders_U16NormVertexAttribs_VertexGPU
{
    vec2 Position;
    vec4 Color;
};

struct Veldrid_Tests_Shaders_U16NormVertexAttribs_FragmentInput
{
    vec4 Position;
    vec4 Color;
};

layout(set = 0, binding = 0) uniform Ortho
{
    mat4 field_Ortho;
};


Veldrid_Tests_Shaders_U16NormVertexAttribs_FragmentInput VS( Veldrid_Tests_Shaders_U16NormVertexAttribs_VertexGPU input_)
{
    Veldrid_Tests_Shaders_U16NormVertexAttribs_FragmentInput output_;
    output_.Position = field_Ortho * vec4(input_.Position, 0, 1);
    output_.Color = input_.Color;
    output_.Color.w = 1;
    return output_;
}


layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;
layout(location = 0) out vec4 fsin_0;

void main()
{
    Veldrid_Tests_Shaders_U16NormVertexAttribs_VertexGPU input_;
    input_.Position = Position;
    input_.Color = Color;
    Veldrid_Tests_Shaders_U16NormVertexAttribs_FragmentInput output_ = VS(input_);
    fsin_0 = output_.Color;
    gl_Position = output_.Position;
    gl_PointSize = 1;
}
