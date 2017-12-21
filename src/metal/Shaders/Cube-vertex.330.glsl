#version 330 core

struct TexturedCube_Shaders_Cube_VertexInput
{
    vec3 Position;
    vec2 TexCoords;
};

struct TexturedCube_Shaders_Cube_FragmentInput
{
    vec4 SystemPosition;
    vec2 TexCoords;
};

uniform Projection
{
    mat4 field_Projection;
};

uniform View
{
    mat4 field_View;
};

uniform World
{
    mat4 field_World;
};

uniform sampler2D SurfaceTexture;

TexturedCube_Shaders_Cube_FragmentInput VS(TexturedCube_Shaders_Cube_VertexInput input_)
{
    TexturedCube_Shaders_Cube_FragmentInput output_;
    vec4 worldPosition = field_World * vec4(input_.Position, 1);
    vec4 viewPosition = field_View * worldPosition;
    vec4 clipPosition = field_Projection * viewPosition;
    output_.SystemPosition =clipPosition;
    output_.TexCoords =input_.TexCoords;
    return output_;
}


in vec3 Position;
in vec2 TexCoords;
out vec2 fsin_0;

void main()
{
    TexturedCube_Shaders_Cube_VertexInput input_;
    input_.Position = Position;
    input_.TexCoords = TexCoords;
    TexturedCube_Shaders_Cube_FragmentInput output_ = VS(input_);
    fsin_0 = output_.TexCoords;
    gl_Position = output_.SystemPosition;
        gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;
}
