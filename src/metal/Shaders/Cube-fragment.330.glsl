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

vec4 FS(TexturedCube_Shaders_Cube_FragmentInput input_)
{
    return texture(SurfaceTexture, input_.TexCoords);
}


in vec2 fsin_0;
out vec4 _outputColor_;

void main()
{
    TexturedCube_Shaders_Cube_FragmentInput input_;
    input_.SystemPosition = gl_FragCoord;
    input_.TexCoords = fsin_0;
    vec4 output_ = FS(input_);
    _outputColor_ = output_;
}
