#version 140 

out float fragmentdepth;

void main()
{
    fragmentdepth = gl_FragCoord.z;
}