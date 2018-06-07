#version 450

layout(set = 0, binding = 0) uniform Projection
{
    mat4 _Proj;
};

layout(set = 0, binding = 1) uniform View
{
    mat4 _View;
};

layout(location = 0) in vec3 vsin_Position;
layout(location = 0) out vec3 fsin_0;

layout(constant_id = 102) const bool ReverseDepthRange = true;

void main()
{
    mat4 view3x3 = mat4(
        _View[0][0], _View[0][1], _View[0][2], 0,
        _View[1][0], _View[1][1], _View[1][2], 0,
        _View[2][0], _View[2][1], _View[2][2], 0,
        0, 0, 0, 1);
    vec4 pos = _Proj * view3x3 * vec4(vsin_Position, 1.0f);
    gl_Position = vec4(pos.x, pos.y, pos.w, pos.w);
    if (ReverseDepthRange) { gl_Position.z = 0; }
    fsin_0 = vsin_Position;
}
