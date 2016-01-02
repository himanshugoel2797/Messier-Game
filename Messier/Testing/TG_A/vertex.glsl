#version 410 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 UV;
layout(location = 2) in vec3 norm;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;

out vec2 texCoordCS_in;
out vec3 worldCS_in;
out vec3 normalCS_in;

void main()
{
	worldCS_in = (World * vec4(position, 1)).xyz;
	normalCS_in = norm;
	texCoordCS_in = UV;
}