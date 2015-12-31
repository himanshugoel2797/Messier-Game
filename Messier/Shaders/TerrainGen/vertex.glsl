#version 410 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 UV;
layout(location = 2) in vec3 normal;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;

out vec2 texCoordPS_in;
out vec3 normalPS_in;

void main()
{
	gl_Position = vec4(position, 1);
	normalPS_in = normal;
	texCoordPS_in = UV;
}