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

layout(set = 0, binding = 0) uniform Ortho
{
    mat4 field_Ortho;
};


Veldrid_Tests_Shaders_TexturedPoints_FragmentInput VS( Veldrid_Tests_Shaders_TexturedPoints_Vertex input_)
{
    Veldrid_Tests_Shaders_TexturedPoints_FragmentInput output_;
    output_.Position = field_Ortho * vec4(input_.Position, 0, 1);
    return output_;
}


layout(location = 0) in vec2 Position;

void main()
{
    Veldrid_Tests_Shaders_TexturedPoints_Vertex input_;
    input_.Position = Position;
    Veldrid_Tests_Shaders_TexturedPoints_FragmentInput output_ = VS(input_);
    gl_Position = output_.Position;
    gl_PointSize = 1;
}
