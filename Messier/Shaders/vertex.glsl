#version 410 core

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 uv_a;

uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;

out vec2 UV;

void main()
{
	mat4 WVP = Proj * View * World;
	UV = uv_a;
	gl_Position = WVP * vec4(position, 1);
}