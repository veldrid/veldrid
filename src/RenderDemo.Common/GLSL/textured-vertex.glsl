#version 330 core

uniform ProjectionMatrixBuffer
{
    mat4 projection_matrix;
};

uniform ViewMatrixBuffer
{
    mat4 view_matrix;
};

uniform WorldMatrixBuffer
{
    mat4 world_matrix;
};

uniform InverseTransposeWorldMatrixBuffer
{
    mat4 inverseTransposeWorldMatrix;
};

uniform sampler2D surfaceTexture;

in vec3 in_position;
in vec3 in_normal;
in vec2 in_texCoord;

out vec3 normal;
out vec2 texCoord;

void main()
{
    vec4 worldPos = world_matrix * vec4(in_position, 1);
    vec4 viewPos = view_matrix * worldPos;
    vec4 screenPos = projection_matrix * viewPos;
    gl_Position = screenPos;

    // Normalize depth range
    gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;

    texCoord = in_texCoord; // Pass along unchanged.

    normal = normalize(mat3(inverseTransposeWorldMatrix) * in_normal);
}
