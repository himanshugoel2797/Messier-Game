#version 410 core

layout(location = 0) in vec4 position;
layout(location = 1) in vec4 norm;
uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;
out vec3 worldCS_in;
out vec3 normalCS_in;
void main()
{
    worldCS_in = (vec4(position.xyz, 1)).xyz;
    normalCS_in = norm.xyz;
}

