#version 330 core 

out float fragmentdepth;

void main()
{
    fragmentdepth = gl_FragCoord.z;
}