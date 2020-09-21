#version 450

layout(location = 0) out vec2 fsin_UV;

void main()
{
    float x = -1.0 + float((gl_VertexIndex & 1) << 2);
    float y = -1.0 + float((gl_VertexIndex & 2) << 1);
    fsin_UV.x = (x + 1.0) * 0.5;
    fsin_UV.y = (y + 1.0) * 0.5;
    gl_Position = vec4(x, y, 0, 1);
}
