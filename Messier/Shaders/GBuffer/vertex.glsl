#version 430 core

// Input vertex data, different for all executions of this shader.
layout(location = 0) in vec3 position;
layout(location = 1) in vec2 vertexUV;
layout(location = 2) in vec3 normal;

// Output data
out vec2 UV;
out float depth;
out vec3 worldXY;
smooth out vec3 normPos;


//Uniforms
uniform mat4 World;
uniform mat4 View;
uniform mat4 Proj;

void main()
{
	mat4 WVP = Proj * View * World;

	gl_Position = WVP * vec4(position, 1);

	normPos = (World * vec4(normal, 0)).xyz;
	worldXY = (World * vec4(position, 1)).xyz;
	UV = vertexUV;
}
