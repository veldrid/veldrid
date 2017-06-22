#version 330 core

uniform ProjectionMatrixBuffer
{
    mat4 projection;
};

uniform ViewMatrixBuffer
{
    mat4 view;
};

uniform CameraInfoBuffer
{
    vec3 cameraWorldPosition;
    float _unused1;
    vec3 cameraLookDirection;
    float _unused2;
};

uniform WorldMatrixBuffer
{
    mat4 world;
};

layout (points) in;
layout (triangle_strip, max_vertices = 24) out;

in vec4 in_color[];

out vec4 out_color;

const vec4 cubePositions[8] = vec4[8]
(
    vec4(-.5, .5, .5, 1),
    vec4(.5, .5, .5, 1),
    vec4(.5, -.5, .5, 1),
    vec4(-.5, -.5, .5, 1),
    vec4(-.5, .5, -.5, 1),
    vec4(.5, .5, -.5, 1),
    vec4(.5, -.5, -.5, 1),
    vec4(-.5, -.5, -.5, 1)
);

const int cubeIndices[24] = int[24]
(
    0, 1, 3, 2, // front
    5, 4, 6, 7, // back
    4, 0, 7, 3, // left
    1, 5, 2, 6, // right
    4, 5, 0, 1, // top
    3, 2, 7, 6 // bottom
);

void main()
{
    vec4 center = gl_in[0].gl_Position;
    float step = (1.0 / 24.0);
    float g = 1.0 / 24.0;
    for (int i = 0; i < 24; i += 4)
    {
        gl_Position = projection * view * world * (center + cubePositions[cubeIndices[i]]);
        // Normalize depth range
        gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;
        out_color = vec4(1, g, 1, 1);
        EmitVertex();
        g += step;
        gl_Position = projection * view * world * (center + cubePositions[cubeIndices[i + 1]]);
        // Normalize depth range
        gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;
        out_color = vec4(1, g, 1, 1);
        EmitVertex();
        g += step;
        gl_Position = projection * view * world * (center + cubePositions[cubeIndices[i + 2]]);
        // Normalize depth range
        gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;
        out_color = vec4(1, g, 1, 1);
        EmitVertex();
        g += step;
        gl_Position = projection * view * world * (center + cubePositions[cubeIndices[i + 3]]);
        // Normalize depth range
        gl_Position.z = gl_Position.z * 2.0 - gl_Position.w;
        out_color = vec4(1, g, 1, 1);
        EmitVertex();
        g += step;

        EndPrimitive();
    }
}