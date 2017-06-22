#version 330 core

uniform ProjectionMatrixBuffer
{
    mat4 projection;
};

uniform ViewMatrixBuffer
{
    mat4 view;
};

uniform LightProjectionMatrixBuffer
{
    mat4 lightProjection;
};

uniform LightViewMatrixBuffer
{
    mat4 lightView;
};

uniform WorldMatrixBuffer
{
    mat4 world;
};

uniform InverseTransposeWorldMatrixBuffer
{
    mat4 inverseTransposeWorld;
};

uniform sampler2D SurfaceTexture;
uniform sampler2D ShadowMap;

in vec3 in_position;
in vec3 in_normal;
in vec2 in_texCoord;

out vec3 out_position_worldSpace;
out vec4 out_lightPosition; //vertex with regard to light view
out vec3 out_normal;
out vec2 out_texCoord;

void main()
{
    vec4 worldPosition = world * vec4(in_position, 1);
    vec4 viewPosition = view * worldPosition;
    gl_Position = projection * viewPosition;
    // Normalize depth range
    gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;

    out_position_worldSpace = worldPosition.xyz;

    out_normal = mat3(inverseTransposeWorld) * in_normal;
    out_normal = normalize(out_normal);

    out_texCoord = in_texCoord;

    //store worldspace projected to light clip space with
    //a texcoord semantic to be interpolated across the surface
    out_lightPosition = world * vec4(in_position, 1);
    out_lightPosition = lightView * out_lightPosition;
    out_lightPosition = lightProjection * out_lightPosition;
    // Normalize depth range
    out_lightPosition.z = out_lightPosition.z * 2.0 - out_lightPosition.w;
}