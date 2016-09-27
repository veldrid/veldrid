#version 140

in vec3 in_position;
out vec4 out_color;

void main()
{
    // Pass-through position
	gl_Position = vec4(in_position, 0);
    out_color = vec4(1, 0, 0, 1); // Set by geometry shader.
}
