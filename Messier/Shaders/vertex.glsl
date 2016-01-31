#version 410 core

layout(location = 0) in vec3 position;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;

out vec2 UV;

void main()
{
	mat4 WVP = Proj * View * World;
	UV = position.xy;
	gl_Position = WVP * vec4(position, 1);
}