#version 410 core

layout(location = 0) in vec3 position;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;

out vec2 texCoordCS_in;
out vec3 worldCS_in;
out vec3 normalCS_in;

void main()
{
	worldCS_in = position;
	normalCS_in = vec3(0, 1, 0);
	texCoordCS_in = position.xy;
}